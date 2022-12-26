using BirthdayBot.DAL.Entities;
using BirthdayBot.DAL.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using RapidBots;
using RapidBots.Extensions;
using RapidBots.Types.Core;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace BirthdayBot.Controllers
{
    public class HomeController : ControllerBase
    {
        private readonly ActionManager actionsManager;
        private readonly IServiceProvider serviceProvider;
        private readonly ILogger<HomeController> logger;
        private readonly IRepository repository;
        private readonly BotClient botClient;

        public HomeController(ActionManager actionsManager, IServiceProvider serviceProvider, ILogger<HomeController> logger, IRepository repository, BotClient botClient)
        {

            this.actionsManager = actionsManager;
            this.serviceProvider = serviceProvider;
            this.logger = logger;
            this.repository = repository;
            this.botClient = botClient;
        }

        [HttpGet]
        [Route("/")]
        public async Task<IActionResult> Get()
        {
            logger.LogDebug("GET request");
            var users = await repository.GetRangeAsync<TUser>(false, x => x.RegistrationDate != null);
            var chats = await repository.GetRangeAsync<DAL.Entities.Chat>(false, x => true);
            var hookInfo = await botClient.GetWebhookInfoAsync();
            return Ok($"Users count: {users.Count()}\nChats count: {chats.Count()}\nHook info:{JsonConvert.SerializeObject(hookInfo, Formatting.Indented)}");
        }

        [HttpPost]
        [Route("/")]
        public async Task<IActionResult> Post([FromBody] Update update)
        {
            // Restrctions
            var chatType = update.GetChatType();
            if(chatType == null || chatType == ChatType.Channel || chatType==ChatType.Sender)
            {
                return Ok();
            }
            if(chatType == ChatType.Group || chatType == ChatType.Supergroup)
            {
                if(update.Type == UpdateType.EditedMessage)
                {
                    return Ok();
                }
                else if (update.Type == UpdateType.Message)
                {
                    if(update.Message.Text?.Trim().StartsWith('/') == false)
                    {
                        return Ok();
                    }
                }
            }

            var requestScope = serviceProvider.CreateScope();

            var repository = requestScope.ServiceProvider.GetService<IRepository>();
            var options = requestScope.ServiceProvider.GetService<RapidBotsOptions>();
            try
            {
                // Get current user
                TUser dbUser = null;
                try
                {
                    if((update.CallbackQuery?.From?.Id ?? update.Message?.From?.Id) != null)
                    {
                        dbUser = await repository.GetAsync<TUser>(false, u => u.Id == (update.CallbackQuery?.From?.Id ?? update.Message?.From?.Id));
                    }
                }
                catch { }
                // Setting culture info for request
                string telegramUserLanguageCode = update.CallbackQuery?.From?.LanguageCode ?? update.Message?.From?.LanguageCode;
                CultureInfo.CurrentCulture = new CultureInfo(dbUser?.LanguageCode ?? telegramUserLanguageCode ?? options.DefaultLanguageCode);
                CultureInfo.CurrentUICulture = new CultureInfo(dbUser?.LanguageCode ?? telegramUserLanguageCode ?? options.DefaultLanguageCode);

                // Find right command
                string commandKey = string.Empty;
                if (update.Type == UpdateType.CallbackQuery)
                {
                    commandKey = actionsManager.GetCommandKey(update.CallbackQuery.Data);
                }
                else if (update.Type == UpdateType.Message)
                {
                    if (!string.IsNullOrEmpty(update.Message.Text))
                    {
                        commandKey = actionsManager.GetCommandKey(update.Message.Text, "yourdate_bot");
                    }
                }
                try
                {
                    var command = actionsManager.Commands.FirstOrDefault(p => p.Key.Equals(commandKey) && p.ValidateUpdate(update));
                    if (command == null)
                    {
                        throw new KeyNotFoundException();
                    }
                    else
                    {
                        await command.Execute(update, actionScope: requestScope);
                    }
                }
                catch (KeyNotFoundException)
                {
                    try
                    {
                        if (dbUser?.CurrentStatus != null)
                        {
                            var input = actionsManager.Inputs[(int)dbUser.CurrentStatus];
                            if (input.ValidateUpdate(update))
                            {
                                await input.Execute(update, actionScope: requestScope);
                            }
                            else
                            {
                                throw new KeyNotFoundException();
                            }
                        }
                        else
                        {
                            throw new KeyNotFoundException();
                        }
                    }
                    catch (KeyNotFoundException)
                    {
                        var action = actionsManager.Actions.First(x => x.ValidateUpdate(update));
                        await action.Execute(update, actionScope: requestScope);
                    }
                }
                return Ok();
            }
            catch (Exception exception)
            {
                logger.LogError(exception.Message + " " + exception.GetType());
                return Ok(exception.Message + " " + exception.GetType());
            }
            finally
            {
                requestScope.Dispose();
            }
        }
    }
}