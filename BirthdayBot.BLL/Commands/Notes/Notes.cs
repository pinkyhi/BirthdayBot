using BirthdayBot.BLL.Menus.Notes;
using BirthdayBot.BLL.Resources;
using BirthdayBot.Core.Resources;
using BirthdayBot.DAL.Entities;
using BirthdayBot.DAL.Interfaces;
using Microsoft.EntityFrameworkCore;
using RapidBots.Types.Attributes;
using Telegram.Bot.Types.Enums;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using RapidBots.Constants;
using RapidBots.Types.Core;
using System;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using RapidBots.Extensions;

namespace BirthdayBot.BLL.Commands.Notes
{
    [ChatType(ChatType.Private)]
    [ExpectedParams(CallbackParams.Page)]
    public class Notes : Command
    {
        private readonly BotClient botClient;

        public Notes(BotClient botClient)
        {
            this.botClient = botClient;
        }

        public override string Key => CommandKeys.Notes;

        public override async Task Execute(Update update, TelegramUser user = null, IServiceScope actionScope = null)
        {
            var resources = actionScope.ServiceProvider.GetService<IStringLocalizer<SharedResources>>();
            var repository = actionScope.ServiceProvider.GetService<IRepository>();

            TUser dbUser = (user as TUser) ?? await repository.GetAsync<TUser>(false, u => u.Id == update.CallbackQuery.From.Id, include: u => u.Include(x => x.Notes));

            if (dbUser?.Notes == null)
            {
                await repository.LoadCollectionAsync(dbUser, x => x.Notes);
            }
            dbUser.MiddlewareData = null;
            dbUser.CurrentStatus = null;
            await repository.UpdateAsync(dbUser);

            int page = Convert.ToInt32(update.GetParams()[CallbackParams.Page]);

            var openerMessage = await botClient.SendTextMessageAsync(update.Message?.Chat?.Id ?? update.CallbackQuery.Message.Chat.Id, resources["MENU_OPENER_TEXT"], replyMarkup: new ReplyKeyboardRemove(), parseMode: Telegram.Bot.Types.Enums.ParseMode.Html, disableNotification: true);
            await botClient.DeleteMessageAsync(openerMessage.Chat.Id, openerMessage.MessageId);

            NotesMenu menu = new NotesMenu(resources);

            try{await botClient.AnswerCallbackQueryAsync(update.CallbackQuery.Id);}catch{}
            try
            {
                await botClient.DeleteMessageAsync(update.CallbackQuery.Message.Chat.Id, update.CallbackQuery.Message.MessageId);
            }
            catch
            { }
            await botClient.SendTextMessageAsync(update.CallbackQuery.Message.Chat.Id, menu.GetDefaultTitle(actionScope), replyMarkup: menu.GetMarkup(page, dbUser.Notes, actionScope), parseMode: Telegram.Bot.Types.Enums.ParseMode.Html);
        }
    }
}
