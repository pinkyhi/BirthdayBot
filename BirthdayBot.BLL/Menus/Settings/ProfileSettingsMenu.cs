using BirthdayBot.BLL.Resources;
using BirthdayBot.Core.Resources;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using RapidBots.Types.Menus;
using Telegram.Bot.Types.ReplyMarkups;

namespace BirthdayBot.BLL.Menus.Settings
{
    public class ProfileSettingsMenu : IMenu
    {
        private readonly IStringLocalizer<SharedResources> resources;

        public ProfileSettingsMenu(IStringLocalizer<SharedResources> resources)
        {
            this.resources = resources;
        }

        public string GetDefaultTitle(IServiceScope actionScope = null, params string[] values)
        {
            return resources["PROFILE_SETTINGS_TEXT", values];
        }

        public IReplyMarkup GetMarkup(IServiceScope actionScope = null)
        {
            InlineKeyboardButton changeBirthDate = new InlineKeyboardButton() { CallbackData = CommandKeys.ChangeBirthDate, Text = resources["CHANGE_BIRTH_DATE_BUTTON"] };
            InlineKeyboardButton changeLocation = new InlineKeyboardButton() { CallbackData = CommandKeys.ChangeLocation, Text = resources["CHANGE_LOCATION_BUTTON"] };
            InlineKeyboardButton backBut = new InlineKeyboardButton() { CallbackData = CommandKeys.UserSettings, Text = resources["BACK_BUTTON"] };

            InlineKeyboardMarkup result = new InlineKeyboardMarkup(new InlineKeyboardButton[][] {
                new[]
                {
                    changeBirthDate
                },
                new[]
                {
                    changeLocation
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
