using BirthdayBot.BLL.Resources;
using BirthdayBot.Core.Resources;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using RapidBots.Types.Menus;
using System.Globalization;
using Telegram.Bot.Types.ReplyMarkups;
namespace BirthdayBot.BLL.Menus.Settings
{
    public class LanguageSettingsMenu : IMenu
    {
        private readonly IStringLocalizer<SharedResources> resources;

        public LanguageSettingsMenu(IStringLocalizer<SharedResources> resources)
        {
            this.resources = resources;
        }

        public string GetDefaultTitle(IServiceScope actionScope = null, params string[] values)
        {
            return resources["LANGUAGE_SETTINGS_TEXT"];
        }

        public IReplyMarkup GetMarkup(IServiceScope actionScope = null)
        {
            InlineKeyboardButton enBut = new InlineKeyboardButton() { CallbackData = QueryHelpers.AddQueryString(CommandKeys.LanguageSettingsChange, "lang", "en"), Text = resources["ENGLISH_LANGUAGE_BUTTON"] };
            InlineKeyboardButton ruBut = new InlineKeyboardButton() { CallbackData = QueryHelpers.AddQueryString(CommandKeys.LanguageSettingsChange, "lang", "ru"), Text = resources["RUSSIAN_LANGUAGE_BUTTON"] };
            InlineKeyboardButton ukBut = new InlineKeyboardButton() { CallbackData = QueryHelpers.AddQueryString(CommandKeys.LanguageSettingsChange, "lang", "uk"), Text = resources["UKRAINIAN_LANGUAGE_BUTTON"] };

            switch (CultureInfo.CurrentCulture.TwoLetterISOLanguageName)
            {
                case "en":
                    enBut.Text += "✔";
                    break;
                case "ru":
                    ruBut.Text += "✔";
                    break;
                case "uk":
                    ukBut.Text += "✔";
                    break;
            }


            InlineKeyboardButton backBut = new InlineKeyboardButton() { CallbackData = CommandKeys.UserSettings, Text = resources["BACK_BUTTON"] };


            InlineKeyboardMarkup result = new InlineKeyboardMarkup(new InlineKeyboardButton[][] {
                new[]
                {
                    enBut
                },
                new[]
                {
                    ruBut
                },
                new[]
                {
                    ukBut
                },
                new[]
                {
                    backBut
                }
            });
            return result;
        }
    }
}
