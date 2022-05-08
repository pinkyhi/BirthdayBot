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
        private readonly bool withBackButton;
        public MonthSelectMenu(IStringLocalizer<SharedResources> resources, bool withBackButton)
        {
            this.resources = resources;
            this.withBackButton = withBackButton;
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
            var keyboard = new List<List<KeyboardButton>>() { monthes0row, monthes1row, monthes2row };
            if (withBackButton)
            {
                keyboard.Add(new List<KeyboardButton>() { new KeyboardButton() { Text = resources["BACK_BUTTON"] } });
            }
            return new ReplyKeyboardMarkup(keyboard);
        }
    }
}
