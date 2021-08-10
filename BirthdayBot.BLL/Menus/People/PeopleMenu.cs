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
            var addButton = new InlineKeyboardButton() { Text = resources["ADD_BUTTON"], CallbackData = CommandKeys.AddPeople };
            var result = new List<List<InlineKeyboardButton>>() { new List<InlineKeyboardButton>() { addButton } };
            var now = DateTime.Now;
            source.Sort((x, y) => (now - y.Target.BirthDate).CompareTo(now - x.Target.BirthDate));
            var pageButtons = this.GetPage(page, source, x =>
            {
                var qParams = new Dictionary<string, string>();
                qParams.Add("targetId", $"{x.TargetId}");
                qParams.Add(CallbackParams.Page, page.ToString());
                return new InlineKeyboardButton() { Text = string.Concat(x.Target.Username, " ", x.Target.BirthDate.ToShortDateString()), CallbackData = QueryHelpers.AddQueryString(CommandKeys.OpenSubscription, qParams) };
            });

            var backBut = new InlineKeyboardButton() { CallbackData = CommandKeys.Start, Text = resources["BACK_BUTTON"] };

            result.AddRange(pageButtons);
            result.Add(new List<InlineKeyboardButton>() { backBut });
            return new InlineKeyboardMarkup(result);
        }
    }
}
