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
        private readonly long refId;

        public AddPeopleMenu(IStringLocalizer<SharedResources> resources, string peoplePage, long refId)
        {
            this.peoplePage = peoplePage;
            this.refId = refId;
            this.resources = resources;
        }

        public string GetDefaultTitle(IServiceScope actionScope = null, params string[] values)
        {
            return resources["ADD_PEOPLE_TEXT", values];
        }

        public IReplyMarkup GetMarkup(IServiceScope actionScope = null)
        {

            InlineKeyboardButton chats = new InlineKeyboardButton() { CallbackData = CommandKeys.AddPeopleFromChat, Text = resources["CHATS_BUTTON"] };
            InlineKeyboardButton personal = new InlineKeyboardButton() { CallbackData = CommandKeys.AddPersonal, Text = resources["PERSONAL_BUTTON"] };
            InlineKeyboardButton notTelegram = new InlineKeyboardButton() { CallbackData = CommandKeys.NotTelegramUser, Text = resources["NOT_TELEGRAM_USER_BUTTON"] };
            InlineKeyboardButton byContact = new InlineKeyboardButton() { CallbackData = CommandKeys.AddByContact, Text = resources["ADD_BY_CONTACT_BUTTON"] };
            InlineKeyboardButton SendPersonalRequest = new InlineKeyboardButton() { SwitchInlineQuery = resources["PERSONAL_INVITE_TEXT", string.Format("https://t.me/yourdate_bot?start=refId={0}", refId)], Text = resources["SEND_PERSONAL_INVITE_BUTTON"] };
            InlineKeyboardButton backBut = new InlineKeyboardButton() { CallbackData = QueryHelpers.AddQueryString(CommandKeys.People, CallbackParams.Page, $"{peoplePage}"), Text = resources["BACK_BUTTON"] };

            InlineKeyboardMarkup result = new InlineKeyboardMarkup(new InlineKeyboardButton[][] {
                new[]
                {
                    personal
                },
                new[]
                {
                    byContact
                },
                new[]
                {
                    chats
                },
                new[]
                {
                    notTelegram
                },
                new[]
                {
                    SendPersonalRequest
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

