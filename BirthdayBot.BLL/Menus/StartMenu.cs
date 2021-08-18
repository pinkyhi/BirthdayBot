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
            return resources["WELCOME_TEXT", values];
        }

        public IReplyMarkup GetMarkup(IServiceScope actionScope = null)
        {
            InlineKeyboardButton people = new InlineKeyboardButton() { CallbackData = QueryHelpers.AddQueryString(CommandKeys.People, CallbackParams.Page, $"{0}"), Text = resources["PEOPLE_BUTTON"] };
            InlineKeyboardButton notes = new InlineKeyboardButton() { CallbackData = QueryHelpers.AddQueryString(CommandKeys.Notes, CallbackParams.Page, $"{0}"), Text = resources["NOTES_BUTTON"]};
            InlineKeyboardButton userSettings = new InlineKeyboardButton() { CallbackData = CommandKeys.UserSettings, Text = resources["USER_SETTINGS_BUTTON"] };
            InlineKeyboardButton startGroup = new InlineKeyboardButton() { Text = resources["SHARE_TO_GROUP"], Url = "https://t.me/birthdayMaster_bot?startgroup" };

            InlineKeyboardMarkup result = new InlineKeyboardMarkup(new InlineKeyboardButton[][] {
                new[]
                {
                    people,
                    notes
                },
                new[]
                {
                    userSettings
                },
                new[]
                {
                    startGroup
                }
            });
            return result;
        }
    }
}
