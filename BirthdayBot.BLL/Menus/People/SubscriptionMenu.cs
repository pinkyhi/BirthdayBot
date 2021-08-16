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
        private readonly int fromPage;
        private readonly Subscription subcription;

        public SubscriptionMenu(IStringLocalizer<SharedResources> resources, int fromPage, Subscription subcription)
        {
            this.resources = resources;
            this.fromPage = fromPage;
            this.subcription = subcription;
        }

        public string GetDefaultTitle(IServiceScope actionScope = null, params string[] values)
        {
            return resources["SUBSCRIPTION_TEXT", "@" + subcription.Target.Username, subcription.Target.BirthDate, subcription.IsStrong ? resources["STRONG_NOTIFICATION_TEXT"] : resources["COMMON_NOTIFICATION_TEXT"]];
        }

        public IReplyMarkup GetMarkup(IServiceScope actionScope = null)
        {
            var qParams = new Dictionary<string, string>();
            qParams.Add("targetId", $"{subcription.TargetId}");
            qParams.Add(CallbackParams.Page, fromPage.ToString());

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

            InlineKeyboardButton back = new InlineKeyboardButton() { CallbackData = QueryHelpers.AddQueryString(CommandKeys.People, CallbackParams.Page, $"{fromPage}"), Text = resources["BACK_BUTTON"] };

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
