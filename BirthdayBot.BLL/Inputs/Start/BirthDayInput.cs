﻿using BirthdayBot.BLL.Menus;
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
using RapidBots.Types.Attributes;
using Telegram.Bot.Types.Enums;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;

namespace BirthdayBot.BLL.Inputs.Start
{
    [ChatType(ChatType.Private)]
    public class BirthDayInput : Input
    {
        private readonly BotClient botClient;

        public BirthDayInput(BotClient botClient)
        {
            this.botClient = botClient;
        }

        public override int Status => 2;

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

            DateTime date = new DateTime();
            // Logic
            try
            {
                int day = Convert.ToInt32(update.Message.Text.Trim());

                if (day < 1 || day > DateTime.DaysInMonth(dbUser.BirthDate.Year, dbUser.BirthDate.Month))
                {
                    throw new ArgumentException();
                }

                // Change status
                try
                {
                    var data = JsonConvert.DeserializeObject<Dictionary<string, string>>(dbUser.MiddlewareData);
                    date = DateTime.Parse(data["date"]);
                    date = date.AddDays(day - dbUser.BirthDate.Day);
                    data["date"] = date.ToString();
                    dbUser.MiddlewareData = JsonConvert.SerializeObject(data);
                }
                catch
                {
                    date = Convert.ToDateTime(dbUser.MiddlewareData).AddDays(day - dbUser.BirthDate.Day);
                    dbUser.MiddlewareData = date.ToString();
                }
                dbUser.CurrentStatus = null;
                await repository.UpdateAsync(dbUser);
            }
            catch
            {
                await botClient.SendTextMessageAsync(update.Message.Chat.Id, resources["BIRTH_DAY_INPUT_ERROR"], parseMode: Telegram.Bot.Types.Enums.ParseMode.Html);
                return;
            }

            BirthDateConfirmationMenu menu = new BirthDateConfirmationMenu(resources);

            // Output
            if(dbUser.RegistrationDate != null)
            {
                var removeMessage = await botClient.SendTextMessageAsync(update.Message.Chat.Id, resources["REPLY_KEYBOARD_REMOVE_TEXT"], replyMarkup: new ReplyKeyboardRemove(), parseMode: Telegram.Bot.Types.Enums.ParseMode.Html, disableNotification: true);
                try { await botClient.DeleteMessageAsync(removeMessage.Chat.Id, removeMessage.MessageId); } catch { }
            }
            await botClient.SendTextMessageAsync(update.Message.Chat.Id, menu.GetDefaultTitle(null, date.ToShortDateString()), parseMode: Telegram.Bot.Types.Enums.ParseMode.Html, replyMarkup: menu.GetMarkup());
        }
    }
}
