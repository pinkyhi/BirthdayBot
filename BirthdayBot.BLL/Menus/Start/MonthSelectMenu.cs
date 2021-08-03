using BirthdayBot.Core.Resources;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using RapidBots.Types.Menus;
using System.Collections.Generic;
using Telegram.Bot.Types.ReplyMarkups;

namespace BirthdayBot.BLL.Menus
{
    public class MonthSelectMenu : IMenu
    {
        private readonly IStringLocalizer<SharedResources> resources;

        public MonthSelectMenu(IStringLocalizer<SharedResources> resources)
        {
            this.resources = resources;
        }

        public string GetDefaultTitle(IServiceScope actionScope = null, params string[] values)
        {
            return resources["BIRTH_MONTH_INPUT"];
        }

        public IReplyMarkup GetMarkup(IServiceScope actionScope = null)
        {
            List<KeyboardButton> monthes0row = new List<KeyboardButton>()
            {
                new KeyboardButton(resources["JANUARY"]),
                new KeyboardButton(resources["FEBRUARY"]),
                new KeyboardButton(resources["MARCH"]),
                new KeyboardButton(resources["APRIL"])

            };
            List<KeyboardButton> monthes1row = new List<KeyboardButton>()
            {
                new KeyboardButton(resources["MAY"]),
                new KeyboardButton(resources["JUNE"]),
                new KeyboardButton(resources["JULY"]),
                new KeyboardButton(resources["AUGUST"])

            };
            List<KeyboardButton> monthes2row = new List<KeyboardButton>()
            {
                new KeyboardButton(resources["SEPTEMBER"]),
                new KeyboardButton(resources["OCTOBER"]),
                new KeyboardButton(resources["NOVEMBER"]),
                new KeyboardButton(resources["DECEMBER"])

            };
            return new ReplyKeyboardMarkup(new List<List<KeyboardButton>>() { monthes0row, monthes1row, monthes2row });
        }
    }
}
