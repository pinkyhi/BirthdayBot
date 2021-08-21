using BirthdayBot.BLL.Menus.Notes;
using BirthdayBot.BLL.Menus.Settings;
using BirthdayBot.BLL.Resources;
using BirthdayBot.Core.Resources;
using BirthdayBot.DAL.Entities;
using BirthdayBot.DAL.Interfaces;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using RapidBots.Types.Attributes;
using Telegram.Bot.Types.Enums;
using RapidBots.Constants;
using RapidBots.Types.Core;
using System;
using System.Linq;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using RapidBots.Extensions;

namespace BirthdayBot.BLL.Commands.Notes
{
    [ChatType(ChatType.Private)]
    [ExpectedParams("property", CallbackParams.Page)]
    public class ChangeNoteType : Command
    {
        private readonly BotClient botClient;

        public ChangeNoteType(BotClient botClient)
        {
            this.botClient = botClient;
        }

        public override string Key => CommandKeys.ChangeNoteType;

        public override async Task Execute(Update update, TelegramUser user = null, IServiceScope actionScope = null)
        {
            var resources = actionScope.ServiceProvider.GetService<IStringLocalizer<SharedResources>>();
            var repository = actionScope.ServiceProvider.GetService<IRepository>();

            TUser dbUser = (user as TUser) ?? await repository.GetAsync<TUser>(true, u => u.Id == update.CallbackQuery.From.Id, include: u => u.Include(x => x.Notes));

            if (dbUser?.Notes == null)
            {
                await repository.LoadCollectionAsync(dbUser, x => x.Notes);
            }

            long noteId = Convert.ToInt64(update.GetParams()["property"]);
            int page = Convert.ToInt32(update.GetParams()[CallbackParams.Page]);

            var note = dbUser.Notes.First(x => x.Id == noteId);
            if (note != null)
            {
                note.IsStrong = !note.IsStrong;
                await repository.UpdateAsync(dbUser);
            }
            else
            {
                throw new ArgumentException();
            }

            NoteMenu menu = new NoteMenu(resources, page, note);

            try{await botClient.AnswerCallbackQueryAsync(update.CallbackQuery.Id);}catch{}
            try
            {
                await botClient.EditMessageTextAsync(update.CallbackQuery.Message.Chat.Id, update.CallbackQuery.Message.MessageId, menu.GetDefaultTitle(actionScope), replyMarkup: menu.GetMarkup(actionScope) as InlineKeyboardMarkup);
            }
            catch
            {
                try
                {
                    await botClient.DeleteMessageAsync(update.CallbackQuery.Message.Chat.Id, update.CallbackQuery.Message.MessageId);
                }
                catch
                {
                }
                await botClient.SendTextMessageAsync(update.CallbackQuery.Message.Chat.Id, menu.GetDefaultTitle(actionScope), replyMarkup: menu.GetMarkup(actionScope));
            }
        }
    }
}
