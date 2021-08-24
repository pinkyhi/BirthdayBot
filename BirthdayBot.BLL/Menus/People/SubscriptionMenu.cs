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
    public class SubscriptionMenu : IMenu
    {
        private readonly IStringLocalizer<SharedResources> resources;
        private readonly Subscription subcription;
        private readonly Dictionary<string, string> qParams;

        public SubscriptionMenu(IStringLocalizer<SharedResources> resources, Dictionary<string,string> qParams, Subscription subcription)
        {
            this.resources = resources;
            this.subcription = subcription;
            this.qParams = qParams;
        }

        public string GetDefaultTitle(IServiceScope actionScope = null, params string[] values)
        {
            return resources["SUBSCRIPTION_TEXT", "@" + subcription.Target.Username, subcription.Target.BirthDate, subcription.IsStrong ? resources["STRONG_NOTIFICATION_TEXT"] : resources["COMMON_NOTIFICATION_TEXT"]];
        }

        public IReplyMarkup GetMarkup(IServiceScope actionScope = null)
        {
            var backDict = new Dictionary<string, string>();

            InlineKeyboardButton removeBut = new InlineKeyboardButton() { CallbackData = QueryHelpers.AddQueryString(CommandKeys.RemoveSubscription, qParams), Text = resources["REMOVE_BUTTON"] };

            InlineKeyboardButton changeSubsotifBut = null;
            if (subcription.IsStrong)
            {
                changeSubsotifBut = new InlineKeyboardButton() { CallbackData = QueryHelpers.AddQueryString(CommandKeys.ChangeSubscriptionType, qParams), Text = resources["STRONG_NOTIFICATION_BUTTON"] };
            }
            else
            {
                changeSubsotifBut = new InlineKeyboardButton() { CallbackData = QueryHelpers.AddQueryString(CommandKeys.ChangeSubscriptionType, qParams), Text = resources["COMMON_NOTIFICATION_BUTTON"] };
            }
            InlineKeyboardButton back = null;
            if (qParams.ContainsKey(CallbackParams.Page))
            {
                backDict.Add(CallbackParams.Page, qParams[CallbackParams.Page]);
                back = new InlineKeyboardButton() { CallbackData = QueryHelpers.AddQueryString(CommandKeys.People, backDict), Text = resources["BACK_BUTTON"] };
            }
            else
            {
                backDict.Add("chatsPage", "0");
                backDict.Add("chatId", qParams["chatId"]);
                backDict.Add(CallbackParams.Page, qParams["chatPage"]);
                back = new InlineKeyboardButton() { CallbackData = QueryHelpers.AddQueryString(CommandKeys.OpenChat, backDict), Text = resources["BACK_BUTTON"] };
            }


            InlineKeyboardMarkup result = new InlineKeyboardMarkup(new InlineKeyboardButton[][] {
                new[]
                {
                    changeSubsotifBut,
                    removeBut
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
