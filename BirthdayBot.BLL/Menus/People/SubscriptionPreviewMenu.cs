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
    public class SubscriptionPreviewMenu : IMenu
    {
        private readonly IStringLocalizer<SharedResources> resources;
        private readonly Subscription subcription;
        private readonly Dictionary<string, string> qParams;

        public SubscriptionPreviewMenu(IStringLocalizer<SharedResources> resources, Dictionary<string, string> qParams, Subscription subcription)
        {
            this.resources = resources;
            this.subcription = subcription;
            this.qParams = qParams;
        }

        public string GetDefaultTitle(IServiceScope actionScope = null, params string[] values)
        {
            return resources["SUBSCRIPTION_PREVIEW_TEXT", "@" + subcription.Target.Username, subcription.Subscriber.GetAnotherUserDateString(subcription.Target)];
        }

        public IReplyMarkup GetMarkup(IServiceScope actionScope = null)
        {
            var backDict = new Dictionary<string, string>();

            InlineKeyboardButton subscribe = new InlineKeyboardButton() { CallbackData = QueryHelpers.AddQueryString(CommandKeys.SubscriptionPreviewConfirm, qParams), Text = resources["SUBSCRIBE_BUTTON"] };

            InlineKeyboardButton back = null;

            backDict.Add("chatsPage", "0");
            backDict.Add("chatId", qParams["chatId"]);
            backDict.Add(CallbackParams.Page, qParams["chatPage"]);
            back = new InlineKeyboardButton() { CallbackData = QueryHelpers.AddQueryString(CommandKeys.OpenChat, backDict), Text = resources["BACK_BUTTON"] };
            
            InlineKeyboardMarkup result = new InlineKeyboardMarkup(new InlineKeyboardButton[][] {
                new[]
                {
                    subscribe
                },
                new[]
                {
                    back
                }
            });
            return result;
        }
    }
}
