using BirthdayBot.DAL.Entities;
using BirthdayBot.DAL.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using RapidBots;
using RapidBots.Types.Core;
using System;
using System.Collections.Generic;
using System.Globalization;
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
            var requestScope = serviceProvider.CreateScope();

            var repository = requestScope.ServiceProvider.GetService<IRepository>();
            var options = requestScope.ServiceProvider.GetService<RapidBotsOptions>();
            try
            {
                // Get current user
                var dbUser = await repository.GetAsync<TUser>(true, u => u.Id == (update.CallbackQuery?.From?.Id ?? update.Message?.From?.Id));
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
                    await actionsManager.Commands[commandKey].Execute(update, user: dbUser, actionScope: requestScope);
                }
                catch (KeyNotFoundException)
                {
                    if (dbUser?.CurrentStatus != null)
                    {
                        await actionsManager.Inputs[(int)dbUser.CurrentStatus].Execute(update, dbUser, requestScope);
                    }
                }

                return Ok();
            }
            catch (Exception exception)
            {
                return Ok(exception.Message);
            }
            finally
            {
                requestScope.Dispose();
            }
        }
    }
}
