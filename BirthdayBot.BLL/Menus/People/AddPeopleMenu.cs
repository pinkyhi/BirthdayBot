using BirthdayBot.BLL.Resources;
using BirthdayBot.Core.Resources;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using RapidBots.Constants;
using RapidBots.Types.Menus;
using Telegram.Bot.Types.ReplyMarkups;

namespace BirthdayBot.BLL.Menus.People
{
    public class AddPeopleMenu : IMenu
    {
        private readonly IStringLocalizer<SharedResources> resources;

        public AddPeopleMenu(IStringLocalizer<SharedResources> resources)
        {
            this.resources = resources;
        }

        public string GetDefaultTitle(IServiceScope actionScope = null, params string[] values)
        {
            return resources["ADD_PEOPLE_TEXT", values];
        }

        public IReplyMarkup GetMarkup(IServiceScope actionScope = null)
        {
            InlineKeyboardButton birthDateConfirm = new InlineKeyboardButton() { CallbackData = CommandKeys.Chats, Text = resources["CHATS_BUTTON"] };
            InlineKeyboardButton birthDateReject = new InlineKeyboardButton() { CallbackData = CommandKeys.Personal, Text = resources["PERSONAL_BUTTON"] };
            InlineKeyboardButton backBut = new InlineKeyboardButton() { CallbackData = QueryHelpers.AddQueryString(CommandKeys.People, CallbackParams.Page, $"{0}"), Text = resources["BACK_BUTTON"] };


            InlineKeyboardMarkup result = new InlineKeyboardMarkup(new InlineKeyboardButton[][] {
                new[]
                {
                    birthDateConfirm,
                    birthDateReject
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

