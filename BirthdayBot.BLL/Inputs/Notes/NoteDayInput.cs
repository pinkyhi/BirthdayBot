﻿using BirthdayBot.BLL.Menus;
using BirthdayBot.BLL.Menus.Notes;
using BirthdayBot.BLL.Menus.Settings;
using BirthdayBot.Core.Resources;
using BirthdayBot.DAL.Entities;
using BirthdayBot.DAL.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using Newtonsoft.Json;
using RapidBots.Types.Core;
using System;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace BirthdayBot.BLL.Inputs.Notes
{
    public class NoteDayInput : IInput
    {
        private readonly BotClient botClient;

        public NoteDayInput(BotClient botClient)
        {
            this.botClient = botClient;
        }

        public int Status => 9;

        public async Task Execute(Update update, TelegramUser user = null, IServiceScope actionScope = null)
        {
            //Initialisation
            var repository = actionScope.ServiceProvider.GetService<IRepository>();
            var actionsManager = actionScope.ServiceProvider.GetService<ActionManager>();
            var resources = actionScope.ServiceProvider.GetService<IStringLocalizer<SharedResources>>();
            TUser dbUser = user as TUser ?? await repository.GetAsync<TUser>(true, u => u.Id == update.Message.From.Id);

            var note = JsonConvert.DeserializeObject<Note>(dbUser.MiddlewareData);

            // Logic
            try
            {
                string inputStr = update.Message.Text.Trim();
                if (inputStr.Equals(resources["BACK_BUTTON"]))
                {
                    var openerMessage = await botClient.SendTextMessageAsync(update.Message?.Chat?.Id ?? update.CallbackQuery.Message.Chat.Id, resources["MENU_OPENER_TEXT"], replyMarkup: new ReplyKeyboardRemove());
                    await botClient.DeleteMessageAsync(openerMessage.Chat.Id, openerMessage.MessageId);

                    dbUser.CurrentStatus = null;
                    dbUser.MiddlewareData = null;
                    await repository.UpdateAsync(dbUser);

                    if (dbUser?.Notes == null)
                    {
                        var tempDbUser = await repository.GetAsync<TUser>(false, u => u.Id == update.Message.From.Id, include: u => u.Include(x => x.Notes));
                        dbUser.Notes = tempDbUser.Notes;
                    }

                    NotesMenu notesMenu = new NotesMenu(resources);
                    await botClient.SendTextMessageAsync(update.Message.Chat.Id, notesMenu.GetDefaultTitle(actionScope), replyMarkup: notesMenu.GetMarkup(0, dbUser.Notes, actionScope));

                    return;
                }
                int day = Convert.ToInt32(inputStr);
                if (day < 0 || day > DateTime.DaysInMonth(note.Date.Year, note.Date.Month))
                {
                    throw new ArgumentException();
                }

                // Change status
                note.Date = note.Date.AddDays(day - note.Date.Day);
                dbUser.MiddlewareData = JsonConvert.SerializeObject(note);
                await repository.UpdateAsync(dbUser);
            }
            catch
            {
                await botClient.SendTextMessageAsync(update.Message.Chat.Id, resources["BIRTH_DAY_INPUT_ERROR"], parseMode: Telegram.Bot.Types.Enums.ParseMode.Markdown);
                return;
            }

            NoteDateConfirmationMenu menu = new NoteDateConfirmationMenu(resources);

            // Output
            await botClient.SendTextMessageAsync(update.Message.Chat.Id, resources["REPLY_KEYBOARD_REMOVE_TEXT"], replyMarkup: new ReplyKeyboardRemove());
            await botClient.SendTextMessageAsync(update.Message.Chat.Id, menu.GetDefaultTitle(null, note.Date.ToShortDateString()), parseMode: Telegram.Bot.Types.Enums.ParseMode.Markdown, replyMarkup: menu.GetMarkup());
        }
    }
}