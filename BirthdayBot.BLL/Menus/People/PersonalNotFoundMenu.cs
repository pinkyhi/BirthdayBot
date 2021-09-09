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

        public PersonalNotFoundMenu(IStringLocalizer<SharedResources> resources, string title)
        {
            this.resources = resources;
            this.title = title;
        }

        public string GetDefaultTitle(IServiceScope actionScope = null, params string[] values)
        {
            return resources["ADD_PERSONAL_INPUT_NOT_FOUND", title];
        }

        public IReplyMarkup GetMarkup(IServiceScope actionScope = null)
        {
            var qParams = new Dictionary<string, string>();
            qParams.Add("title", title);

            InlineKeyboardButton AddManually = new InlineKeyboardButton() { CallbackData = QueryHelpers.AddQueryString(CommandKeys.AddNote, qParams), Text = resources["ADD_MANUALLY_BUTTON"] };
            InlineKeyboardButton SendPersonalRequest = new InlineKeyboardButton() { SwitchInlineQuery = resources["PERSONAL_INVITE_TEXT"], Text = resources["SEND_PERSONAL_INVITE_BUTTON"] };

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
