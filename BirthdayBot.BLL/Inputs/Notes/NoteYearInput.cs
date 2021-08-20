using BirthdayBot.BLL.Menus;
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
using RapidBots.Types.Attributes;
using Telegram.Bot.Types.Enums;

namespace BirthdayBot.BLL.Inputs.Notes
{
    [ChatType(ChatType.Private)]
    public class NoteYearInput : Input
    {
        private readonly BotClient botClient;

        public NoteYearInput(BotClient botClient)
        {
            this.botClient = botClient;
        }

        public override int Status => 7;

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

                int year = Convert.ToInt32(inputStr);
                if (year < 1900 || year > DateTime.Now.Year)
                {
                    throw new ArgumentException();
                }

                // Change status
                var note = JsonConvert.DeserializeObject<Note>(dbUser.MiddlewareData);
                note.Date = note.Date.AddYears(year - note.Date.Year);
                dbUser.MiddlewareData = JsonConvert.SerializeObject(note);
                dbUser.CurrentStatus = actionsManager.FindInputStatusByType<NoteMonthInput>();
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
            await botClient.SendTextMessageAsync(update.Message.Chat.Id, resources["NOTE_MONTH_INPUT"], replyMarkup: menu.GetMarkup(), parseMode: Telegram.Bot.Types.Enums.ParseMode.Markdown);
        }
    }
}
