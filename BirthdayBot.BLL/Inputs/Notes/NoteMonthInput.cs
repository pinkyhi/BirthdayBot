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
using System.Collections.Generic;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using RapidBots.Types.Attributes;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace BirthdayBot.BLL.Inputs.Notes
{
    [ChatType(ChatType.Private)]
    public class NoteMonthInput : Input
    {
        private readonly BotClient botClient;

        public NoteMonthInput(BotClient botClient)
        {
            this.botClient = botClient;
        }

        public override int Status => 8;

        public override async Task Execute(Update update, TelegramUser user = null, IServiceScope actionScope = null)
        {
            //Initialisation
            var repository = actionScope.ServiceProvider.GetService<IRepository>();
            var actionsManager = actionScope.ServiceProvider.GetService<ActionManager>();
            var resources = actionScope.ServiceProvider.GetService<IStringLocalizer<SharedResources>>();
            TUser dbUser = user as TUser ?? await repository.GetAsync<TUser>(true, u => u.Id == update.Message.From.Id);

            // Logic
            try
            {
                string inputStr = update.Message.Text.Trim();
                if (inputStr.Equals(resources["BACK_BUTTON"]))
                {
                    var openerMessage = await botClient.SendTextMessageAsync(update.Message?.Chat?.Id ?? update.CallbackQuery.Message.Chat.Id, resources["MENU_OPENER_TEXT"], replyMarkup: new ReplyKeyboardRemove(), parseMode: Telegram.Bot.Types.Enums.ParseMode.Html);
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
                    await botClient.SendTextMessageAsync(update.Message.Chat.Id, notesMenu.GetDefaultTitle(actionScope), replyMarkup: notesMenu.GetMarkup(0, dbUser.Notes, actionScope), parseMode: Telegram.Bot.Types.Enums.ParseMode.Html);

                    return;
                }

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

                int month = monthsStr.IndexOf(inputStr);
                if (month == -1)
                {
                    throw new ArgumentException();
                }

                month++;

                // Change status
                var note = JsonConvert.DeserializeObject<Note>(dbUser.MiddlewareData);
                note.Date = note.Date.AddMonths(month - note.Date.Month);
                dbUser.MiddlewareData = JsonConvert.SerializeObject(note);
                dbUser.CurrentStatus = actionsManager.FindInputStatusByType<NoteDayInput>();
                await repository.UpdateAsync(dbUser);
            }
            catch
            {
                await botClient.SendTextMessageAsync(update.Message.Chat.Id, resources["BIRTH_MONTH_INPUT_ERROR"], parseMode: Telegram.Bot.Types.Enums.ParseMode.Html);
                return;
            }

            // Output

            await botClient.SendTextMessageAsync(update.Message.Chat.Id, resources["NOTE_DAY_INPUT"], parseMode: Telegram.Bot.Types.Enums.ParseMode.Html, replyMarkup: new ReplyKeyboardMarkup(new KeyboardButton() { Text = resources["BACK_BUTTON"] }) { ResizeKeyboard = true });       
        }
    }
}
