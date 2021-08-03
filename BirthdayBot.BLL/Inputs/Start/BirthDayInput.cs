using BirthdayBot.BLL.Menus;
using BirthdayBot.Core.Resources;
using BirthdayBot.DAL.Entities;
using BirthdayBot.DAL.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using RapidBots.Types.Core;
using System;
using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace BirthdayBot.BLL.Inputs.Start
{
    class BirthDayInput : IInput
    {
        private readonly BotClient botClient;

        public BirthDayInput(BotClient botClient)
        {
            this.botClient = botClient;
        }

        public int Status => 2;

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
                int day = Convert.ToInt32(update.Message.Text.Trim());

                if (day < 0 || day > DateTime.DaysInMonth(dbUser.BirthDate.Year, dbUser.BirthDate.Month))
                {
                    throw new ArgumentException();
                }

                // Change status
                dbUser.MiddlewareData = Convert.ToDateTime(dbUser.MiddlewareData).AddDays(day - dbUser.BirthDate.Day).ToString();
                await repository.UpdateAsync(dbUser);
            }
            catch
            {
                await botClient.SendTextMessageAsync(update.Message.Chat.Id, resources["BIRTH_DAY_INPUT_ERROR"], parseMode: Telegram.Bot.Types.Enums.ParseMode.Markdown);
                return;
            }

            BirthDateConfirmationMenu menu = new BirthDateConfirmationMenu(resources);

            // Output
            await botClient.SendTextMessageAsync(update.Message.Chat.Id, menu.GetDefaultTitle(null, Convert.ToDateTime(dbUser.MiddlewareData).ToShortDateString()), parseMode: Telegram.Bot.Types.Enums.ParseMode.Markdown, replyMarkup: menu.GetMarkup());
        }
    }
}
