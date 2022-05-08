using BirthdayBot.BLL.Resources;
using BirthdayBot.Core.Resources;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using RapidBots.Types.Menus;
using Telegram.Bot.Types.ReplyMarkups;

namespace BirthdayBot.BLL.Menus.Notes
{
    public class NoteDateConfirmationMenu : IMenu
    {
        private readonly IStringLocalizer<SharedResources> resources;

        public NoteDateConfirmationMenu(IStringLocalizer<SharedResources> resources)
        {
            this.resources = resources;
        }

        public string GetDefaultTitle(IServiceScope actionScope = null, params string[] values)
        {
            return resources["NOTE_DATE_CONFIRMATION", values];
        }

        public IReplyMarkup GetMarkup(IServiceScope actionScope = null)
        {
            InlineKeyboardButton birthDateConfirm = new InlineKeyboardButton() { CallbackData = CommandKeys.NoteDateConfirm, Text = resources["CONFIRM_BUTTON"] };
            InlineKeyboardButton birthDateReject = new InlineKeyboardButton() { CallbackData = CommandKeys.NoteDateReject, Text = resources["REJECT_BUTTON"] };

            InlineKeyboardMarkup result = new InlineKeyboardMarkup(new InlineKeyboardButton[][] {
                new[]
                {
                    birthDateConfirm,
                    birthDateReject
                }
            });
            return result;
        }
    }
}
