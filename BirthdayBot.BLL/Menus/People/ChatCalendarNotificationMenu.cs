using BirthdayBot.BLL.Resources;
using BirthdayBot.Core.Resources;
using BirthdayBot.DAL.Entities;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using RapidBots.Constants;
using RapidBots.Types.Menus;
using System.Collections.Generic;
using Telegram.Bot.Types.ReplyMarkups;

namespace BirthdayBot.BLL.Menus.People
{
    public class ChatCalendarNotificationMenu : IMenu
    {
        private readonly IStringLocalizer<SharedResources> resources;
        private readonly long userId;
        private readonly long chatId;

        public ChatCalendarNotificationMenu(IStringLocalizer<SharedResources> resources, long userId, long chatId)
        {
            this.resources = resources;
            this.userId = userId;
            this.chatId = chatId;
        }

        public string GetDefaultTitle(IServiceScope actionScope = null, params string[] values)
        {
            return resources["FRIEND_ADDED_TO_CHAT_CALENDAR", values];
        }

        public IReplyMarkup GetMarkup(IServiceScope actionScope = null)
        {
            var backDict = new Dictionary<string, string>();

            InlineKeyboardButton subscribe = new InlineKeyboardButton() { CallbackData = QueryHelpers.AddQueryString(CommandKeys.SubscribeByReferralReply, "userId", userId.ToString()), Text = resources["SUBSCRIBE_BUTTON"] };
            InlineKeyboardButton notSubscribe = new InlineKeyboardButton() { CallbackData = CommandKeys.RemoveMessage, Text = "❌" };
            var qParams = new Dictionary<string, string>();
            qParams.Add("chatId", chatId.ToString());
            qParams.Add("oneTime", "1");
            InlineKeyboardButton unsubscribeFromChatCalendar = new InlineKeyboardButton() { CallbackData = QueryHelpers.AddQueryString(CommandKeys.ChangeChatSubscription, qParams), Text = resources["UNSUBSCRIBE_CHAT_BUTTON"] };

            InlineKeyboardMarkup result = new InlineKeyboardMarkup(new InlineKeyboardButton[][] {
                new[]
                {
                    subscribe, notSubscribe
                },
                new[]
                {
                    unsubscribeFromChatCalendar
                }
            });
            return result;
        }
    }
}
