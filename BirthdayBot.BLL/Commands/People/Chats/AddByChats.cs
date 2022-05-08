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
    [ExpectedParams(CallbackParams.Page)]
    public class AddByChats : Command
    {
        private readonly BotClient botClient;

        public AddByChats(BotClient botClient)
        {
            this.botClient = botClient;
        }

        public override string Key => CommandKeys.AddByChats;

        public override async Task Execute(Update update, TelegramUser user = null, IServiceScope actionScope = null)
        {
            var resources = actionScope.ServiceProvider.GetService<IStringLocalizer<SharedResources>>();
            var repository = actionScope.ServiceProvider.GetService<IRepository>();

            TUser dbUser = (user as TUser) ?? await repository.GetAsync<TUser>(true, u => u.Id == update.CallbackQuery.From.Id, include: u => u.Include(x => x.ChatMembers).ThenInclude(x => x.Chat).ThenInclude(x => x.ChatMembers));

            if (dbUser?.ChatMembers == null)
            {
                await repository.LoadCollectionAsync(dbUser, x => x.ChatMembers);
                foreach(var chatMember in dbUser.ChatMembers)
                {
                    if(chatMember.Chat == null)
                    {
                        await repository.LoadReferenceAsync(chatMember, x => x.Chat);
                        await repository.LoadCollectionAsync(chatMember.Chat, x => x.ChatMembers);
                    }
                    
                }
            }
            dbUser.MiddlewareData = null;
            dbUser.CurrentStatus = null;
            await repository.UpdateAsync(dbUser);

            int page = Convert.ToInt32(update.GetParams()[CallbackParams.Page]);

            var openerMessage = await botClient.SendTextMessageAsync(update.Message?.Chat?.Id ?? update.CallbackQuery.Message.Chat.Id, resources["MENU_OPENER_TEXT"], replyMarkup: new ReplyKeyboardRemove(), parseMode: Telegram.Bot.Types.Enums.ParseMode.Html, disableNotification: true);
            await botClient.DeleteMessageAsync(openerMessage.Chat.Id, openerMessage.MessageId);

            ChatsMenu menu = new ChatsMenu(resources);

            try { await botClient.AnswerCallbackQueryAsync(update.CallbackQuery.Id); } catch { }
            try
            {
                await botClient.DeleteMessageAsync(update.CallbackQuery.Message.Chat.Id, update.CallbackQuery.Message.MessageId);
            }
            catch
            { }
            var chats = dbUser.ChatMembers.Select(x => x.Chat).Distinct().ToList();
            if (chats.Count() > 0)
            {
                await botClient.SendTextMessageAsync(update.CallbackQuery.Message.Chat.Id, menu.GetDefaultTitle(actionScope), replyMarkup: menu.GetMarkup(page, chats, actionScope), parseMode: Telegram.Bot.Types.Enums.ParseMode.Html);
            }
            else
            {
                await botClient.SendTextMessageAsync(update.CallbackQuery.Message.Chat.Id, string.Concat(menu.GetDefaultTitle(actionScope), resources["CHATS_WARNING_TEXT"]), replyMarkup: menu.GetMarkup(page, chats, actionScope), parseMode: Telegram.Bot.Types.Enums.ParseMode.Html);
            }
        }
    }
}
