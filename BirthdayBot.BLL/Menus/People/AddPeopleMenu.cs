using BirthdayBot.BLL.Resources;
using BirthdayBot.Core.Resources;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using RapidBots.Constants;
using RapidBots.Types.Menus;
using System.Collections.Generic;
using Telegram.Bot.Types.ReplyMarkups;

namespace BirthdayBot.BLL.Menus.People
{
    public class AddPeopleMenu : IMenu
    {
        private readonly IStringLocalizer<SharedResources> resources;
        private readonly string peoplePage;

        public AddPeopleMenu(IStringLocalizer<SharedResources> resources, string peoplePage)
        {
            this.peoplePage = peoplePage;
            this.resources = resources;
        }

        public string GetDefaultTitle(IServiceScope actionScope = null, params string[] values)
        {
            return resources["ADD_PEOPLE_TEXT", values];
        }

        public IReplyMarkup GetMarkup(IServiceScope actionScope = null)
        {
            Dictionary<string, string> qParams = new Dictionary<string, string>();
            qParams.Add(CallbackParams.Page, "0");
            InlineKeyboardButton chats = new InlineKeyboardButton() { CallbackData = QueryHelpers.AddQueryString(CommandKeys.AddByChats, qParams), Text = resources["CHATS_BUTTON"] };
            InlineKeyboardButton personal = new InlineKeyboardButton() { CallbackData = CommandKeys.AddPersonal, Text = resources["PERSONAL_BUTTON"] };
            InlineKeyboardButton notTelegram = new InlineKeyboardButton() { CallbackData = CommandKeys.NotTelegramUser, Text = resources["NOT_TELEGRAM_USER_BUTTON"] };
            InlineKeyboardButton byContact = new InlineKeyboardButton() { CallbackData = CommandKeys.AddByContact, Text = resources["ADD_BY_CONTACT_BUTTON"] };
            InlineKeyboardButton backBut = new InlineKeyboardButton() { CallbackData = QueryHelpers.AddQueryString(CommandKeys.People, CallbackParams.Page, $"{peoplePage}"), Text = resources["BACK_BUTTON"] };

            InlineKeyboardMarkup result = new InlineKeyboardMarkup(new InlineKeyboardButton[][] {
                new[]
                {
                    chats,
                    personal
                },
                new[]
                {
                    notTelegram
                },
                new[]
                {
                    byContact
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

