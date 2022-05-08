using BirthdayBot.BLL.Resources;
using BirthdayBot.Core.Resources;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using RapidBots.Types.Menus;
using Telegram.Bot.Types.ReplyMarkups;

namespace BirthdayBot.BLL.Menus.Settings
{
    public class UserSettingsMenu : IMenu
    {
        private readonly IStringLocalizer<SharedResources> resources;

        public UserSettingsMenu(IStringLocalizer<SharedResources> resources)
        {
            this.resources = resources;
        }

        public string GetDefaultTitle(IServiceScope actionScope = null, params string[] values)
        {
            return resources["USER_SETTINGS_TEXT"];
        }

        public IReplyMarkup GetMarkup(IServiceScope actionScope = null)
        {
            InlineKeyboardButton notifSett = new InlineKeyboardButton() { CallbackData = CommandKeys.NotificationsSettings, Text = resources["NOTIFICATIONS_SETTINGS_BUTTON"] };
            InlineKeyboardButton langSett = new InlineKeyboardButton() { CallbackData = CommandKeys.LanguageSettings, Text = resources["LANGUAGE_SETTINGS_BUTTON"] };
            InlineKeyboardButton confidSett = new InlineKeyboardButton() { CallbackData = CommandKeys.ConfidentialitySettings, Text = resources["CONFIDENTIALITY_SETTINGS_BUTTON"] };
            InlineKeyboardButton profileSett = new InlineKeyboardButton() { CallbackData = CommandKeys.ProfileSettings, Text = resources["PROFILE_SETTINGS_BUTTON"] };
            InlineKeyboardButton backBut = new InlineKeyboardButton() { CallbackData = CommandKeys.Start, Text = resources["BACK_BUTTON"] };

            InlineKeyboardMarkup result = new InlineKeyboardMarkup(new InlineKeyboardButton[][] {
                new[]
                {
                    profileSett                    
                },
                new[]
                {
                    notifSett
                },
                new[]
                {
                    confidSett
                }, 
                new[]
                {
                   langSett
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
