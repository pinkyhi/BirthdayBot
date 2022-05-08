using BirthdayBot.BLL.Menus.People;
using BirthdayBot.BLL.Resources;
using BirthdayBot.Core.Resources;
using BirthdayBot.DAL.Entities;
using BirthdayBot.DAL.Interfaces;
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
using RapidBots.Types.Attributes;
using Telegram.Bot.Types.Enums;
using RapidBots.Extensions;
using System.Collections.Generic;

namespace BirthdayBot.BLL.Commands.People
{
    [ChatType(ChatType.Private)]
    [ExpectedParams("targetId", CallbackParams.Page)]
    [ExpectedParams("chi", "chp", "targetId")]
    public class SubscriptionRemoveConfirm : Command
    {
        private readonly BotClient botClient;

        public SubscriptionRemoveConfirm(BotClient botClient)
        {
            this.botClient = botClient;
        }

        public override string Key => CommandKeys.SubscriptionRemoveConfirm;

        public override async Task Execute(Update update, TelegramUser user = null, IServiceScope actionScope = null)
        {
            var resources = actionScope.ServiceProvider.GetService<IStringLocalizer<SharedResources>>();
            var repository = actionScope.ServiceProvider.GetService<IRepository>();

            TUser dbUser = (user as TUser) ?? await repository.GetAsync<TUser>(true, u => u.Id == update.CallbackQuery.From.Id, include: u => u.Include(x => x.Subscriptions).ThenInclude(x => x.Target));

            if (dbUser?.Subscriptions == null)
            {
                await repository.LoadCollectionAsync(dbUser, x => x.Subscriptions);
                await repository.LoadCollectionAsync(dbUser, x => x.Subscribers);
            }

            var qParams = new Dictionary<string, string>();
            var updateParams = update.GetParams();
            long targetId = Convert.ToInt32(updateParams["targetId"]);
            qParams.Add("targetId", updateParams["targetId"]);
            if (updateParams.ContainsKey(CallbackParams.Page))
            {
                qParams.Add(CallbackParams.Page, updateParams[CallbackParams.Page]);
            }
            else
            {
                qParams.Add("chi", updateParams["chi"]);
                qParams.Add("chp", updateParams["chp"]);
            }

            var subscription = dbUser.Subscriptions.First(x => x.TargetId == targetId);
            if (subscription != null)
            {
                dbUser.Subscriptions.Remove(subscription);
                await repository.UpdateAsync(dbUser);
            }
            else
            {
                throw new ArgumentException();
            }

            if (updateParams.ContainsKey(CallbackParams.Page))
            {
                PeopleMenu menu = new PeopleMenu(resources);

                try { await botClient.AnswerCallbackQueryAsync(update.CallbackQuery.Id); } catch { }
                try
                {
                    await botClient.DeleteMessageAsync(update.CallbackQuery.Message.Chat.Id, update.CallbackQuery.Message.MessageId);
                }
                catch
                { }
                await botClient.SendTextMessageAsync(update.CallbackQuery.Message.Chat.Id, menu.GetDefaultTitle(actionScope), replyMarkup: menu.GetMarkup(Convert.ToInt32(updateParams[CallbackParams.Page]), dbUser.Subscriptions, actionScope), parseMode: Telegram.Bot.Types.Enums.ParseMode.Html);
            }
            else
            {
                long chatId = Convert.ToInt64(updateParams["chi"]);
                int page = Convert.ToInt32(updateParams["chp"]);
                var chat = await repository.GetAsync<DAL.Entities.Chat>(false, c => c.Id == chatId, x => x.Include(x => x.ChatMembers).ThenInclude(x => x.User));

                var openerMessage = await botClient.SendTextMessageAsync(update.Message?.Chat?.Id ?? update.CallbackQuery.Message.Chat.Id, resources["MENU_OPENER_TEXT"], replyMarkup: new ReplyKeyboardRemove(), parseMode: Telegram.Bot.Types.Enums.ParseMode.Html, disableNotification: true);
                await botClient.DeleteMessageAsync(openerMessage.Chat.Id, openerMessage.MessageId);

                var chatMembers = chat.ChatMembers;
                if (chatMembers == null)
                {
                    await botClient.SendTextMessageAsync(update.Message?.Chat?.Id ?? update.CallbackQuery.Message.Chat.Id, resources["OPEN_CHAT_ERROR"], parseMode: Telegram.Bot.Types.Enums.ParseMode.Html);
                    throw new ArgumentException();
                }
                var userChatMember = chatMembers.Find(x => x.UserId == dbUser.Id);
                chatMembers.Remove(chatMembers.Find(x => x.UserId == dbUser.Id));


                Dictionary<string, string> queryParams = new Dictionary<string, string>();

                queryParams.Add("chi", $"{chatId}");
                queryParams.Add("chsP", $"{page}");

                OpenChatMenu menu = new OpenChatMenu(queryParams, resources, $"{page}", dbUser, chat.Id, userChatMember);

                try { await botClient.AnswerCallbackQueryAsync(update.CallbackQuery.Id); } catch { }
                try
                {
                    await botClient.DeleteMessageAsync(update.CallbackQuery.Message.Chat.Id, update.CallbackQuery.Message.MessageId);
                }
                catch
                { }
                if (chatMembers.Count() > 0)
                {
                    await botClient.SendTextMessageAsync(update.CallbackQuery.Message.Chat.Id, menu.GetDefaultTitle(actionScope), replyMarkup: menu.GetMarkup(page, chatMembers, actionScope), parseMode: Telegram.Bot.Types.Enums.ParseMode.Html);
                }
                else
                {
                    await botClient.SendTextMessageAsync(update.CallbackQuery.Message.Chat.Id, string.Concat(menu.GetDefaultTitle(actionScope), resources["CHAT_WARNING_TEXT"]), replyMarkup: menu.GetMarkup(page, chatMembers, actionScope), parseMode: Telegram.Bot.Types.Enums.ParseMode.Html);
                }
            }
        }
    }
}
