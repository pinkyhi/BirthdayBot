using BirthdayBot.BLL.Resources;
using BirthdayBot.Core.Resources;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using RapidBots.Types.Menus;
using Telegram.Bot.Types.ReplyMarkups;

namespace BirthdayBot.BLL.Menus.Settings
{
    public class ConfidentialitySettingsMenu : IMenu
    {
        private readonly IStringLocalizer<SharedResources> resources;

        public ConfidentialitySettingsMenu(IStringLocalizer<SharedResources> resources)
        {
            this.resources = resources;
        }

        public string GetDefaultTitle(IServiceScope actionScope = null, params string[] values)
        {
            return resources["CONFIDENTIALITY_SETTINGS_TEXT", values];
        }

        public IReplyMarkup GetMarkup(IServiceScope actionScope = null)
        {
            InlineKeyboardButton ageConfidBut = new InlineKeyboardButton() { CallbackData = CommandKeys.AgeConfidentialityChange, Text = resources["AGE_CONFIDENTIALITY_BUTTON"] };
            InlineKeyboardButton confidInfoBut = new InlineKeyboardButton() { CallbackData = CommandKeys.ConfidentialitySettingsInfo, Text = resources["INFO_BUTTON"] };
            InlineKeyboardButton backBut = new InlineKeyboardButton() { CallbackData = CommandKeys.UserSettings, Text = resources["BACK_BUTTON"] };

            InlineKeyboardMarkup result = new InlineKeyboardMarkup(new InlineKeyboardButton[][] {
                new[]
                {
                    ageConfidBut
                },
                new[]
                {
                    confidInfoBut
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
