using BirthdayBot.BLL.Resources;
using BirthdayBot.Core.Resources;
using BirthdayBot.DAL.Entities;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using RapidBots.Constants;
using RapidBots.Types.Menus;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Telegram.Bot.Types.ReplyMarkups;

namespace BirthdayBot.BLL.Menus.People
{
    public class PeopleMenu : PaginationMenu
    {
        private readonly IStringLocalizer<SharedResources> resources;

        public PeopleMenu(IStringLocalizer<SharedResources> resources) : base(8, 1, CommandKeys.Notes)
        {
            this.resources = resources;
        }

        public override string GetDefaultTitle(IServiceScope actionScope = null, params string[] values)
        {
            return resources["PEOPLE_TEXT", values];
        }

        public IReplyMarkup GetMarkup(int page, List<Subscription> source, IServiceScope actionScope = null)
        {
            var addButton = new InlineKeyboardButton() { Text = resources["ADD_BUTTON"], CallbackData = QueryHelpers.AddQueryString(CommandKeys.AddPeople, "peoplePage", page.ToString()) };
            var result = new List<List<InlineKeyboardButton>>() { new List<InlineKeyboardButton>() { addButton } };
            var now = DateTime.Now;
            source.Sort((x, y) => (x.IsStrong).CompareTo(y.IsStrong));
            int lastStrongCount = source.Count(x => x.IsStrong);
            int notSrtongCount = source.Count() - lastStrongCount;
            var comparer = new SubscriptionComparer<Subscription>();
            source.Sort(0, lastStrongCount, comparer);
            source.Sort(lastStrongCount, notSrtongCount, comparer);
            var pageButtons = this.GetPage(page, source, x =>
            {
                var qParams = new Dictionary<string, string>();
                qParams.Add("targetId", $"{x.TargetId}");
                qParams.Add(CallbackParams.Page, page.ToString());
                if (x.IsStrong)
                {
                    return new InlineKeyboardButton() { Text = string.Concat(resources["STRONG_NOTIFICATION_BUTTON"], x.Target.Username, " ", x.Target.BirthDate.ToShortDateString()), CallbackData = QueryHelpers.AddQueryString(CommandKeys.OpenSubscription, qParams) };
                }
                else
                {
                    return new InlineKeyboardButton() { Text = string.Concat(x.Target.Username, " ", x.Target.BirthDate.ToShortDateString()), CallbackData = QueryHelpers.AddQueryString(CommandKeys.OpenSubscription, qParams) };
                }
            });

            var backBut = new InlineKeyboardButton() { CallbackData = CommandKeys.Start, Text = resources["BACK_BUTTON"] };

            result.AddRange(pageButtons);
            result.Add(new List<InlineKeyboardButton>() { backBut });
            return new InlineKeyboardMarkup(result);
        }
    }

    public class SubscriptionComparer<T> : IComparer<T> where T : Subscription
    {
        private readonly DateTime now;
        public SubscriptionComparer()
        {
            now = DateTime.Now;
        }
        public int Compare([AllowNull] T x, [AllowNull] T y)
        {
            if(x == null && y == null)
            {
                return 0;
            }
            else if(x == null)
            {
                return 1;
            }
            else if(y == null)
            {
                return -1;
            }
            else
            {
                return (now - y.Target.BirthDate).CompareTo(now - x.Target.BirthDate);
            }
        }
    }
}
