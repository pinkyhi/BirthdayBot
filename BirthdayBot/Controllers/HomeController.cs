using BirthdayBot.DAL.Entities;
using BirthdayBot.DAL.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
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

        public HomeController(ActionManager actionsManager, IServiceProvider serviceProvider)
        {

            this.actionsManager = actionsManager;
            this.serviceProvider = serviceProvider;
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
                    dbUser = await repository.GetAsync<TUser>(true, u => u.Id == (update.CallbackQuery?.From?.Id ?? update.Message?.From?.Id));
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
                        commandKey = actionsManager.GetCommandKey(update.Message.Text);
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
                        await command.Execute(update, dbUser, requestScope);
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
                                await input.Execute(update, dbUser, requestScope);
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
                        await action.Execute(update, dbUser, requestScope);
                    }
                }
                return Ok();
            }
            catch (Exception exception)
            {
                return Ok(exception.Message + " " + exception.GetType());
            }
            finally
            {
                requestScope.Dispose();
            }
        }
    }
}
