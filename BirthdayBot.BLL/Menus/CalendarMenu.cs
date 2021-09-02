using BirthdayBot.BLL.Resources;
using BirthdayBot.Core.Resources;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using RapidBots.Constants;
using RapidBots.Types.Menus;
using System;
using System.Collections.Generic;
using Telegram.Bot.Types.ReplyMarkups;

namespace BirthdayBot.BLL.Menus
{
    public class CalendarMenu : IMenu
    {
        private readonly IStringLocalizer<SharedResources> resources;
        private readonly int month;
        private readonly string users;
        List<string> monthes = new List<string>();
            

        public CalendarMenu(IStringLocalizer<SharedResources> resources, int month, string users)
        {
            this.resources = resources;
            this.month = month;
            this.users = users;
            monthes.AddRange(new List<string>(){
                resources["JANUARY"],
                resources["FEBRUARY"],
                resources["MARCH"],
                resources["APRIL"],
                resources["MAY"],
                resources["JUNE"],
                resources["JULY"],
                resources["AUGUST"],
                resources["SEPTEMBER"],
                resources["OCTOBER"],
                resources["NOVEMBER"],
                resources["DECEMBER"]
            });
        }

        public string GetDefaultTitle(IServiceScope actionScope = null, params string[] values)
        {
            return string.Concat(resources["CALENDAR_MENU_TEXT", monthes[month - 1]], "\n", users);
        }

        public IReplyMarkup GetMarkup(IServiceScope actionScope = null)
        {

            InlineKeyboardButton back = new InlineKeyboardButton() { CallbackData = QueryHelpers.AddQueryString(CommandKeys.Calendar, "month", $"{(month - 1 < 1 ? 12 : month - 1)}"), Text = string.Concat(monthes[month - 2 < 0 ? 11 : month - 2], "←") };
            InlineKeyboardButton forward = new InlineKeyboardButton() { CallbackData = QueryHelpers.AddQueryString(CommandKeys.Calendar, "month", $"{(month + 1 > 12 ? 1 : month + 1)}"), Text = string.Concat("→", monthes[month > 11 ? 0 : month]) };

            InlineKeyboardMarkup result = new InlineKeyboardMarkup(new InlineKeyboardButton[][] {
                new[]
                {
                    back,
                    forward
                }
            });
            return result;
        }
    }
}