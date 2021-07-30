using BirthdayBot.BLL.Resources;
using BirthdayBot.Core.Resources;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using RapidBots.Constants;
using RapidBots.Types.Menus;
using System;
using Telegram.Bot.Types.ReplyMarkups;

namespace BirthdayBot.BLL.Menus
{
    public class StartMenu : IMenu
    {
        private readonly IStringLocalizer<SharedResources> resources;

        public StartMenu(IStringLocalizer<SharedResources> resources)
        {
            this.resources = resources;
        }

        public string GetDefaultTitle(IServiceScope actionScope = null, params string[] values)
        {
            return resources["WELCOME_TEX", values];
        }

        public IReplyMarkup GetMarkup(IServiceScope actionScope = null)
        {
            InlineKeyboardButton subscriptions = new InlineKeyboardButton() { CallbackData = QueryHelpers.AddQueryString(CommandKeys.Subscriptions, CallbackParams.Page, $"{0}"), Text = resources["SUBSCRIPTIONS_BUTTON"]};
            InlineKeyboardButton people = new InlineKeyboardButton() { CallbackData = CommandKeys.People, Text = resources["PEOPLE_BUTTON"] };
            InlineKeyboardButton userSettings = new InlineKeyboardButton() { CallbackData = CommandKeys.UserSettings, Text = resources["USER_SETTINGS_BUTTON"] };

            InlineKeyboardMarkup result = new InlineKeyboardMarkup(new InlineKeyboardButton[][] {
                new[]
                {
                    people
                },
                new[]
                {
                    subscriptions,
                    userSettings
                }
            });
            return result;
        }
    }
}
