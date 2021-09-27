using BirthdayBot.BLL.Resources;
using BirthdayBot.Core.Resources;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using RapidBots.Types.Menus;
using System.Collections.Generic;
using Telegram.Bot.Types.ReplyMarkups;

namespace BirthdayBot.BLL.Menus.People
{
    public class PersonalNotFoundMenu : IMenu
    {
        private readonly IStringLocalizer<SharedResources> resources;
        private readonly string title;
        private readonly long refId;

        public PersonalNotFoundMenu(IStringLocalizer<SharedResources> resources, string title, long refId)
        {
            this.resources = resources;
            this.title = title;
            this.refId = refId;
        }

        public string GetDefaultTitle(IServiceScope actionScope = null, params string[] values)
        {
            return resources["ADD_PERSONAL_INPUT_NOT_FOUND", title];
        }

        public IReplyMarkup GetMarkup(IServiceScope actionScope = null)
        {
            InlineKeyboardButton AddManually = new InlineKeyboardButton() { CallbackData = $"{CommandKeys.AddNote}?title={title}", Text = resources["ADD_MANUALLY_BUTTON"] };
            InlineKeyboardButton SendPersonalRequest = new InlineKeyboardButton() { SwitchInlineQuery = resources["PERSONAL_INVITE_TEXT", string.Format("https://t.me/yourdate_bot?start=refId={0}", refId)], Text = resources["SEND_PERSONAL_INVITE_BUTTON"] };

            InlineKeyboardMarkup result = new InlineKeyboardMarkup(new InlineKeyboardButton[][] {
                new[]
                {
                    AddManually,
                    SendPersonalRequest
                }
            });
            return result;
        }
    }
}
