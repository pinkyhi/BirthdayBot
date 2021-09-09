using BirthdayBot.BLL.Menus.People;
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
using System.Linq;

namespace BirthdayBot.BLL.Commands.People.Chats
{
    [ChatType(ChatType.Private)]
    [ExpectedParams(CallbackParams.Page, "chatId", "chatsPage")]
    public class OpenChat : Command
    {
        private readonly BotClient botClient;

        public OpenChat(BotClient botClient)
        {
            this.botClient = botClient;
        }

        public override string Key => CommandKeys.OpenChat;

        public override async Task Execute(Update update, TelegramUser user = null, IServiceScope actionScope = null)
        {
            var resources = actionScope.ServiceProvider.GetService<IStringLocalizer<SharedResources>>();
            var repository = actionScope.ServiceProvider.GetService<IRepository>();

            TUser dbUser = (user as TUser) ?? await repository.GetAsync<TUser>(false, u => u.Id == update.CallbackQuery.From.Id, include: u => u.Include(x => x.ChatMembers).ThenInclude(x => x.Chat));
            if (dbUser?.Subscriptions == null)
            {
                await repository.LoadCollectionAsync(dbUser, x => x.Subscriptions);
            }
            dbUser.MiddlewareData = null;
            dbUser.CurrentStatus = null;
            await repository.UpdateAsync(dbUser);

            int page = Convert.ToInt32(update.GetParams()[CallbackParams.Page]);
            long chatId = Convert.ToInt64(update.GetParams()["chatId"]);
            string chatsPage = update.GetParams()["chatsPage"];
            var chat = await repository.GetAsync<DAL.Entities.Chat>(false, c => c.Id == chatId, x => x.Include(x => x.ChatMembers).ThenInclude(x => x.User));

            var openerMessage = await botClient.SendTextMessageAsync(update.Message?.Chat?.Id ?? update.CallbackQuery.Message.Chat.Id, resources["MENU_OPENER_TEXT"], replyMarkup: new ReplyKeyboardRemove(), parseMode: Telegram.Bot.Types.Enums.ParseMode.Html);
            await botClient.DeleteMessageAsync(openerMessage.Chat.Id, openerMessage.MessageId);

            var chatMembers = chat.ChatMembers;
            if (chatMembers == null)
            {
                await botClient.SendTextMessageAsync(update.Message?.Chat?.Id ?? update.CallbackQuery.Message.Chat.Id, resources["OPEN_CHAT_ERROR"], parseMode: Telegram.Bot.Types.Enums.ParseMode.Html);
                throw new ArgumentException();
            }
            chatMembers.Remove(chatMembers.Find(x => x.UserId == dbUser.Id));

            OpenChatMenu menu = new OpenChatMenu(resources, chatsPage, dbUser);

            try { await botClient.AnswerCallbackQueryAsync(update.CallbackQuery.Id); } catch { }
            try
            {
                await botClient.DeleteMessageAsync(update.CallbackQuery.Message.Chat.Id, update.CallbackQuery.Message.MessageId);
            }
            catch
            { }
            if (chatMembers.Count() > 0)
            {
                await botClient.SendTextMessageAsync(update.CallbackQuery.Message.Chat.Id, menu.GetDefaultTitle(actionScope), replyMarkup: menu.GetMarkup(page, chatMembers, actionScope), parseMode: ParseMode.Html);
            }
            else
            {
                await botClient.SendTextMessageAsync(update.CallbackQuery.Message.Chat.Id, string.Concat(menu.GetDefaultTitle(actionScope), resources["CHAT_WARNING_TEXT"]), replyMarkup: menu.GetMarkup(page, chatMembers, actionScope), parseMode: ParseMode.Html);
            }
        }
    }
}
