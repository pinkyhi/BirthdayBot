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
using RapidBots.Constants;
using RapidBots.Types.Core;
using System;
using System.Linq;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace BirthdayBot.BLL.Commands.Notes
{
    public class OpenNote : ICommand
    {
        private readonly BotClient botClient;

        public OpenNote(BotClient botClient)
        {
            this.botClient = botClient;
        }

        public string Key => CommandKeys.OpenNote;

        public async Task Execute(Update update, TelegramUser user = null, IServiceScope actionScope = null)
        {
            var resources = actionScope.ServiceProvider.GetService<IStringLocalizer<SharedResources>>();
            var repository = actionScope.ServiceProvider.GetService<IRepository>();

            TUser dbUser = (user as TUser) ?? await repository.GetAsync<TUser>(false, u => u.Id == update.CallbackQuery.From.Id, include: u => u.Include(x => x.Notes));

            if (dbUser?.Notes == null)
            {
                await repository.LoadCollectionAsync(dbUser, x => x.Notes);
            }

            string queryString = update.CallbackQuery.Data.Substring(update.CallbackQuery.Data.IndexOf('?') + 1);   // Common action

            var parsedQuery = QueryHelpers.ParseNullableQuery(queryString);
            long noteId = Convert.ToInt32(parsedQuery["property"][0]);
            int page = Convert.ToInt32(parsedQuery[CallbackParams.Page][0]);

            var note = dbUser.Notes.First(x => x.Id == noteId);

            NoteMenu menu = new NoteMenu(resources, page, note);

            await botClient.AnswerCallbackQueryAsync(update.CallbackQuery.Id);
            try
            {
                await botClient.DeleteMessageAsync(update.CallbackQuery.Message.Chat.Id, update.CallbackQuery.Message.MessageId);
            }
            catch
            { }
            await botClient.SendTextMessageAsync(update.CallbackQuery.Message.Chat.Id, menu.GetDefaultTitle(actionScope), replyMarkup: menu.GetMarkup(actionScope));
        }
    }
}
