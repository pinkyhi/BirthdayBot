using BirthdayBot.BLL.Resources;
using BirthdayBot.Core.Resources;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using RapidBots.Types.Menus;
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
            return string.Concat(resources["CALENDAR_MENU_TEXT", $"<b>{monthes[month - 1]}</b>"], "\n", users);
        }

        public IReplyMarkup GetMarkup(IServiceScope actionScope = null)
        {
            List<InlineKeyboardButton> monthes0row = new List<InlineKeyboardButton>()
            {
                new InlineKeyboardButton(){CallbackData = QueryHelpers.AddQueryString(CommandKeys.Calendar, "month", $"1"), Text = string.Concat(resources["JANUARY"])},
                new InlineKeyboardButton(){CallbackData = QueryHelpers.AddQueryString(CommandKeys.Calendar, "month", $"2"), Text = string.Concat(resources["FEBRUARY"])},
                new InlineKeyboardButton(){CallbackData = QueryHelpers.AddQueryString(CommandKeys.Calendar, "month", $"3"), Text = string.Concat(resources["MARCH"])},
                new InlineKeyboardButton(){CallbackData = QueryHelpers.AddQueryString(CommandKeys.Calendar, "month", $"4"), Text = string.Concat(resources["APRIL"])}

            };
            List<InlineKeyboardButton> monthes1row = new List<InlineKeyboardButton>()
            {
                new InlineKeyboardButton(){CallbackData = QueryHelpers.AddQueryString(CommandKeys.Calendar, "month", $"5"), Text = string.Concat(resources["MAY"])},
                new InlineKeyboardButton(){CallbackData = QueryHelpers.AddQueryString(CommandKeys.Calendar, "month", $"6"), Text = string.Concat(resources["JUNE"])},
                new InlineKeyboardButton(){CallbackData = QueryHelpers.AddQueryString(CommandKeys.Calendar, "month", $"7"), Text = string.Concat(resources["JULY"])},
                new InlineKeyboardButton(){CallbackData = QueryHelpers.AddQueryString(CommandKeys.Calendar, "month", $"8"), Text = string.Concat(resources["AUGUST"])}

            };
            List<InlineKeyboardButton> monthes2row = new List<InlineKeyboardButton>()
            {
                new InlineKeyboardButton(){CallbackData = QueryHelpers.AddQueryString(CommandKeys.Calendar, "month", $"9"), Text = string.Concat(resources["SEPTEMBER"])},
                new InlineKeyboardButton(){CallbackData = QueryHelpers.AddQueryString(CommandKeys.Calendar, "month", $"10"), Text = string.Concat(resources["OCTOBER"])},
                new InlineKeyboardButton(){CallbackData = QueryHelpers.AddQueryString(CommandKeys.Calendar, "month", $"11"), Text = string.Concat(resources["NOVEMBER"])},
                new InlineKeyboardButton(){CallbackData = QueryHelpers.AddQueryString(CommandKeys.Calendar, "month", $"12"), Text = string.Concat(resources["DECEMBER"])}

            };
            var keyboard = new List<List<InlineKeyboardButton>>() { monthes0row, monthes1row, monthes2row };

            InlineKeyboardMarkup result = new InlineKeyboardMarkup(keyboard);
            return result;
        }
    }
}