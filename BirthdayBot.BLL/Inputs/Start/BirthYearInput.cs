using BirthdayBot.BLL.Menus;
using BirthdayBot.Core.Resources;
using BirthdayBot.DAL.Entities;
using BirthdayBot.DAL.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using RapidBots.Types.Core;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace BirthdayBot.BLL.Inputs.Start
{
    public class BirthYearInput : IInput
    {
        private readonly BotClient botClient;

        public BirthYearInput(BotClient botClient)
        {
            this.botClient = botClient;
        }

        public int Status => 0;

        public async Task Execute(Update update, TelegramUser user = null, IServiceScope actionScope = null)
        {
            //Initialisation
            var repository = actionScope.ServiceProvider.GetService<IRepository>();
            var actionsManager = actionScope.ServiceProvider.GetService<ActionManager>();
            var resources = actionScope.ServiceProvider.GetService<IStringLocalizer<SharedResources>>();
            TUser dbUser = user as TUser ?? await repository.GetAsync<TUser>(true, u => u.Id == update.Message.From.Id);

            // Logic
            try
            {
                int year = Convert.ToInt32(update.Message.Text.Trim());
                if (year < 1900 || year > DateTime.Now.Year)
                {
                    throw new ArgumentException();
                }

                // Change status
                dbUser.BirthDate = dbUser.BirthDate.AddYears(year - dbUser.BirthDate.Year);
                dbUser.CurrentStatus = actionsManager.FindInputStatusByType<BirthMonthInput>();
                await repository.UpdateAsync(dbUser);
            }
            catch
            {
                await botClient.SendTextMessageAsync(update.Message.Chat.Id, resources["BIRTH_YEAR_INPUT_ERROR"], parseMode: Telegram.Bot.Types.Enums.ParseMode.Markdown);
                return;
            }

            MonthSelectMenu menu = new MonthSelectMenu(resources);

            // Output
            await botClient.SendTextMessageAsync(update.Message.Chat.Id, menu.GetDefaultTitle(), replyMarkup: menu.GetMarkup(), parseMode: Telegram.Bot.Types.Enums.ParseMode.Markdown);
        }
    }
}
