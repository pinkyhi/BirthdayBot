using BirthdayBot.BLL.Inputs.Start;
using BirthdayBot.BLL.Menus.Notes;
using BirthdayBot.BLL.Menus.Settings;
using BirthdayBot.Core.Enums;
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
    public class NoteTitleInput : IInput
    {
        private readonly BotClient botClient;

        public NoteTitleInput(BotClient botClient)
        {
            this.botClient = botClient;
        }

        public int Status => 6;

        public async Task Execute(Update update, TelegramUser user = null, IServiceScope actionScope = null)
        {
            //Initialisation
            var repository = actionScope.ServiceProvider.GetService<IRepository>();
            var actionsManager = actionScope.ServiceProvider.GetService<ActionManager>();
            var resources = actionScope.ServiceProvider.GetService<IStringLocalizer<SharedResources>>();
            TUser dbUser = user as TUser ?? await repository.GetAsync<TUser>(false, u => u.Id == update.Message.From.Id);

            // Logic
            try
            {
                string titleStr = update.Message.Text.Trim();
                if (titleStr.Equals(resources["BACK_BUTTON"]))
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

                    NotesMenu menu = new NotesMenu(resources);
                    await botClient.SendTextMessageAsync(update.Message.Chat.Id, menu.GetDefaultTitle(actionScope), replyMarkup: menu.GetMarkup(0, dbUser.Notes, actionScope));

                    return;
                }

                if (titleStr.Length > 32)
                {
                    titleStr = titleStr.Substring(0, 32);
                }

                var note = JsonConvert.DeserializeObject<Note>(dbUser.MiddlewareData);
                note.Title = titleStr;
                dbUser.MiddlewareData = JsonConvert.SerializeObject(note);
                // Change status
                dbUser.CurrentStatus = actionsManager.FindInputStatusByType<NoteYearInput>();
                await repository.UpdateAsync(dbUser);
            }
            catch
            {
                await botClient.SendTextMessageAsync(update.Message.Chat.Id, resources["NOTE_TITLE_INPUT_ERROR"], parseMode: Telegram.Bot.Types.Enums.ParseMode.Markdown);
                return;
            }

            // Output
            KeyboardButton backBut = new KeyboardButton() { Text = resources["BACK_BUTTON"] };

            await repository.UpdateAsync(dbUser);
            await botClient.SendTextMessageAsync(update.Message.Chat.Id, resources["NOTE_YEAR_INPUT"], replyMarkup: new ReplyKeyboardMarkup(backBut) { ResizeKeyboard = true });
        }
    }
}
