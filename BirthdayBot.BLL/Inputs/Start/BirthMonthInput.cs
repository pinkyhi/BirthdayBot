using BirthdayBot.BLL.Menus.Settings;
using BirthdayBot.Core.Resources;
using BirthdayBot.DAL.Entities;
using BirthdayBot.DAL.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using RapidBots.Types.Core;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using RapidBots.Types.Attributes;
using Telegram.Bot.Types.Enums;
using Newtonsoft.Json;
using System.Linq;

namespace BirthdayBot.BLL.Inputs.Start
{
    [ChatType(ChatType.Private)]
    public class BirthMonthInput : Input
    {
        private readonly BotClient botClient;

        public BirthMonthInput(BotClient botClient)
        {
            this.botClient = botClient;
        }

        public override int Status => 1;

        public override async Task Execute(Update update, TelegramUser user = null, IServiceScope actionScope = null)
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
                    await repository.LoadCollectionAsync(dbUser, x => x.Addresses);
                }
                ProfileSettingsMenu changeMenu = new ProfileSettingsMenu(resources);
                string fAddress = dbUser.Addresses.FirstOrDefault(x => x.Types.Contains("administrative_area_level_1"))?.Formatted_Address ?? dbUser.Addresses.FirstOrDefault(x => x.Types.Contains("country"))?.Formatted_Address ?? ":)";
                var removeMessage = await botClient.SendTextMessageAsync(update.Message.Chat.Id, resources["REPLY_KEYBOARD_REMOVE_TEXT"], replyMarkup: new ReplyKeyboardRemove(), parseMode: Telegram.Bot.Types.Enums.ParseMode.Html, disableNotification: true);
                try { await botClient.DeleteMessageAsync(removeMessage.Chat.Id, removeMessage.MessageId); } catch { }

                await botClient.SendTextMessageAsync(update.Message.Chat.Id, changeMenu.GetDefaultTitle(actionScope, dbUser.BirthDate.ToShortDateString(), fAddress), replyMarkup: changeMenu.GetMarkup(actionScope), parseMode: Telegram.Bot.Types.Enums.ParseMode.Html);

                return;
            }

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
                try
                {
                    var data = JsonConvert.DeserializeObject<Dictionary<string, string>>(dbUser.MiddlewareData);
                    var date = DateTime.Parse(data["date"]);
                    date = date.AddMonths(month - dbUser.BirthDate.Month);
                    data["date"] = date.ToString();
                    dbUser.MiddlewareData = JsonConvert.SerializeObject(data);
                }
                catch
                {
                    dbUser.MiddlewareData = Convert.ToDateTime(dbUser.MiddlewareData).AddMonths(month - dbUser.BirthDate.Month).ToString();
                }
                dbUser.CurrentStatus = actionsManager.FindInputStatusByType<BirthDayInput>();
                await repository.UpdateAsync(dbUser);
            }
            catch
            {
                await botClient.SendTextMessageAsync(update.Message.Chat.Id, resources["BIRTH_MONTH_INPUT_ERROR"], parseMode: Telegram.Bot.Types.Enums.ParseMode.Html);
                return;
            }

            // Output
            if(dbUser.RegistrationDate == null)
            {
                await botClient.SendTextMessageAsync(update.Message.Chat.Id, resources["BIRTH_DAY_INPUT"], parseMode: Telegram.Bot.Types.Enums.ParseMode.Html, replyMarkup: new ReplyKeyboardRemove() { Selective = false });
            }
            else
            {
                await botClient.SendTextMessageAsync(update.Message.Chat.Id, resources["BIRTH_DAY_INPUT"], parseMode: Telegram.Bot.Types.Enums.ParseMode.Html, replyMarkup: new ReplyKeyboardMarkup(new KeyboardButton() { Text = resources["BACK_BUTTON"] }) { ResizeKeyboard = true });
            }
        }
    }
}
