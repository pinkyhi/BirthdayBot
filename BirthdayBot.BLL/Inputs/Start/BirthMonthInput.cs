using BirthdayBot.Core.Resources;
using BirthdayBot.DAL.Entities;
using BirthdayBot.DAL.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using RapidBots.Types.Core;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace BirthdayBot.BLL.Inputs.Start
{
    class BirthMonthInput : IInput
    {
        private readonly BotClient botClient;

        public BirthMonthInput(BotClient botClient)
        {
            this.botClient = botClient;
        }

        public int Status => 1;

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
                List<string> monthsStr = new List<string>()
                {
                    resources["JANUARY"],
                    resources["FEBRUARY"],
                    resources["MARCH"],
                    resources["APRIL"],
                    resources["MAY"],
                    resources["JUNE"],
                    resources["JULY"],
                    resources["AUGUST"],
                    resources["SEPTEMBER"],
                    resources["OCTOBER"],
                    resources["NOVEMBER"],
                    resources["DECEMBER"]
                };

                int month = monthsStr.IndexOf(update.Message.Text.Trim());
                if(month == -1)
                {
                    throw new ArgumentException();
                }

                month++;

                // Change status
                dbUser.MiddlewareData = Convert.ToDateTime(dbUser.MiddlewareData).AddMonths(month - dbUser.BirthDate.Month).ToString();
                dbUser.CurrentStatus = actionsManager.FindInputStatusByType<BirthDayInput>();
                await repository.UpdateAsync(dbUser);
            }
            catch
            {
                await botClient.SendTextMessageAsync(update.Message.Chat.Id, resources["BIRTH_MONTH_INPUT_ERROR"], parseMode: Telegram.Bot.Types.Enums.ParseMode.Markdown);
                return;
            }

            // Output
            await botClient.SendTextMessageAsync(update.Message.Chat.Id, resources["BIRTH_DAY_INPUT"], parseMode: Telegram.Bot.Types.Enums.ParseMode.Markdown, replyMarkup: new ReplyKeyboardRemove() { Selective = false });
        }
    }
}
