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

namespace BirthdayBot.BLL.Menus.Notes
{
    public class SubscriptionRemoveConfirmation : IMenu
    {
        private readonly IStringLocalizer<SharedResources> resources;
        private readonly int fromPage;
        private readonly Subscription subscription;

        public SubscriptionRemoveConfirmation(IStringLocalizer<SharedResources> resources, int fromPage, Subscription subscription)
        {
            this.resources = resources;
            this.fromPage = fromPage;
            this.subscription = subscription;
        }

        public string GetDefaultTitle(IServiceScope actionScope = null, params string[] values)
        {
            return resources["SUBSCRIPTION_REMOVE_CONFIRMATION", values];
        }

        public IReplyMarkup GetMarkup(IServiceScope actionScope = null)
        {
            var qParams = new Dictionary<string, string>();
            qParams.Add("targetId", $"{subscription.TargetId}");
            qParams.Add(CallbackParams.Page, fromPage.ToString());

            InlineKeyboardButton Confirm = new InlineKeyboardButton() { CallbackData = QueryHelpers.AddQueryString(CommandKeys.SubscriptionRemoveConfirm, qParams), Text = resources["CONFIRM_BUTTON"] };
            InlineKeyboardButton Reject = new InlineKeyboardButton() { CallbackData = QueryHelpers.AddQueryString(CommandKeys.OpenSubscription, qParams), Text = resources["REJECT_BUTTON"] };

            InlineKeyboardMarkup result = new InlineKeyboardMarkup(new InlineKeyboardButton[][] {
                new[]
                {
                    Confirm,
                    Reject
                }
            });
            return result;
        }
    }
}
