using BirthdayBot.BLL.Resources;
using BirthdayBot.Core.Resources;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using RapidBots.Types.Menus;
using Telegram.Bot.Types.ReplyMarkups;

namespace BirthdayBot.BLL.Menus.Settings
{
    public class NotificationsSettingsMenu : IMenu
    {
        private readonly IStringLocalizer<SharedResources> resources;

        public NotificationsSettingsMenu(IStringLocalizer<SharedResources> resources)
        {
            this.resources = resources;
        }

        public string GetDefaultTitle(IServiceScope actionScope = null, params string[] values)
        {
            return resources["NOTIFICATIONS_SETTINGS_TEXT"];
        }

        public IReplyMarkup GetMarkup(IServiceScope actionScope = null)
        {
            InlineKeyboardButton strongNotifBut = new InlineKeyboardButton() { CallbackData = CommandKeys.StrongNotificationsSettings, Text = resources["STRONG_NOTIFICATIONS_BUTTON"] };
            InlineKeyboardButton commonNotifBut = new InlineKeyboardButton() { CallbackData = CommandKeys.CommonNotificationsSettings, Text = resources["COMMON_NOTIFICATIONS_BUTTON"] };
            InlineKeyboardButton notifSettInfo = new InlineKeyboardButton() { CallbackData = CommandKeys.NotificationSettingsInfo, Text = resources["INFO_BUTTON"] };
            InlineKeyboardButton backBut =  new InlineKeyboardButton() { CallbackData = CommandKeys.UserSettings, Text = resources["BACK_BUTTON"] };


            InlineKeyboardMarkup result = new InlineKeyboardMarkup(new InlineKeyboardButton[][] {
                new[]
                {
                    strongNotifBut
                },
                new[]
                {
                    commonNotifBut
                },
                new[]
                {
                    notifSettInfo
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
