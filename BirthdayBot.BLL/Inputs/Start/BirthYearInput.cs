using BirthdayBot.BLL.Menus;
using BirthdayBot.BLL.Menus.Settings;
using BirthdayBot.Core.Resources;
using BirthdayBot.DAL.Entities;
using BirthdayBot.DAL.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using RapidBots.Types.Core;
using System;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

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

            if (dbUser.RegistrationDate != null && update.Message.Text.Trim().Equals(resources["BACK_BUTTON"]))
            {
                dbUser.CurrentStatus = null;
                dbUser.MiddlewareData = null;
                await repository.UpdateAsync(dbUser);

                if (dbUser?.Addresses == null)
                {
                    var tempDbUser = await repository.GetAsync<TUser>(false, u => u.Id == update.Message.From.Id, include: u => u.Include(x => x.Addresses));
                    dbUser.Addresses = tempDbUser.Addresses;
                }
                ProfileSettingsMenu changeMenu = new ProfileSettingsMenu(resources);
                await botClient.SendTextMessageAsync(update.Message.Chat.Id, resources["REPLY_KEYBOARD_REMOVE_TEXT"], replyMarkup: new ReplyKeyboardRemove());
                await botClient.SendTextMessageAsync(update.Message.Chat.Id, changeMenu.GetDefaultTitle(actionScope, dbUser.BirthDate.ToShortDateString(), dbUser.Addresses[0].Formatted_Address), replyMarkup: changeMenu.GetMarkup(actionScope));

                return;
            }

            // Logic
            try
            {
                int year = Convert.ToInt32(update.Message.Text.Trim());
                if (year < 1900 || year > DateTime.Now.Year)
                {
                    throw new ArgumentException();
                }

                // Change status
                dbUser.MiddlewareData = dbUser.BirthDate.AddYears(year - dbUser.BirthDate.Year).ToString();
                dbUser.CurrentStatus = actionsManager.FindInputStatusByType<BirthMonthInput>();
                await repository.UpdateAsync(dbUser);
            }
            catch
            {
                await botClient.SendTextMessageAsync(update.Message.Chat.Id, resources["BIRTH_YEAR_INPUT_ERROR"], parseMode: Telegram.Bot.Types.Enums.ParseMode.Markdown);
                return;
            }

            MonthSelectMenu menu = null;
            if (dbUser.RegistrationDate == null)
            {
                menu = new MonthSelectMenu(resources, false);
            }
            else
            {
                menu = new MonthSelectMenu(resources, true);
            }

            // Output
            await botClient.SendTextMessageAsync(update.Message.Chat.Id, menu.GetDefaultTitle(), replyMarkup: menu.GetMarkup(), parseMode: Telegram.Bot.Types.Enums.ParseMode.Markdown);
        }
    }
}
