using BirthdayBot.BLL.Resources;
using BirthdayBot.Core.Resources;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using RapidBots.Types.Menus;
using System.Collections.Generic;
using System.Linq;
using Telegram.Bot.Types.ReplyMarkups;

namespace BirthdayBot.BLL.Menus
{
    public class CalendarMenu : IMenu
    {
        private readonly IStringLocalizer<SharedResources> resources;
        private readonly int month;
        private readonly string users;
        private readonly IEnumerable<IGrouping<int, int>> countPerMonth;
        List<string> monthes = new List<string>();
            

        public CalendarMenu(IStringLocalizer<SharedResources> resources, int month, string users, IEnumerable<IGrouping<int, int>> countPerMonth)
        {
            this.resources = resources;
            this.month = month;
            this.users = users;
            this.countPerMonth = countPerMonth;
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
                new InlineKeyboardButton(){CallbackData = QueryHelpers.AddQueryString(CommandKeys.Calendar, "month", $"1"), Text = string.Concat(resources["JANUARY"], countPerMonth.FirstOrDefault(x => x.Key == 1) == null ? "" : $" ({countPerMonth.FirstOrDefault(x => x.Key == 1).Count()})" )},
                new InlineKeyboardButton(){CallbackData = QueryHelpers.AddQueryString(CommandKeys.Calendar, "month", $"2"), Text = string.Concat(resources["FEBRUARY"], countPerMonth.FirstOrDefault(x => x.Key == 2) == null ? "" : $" ({countPerMonth.FirstOrDefault(x => x.Key == 2).Count()})" )},
                new InlineKeyboardButton(){CallbackData = QueryHelpers.AddQueryString(CommandKeys.Calendar, "month", $"3"), Text = string.Concat(resources["MARCH"], countPerMonth.FirstOrDefault(x => x.Key == 3) == null ? "" : $" ({countPerMonth.FirstOrDefault(x => x.Key == 3).Count()})" )},
                new InlineKeyboardButton(){CallbackData = QueryHelpers.AddQueryString(CommandKeys.Calendar, "month", $"4"), Text = string.Concat(resources["APRIL"], countPerMonth.FirstOrDefault(x => x.Key == 4) == null ? "" : $" ({countPerMonth.FirstOrDefault(x => x.Key == 4).Count()})" )}

            };
            List<InlineKeyboardButton> monthes1row = new List<InlineKeyboardButton>()
            {
                new InlineKeyboardButton(){CallbackData = QueryHelpers.AddQueryString(CommandKeys.Calendar, "month", $"5"), Text = string.Concat(resources["MAY"], countPerMonth.FirstOrDefault(x => x.Key == 5) == null ? "" : $" ({countPerMonth.FirstOrDefault(x => x.Key == 5).Count()})" )},
                new InlineKeyboardButton(){CallbackData = QueryHelpers.AddQueryString(CommandKeys.Calendar, "month", $"6"), Text = string.Concat(resources["JUNE"], countPerMonth.FirstOrDefault(x => x.Key == 6) == null ? "" : $" ({countPerMonth.FirstOrDefault(x => x.Key == 6).Count()})" )},
                new InlineKeyboardButton(){CallbackData = QueryHelpers.AddQueryString(CommandKeys.Calendar, "month", $"7"), Text = string.Concat(resources["JULY"], countPerMonth.FirstOrDefault(x => x.Key == 7) == null ? "" : $" ({countPerMonth.FirstOrDefault(x => x.Key == 7).Count()})" )},
                new InlineKeyboardButton(){CallbackData = QueryHelpers.AddQueryString(CommandKeys.Calendar, "month", $"8"), Text = string.Concat(resources["AUGUST"], countPerMonth.FirstOrDefault(x => x.Key == 8) == null ? "" : $" ({countPerMonth.FirstOrDefault(x => x.Key == 8).Count()})" )}

            };
            List<InlineKeyboardButton> monthes2row = new List<InlineKeyboardButton>()
            {
                new InlineKeyboardButton(){CallbackData = QueryHelpers.AddQueryString(CommandKeys.Calendar, "month", $"9"), Text = string.Concat(resources["SEPTEMBER"], countPerMonth.FirstOrDefault(x => x.Key == 9) == null ? "" : $" ({countPerMonth.FirstOrDefault(x => x.Key == 9).Count()})" )},
                new InlineKeyboardButton(){CallbackData = QueryHelpers.AddQueryString(CommandKeys.Calendar, "month", $"10"), Text = string.Concat(resources["OCTOBER"], countPerMonth.FirstOrDefault(x => x.Key == 10) == null ? "" : $" ({countPerMonth.FirstOrDefault(x => x.Key == 10).Count()})" )},
                new InlineKeyboardButton(){CallbackData = QueryHelpers.AddQueryString(CommandKeys.Calendar, "month", $"11"), Text = string.Concat(resources["NOVEMBER"], countPerMonth.FirstOrDefault(x => x.Key == 11) == null ? "" : $" ({countPerMonth.FirstOrDefault(x => x.Key == 11).Count()})" )},
                new InlineKeyboardButton(){CallbackData = QueryHelpers.AddQueryString(CommandKeys.Calendar, "month", $"12"), Text = string.Concat(resources["DECEMBER"], countPerMonth.FirstOrDefault(x => x.Key == 12) == null ? "" : $" ({countPerMonth.FirstOrDefault(x => x.Key == 12).Count()})" )}

            };
            var keyboard = new List<List<InlineKeyboardButton>>() { monthes0row, monthes1row, monthes2row };

            InlineKeyboardMarkup result = new InlineKeyboardMarkup(keyboard);
            return result;
        }
    }
}