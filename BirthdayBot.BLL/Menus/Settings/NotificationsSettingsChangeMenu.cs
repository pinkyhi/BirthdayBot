using BirthdayBot.BLL.Resources;
using BirthdayBot.Core.Resources;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using RapidBots.Types.Menus;
using System.Collections.Generic;
using Telegram.Bot.Types.ReplyMarkups;

namespace BirthdayBot.BLL.Menus.Settings
{
    public class NotificationsSettingsChangeMenu : IMenu
    {
        private readonly IStringLocalizer<SharedResources> resources;
        private readonly Dictionary<int, int> delays;

        public NotificationsSettingsChangeMenu(IStringLocalizer<SharedResources> resources, Dictionary<int, int> delays)
        {
            this.delays = delays;
            this.resources = resources;
        }

        public string GetDefaultTitle(IServiceScope actionScope = null, params string[] values)
        {
            return resources["NOTIFICATIONS_SETTINGS_CHANGE_TEXT"];
        }

        public IReplyMarkup GetMarkup(IServiceScope actionScope = null)
        {
            List<List<InlineKeyboardButton>> keyboard = new List<List<InlineKeyboardButton>>();
            foreach(var pair in this.delays)
            {
                List<InlineKeyboardButton> row = new List<InlineKeyboardButton>();
                var pairCallback = QueryHelpers.AddQueryString(CommandKeys.NotificationsSettingsChange, "property", $"{pair.Key}");
                row.Add(new InlineKeyboardButton() { CallbackData = pairCallback, Text = resources["NOTIFICATIONS_SETTINGS_CHANGE_BUTTON", pair.Value] });
                keyboard.Add(row);
            }
            keyboard.Add(new List<InlineKeyboardButton>() { new InlineKeyboardButton() { CallbackData = CommandKeys.NotificationsSettings, Text = resources["BACK_BUTTON"] } });

            InlineKeyboardMarkup result = new InlineKeyboardMarkup(keyboard);
            return result;
        }
    }
}
