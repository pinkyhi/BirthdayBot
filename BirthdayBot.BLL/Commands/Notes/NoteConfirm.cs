using BirthdayBot.BLL.Menus.Notes;
using BirthdayBot.BLL.Resources;
using BirthdayBot.Core.Resources;
using BirthdayBot.Core.Types;
using BirthdayBot.DAL.Entities;
using BirthdayBot.DAL.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using Newtonsoft.Json;
using RapidBots.Types.Core;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using RapidBots.Types.Attributes;
using Telegram.Bot.Types.Enums;
using BirthdayBot.Core.Const;

namespace BirthdayBot.BLL.Commands.Notes
{
    [ChatType(ChatType.Private)]
    public class NoteConfirm : Command
    {
        private readonly BotClient botClient;
        private readonly ClientSettings clientSettings;

        public NoteConfirm(BotClient botClient, ClientSettings clientSetting)
        {
            this.clientSettings = clientSetting;
            this.botClient = botClient;
        }

        public override string Key => CommandKeys.NoteDateConfirm;

        public override async Task Execute(Update update, TelegramUser user = null, IServiceScope actionScope = null)
        {
            var repository = actionScope.ServiceProvider.GetService<IRepository>();
            var actionsManager = actionScope.ServiceProvider.GetService<ActionManager>();
            var resources = actionScope.ServiceProvider.GetService<IStringLocalizer<SharedResources>>();

            TUser dbUser = (user as TUser) ?? await repository.GetAsync<TUser>(false, u => u.Id == update.CallbackQuery.From.Id, include: u => u.Include(x => x.Notes));

            if (dbUser?.Notes == null)
            {
                await repository.LoadCollectionAsync(dbUser, x => x.Notes);
            }
            var note = JsonConvert.DeserializeObject<Note>(dbUser.MiddlewareData);
            if(dbUser.Notes.Count < Limitations.NotesLimit)
            {
                dbUser.Notes.Add(note);
            }
            else
            {
                await botClient.AnswerCallbackQueryAsync(update.CallbackQuery.Id, text: resources["NOTES_LIMIT"], showAlert: true);
            }
            dbUser.MiddlewareData = null;
            dbUser.CurrentStatus = null;
            await repository.UpdateAsync(dbUser);

            try{await botClient.AnswerCallbackQueryAsync(update.CallbackQuery.Id);}catch{}
            try
            {
                await botClient.DeleteMessageAsync(update.CallbackQuery.Message.Chat.Id, update.CallbackQuery.Message.MessageId);
            }
            catch
            { }

            int page = 0;

            NoteMenu menu = new NoteMenu(resources, page, note);

            try{await botClient.AnswerCallbackQueryAsync(update.CallbackQuery.Id);}catch{}
            try
            {
                await botClient.DeleteMessageAsync(update.CallbackQuery.Message.Chat.Id, update.CallbackQuery.Message.MessageId);
            }
            catch
            { }
            await botClient.SendTextMessageAsync(update.CallbackQuery.Message.Chat.Id, menu.GetDefaultTitle(actionScope), replyMarkup: menu.GetMarkup(actionScope), parseMode: Telegram.Bot.Types.Enums.ParseMode.Html);
        }
    }
}
