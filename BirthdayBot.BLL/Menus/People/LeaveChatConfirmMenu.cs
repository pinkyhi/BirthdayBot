using BirthdayBot.BLL.Resources;
using BirthdayBot.Core.Resources;
using BirthdayBot.DAL.Entities;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using RapidBots.Constants;
using RapidBots.Types.Menus;
using System.Collections.Generic;
using Telegram.Bot.Types.ReplyMarkups;

namespace BirthdayBot.BLL.Menus.People
{
    public class LeaveChatConfirmMenu : IMenu
    {
        private readonly IStringLocalizer<SharedResources> resources;
        private readonly Dictionary<string, string> qParams;
        private readonly Chat chat;

        public LeaveChatConfirmMenu(IStringLocalizer<SharedResources> resources, Dictionary<string, string> qParams, Chat chat)
        {
            this.resources = resources;
            this.qParams = qParams;
            this.chat = chat;
        }

        public string GetDefaultTitle(IServiceScope actionScope = null, params string[] values)
        {
            return resources["LEAVE_CHAT_CONFIRM_TEXT", chat.Title];
        }

        public IReplyMarkup GetMarkup(IServiceScope actionScope = null)
        {
            var leaveDict = new Dictionary<string, string>();
            leaveDict.Add("chi", qParams["chi"]);
            leaveDict.Add("chsP", qParams["chsP"]);

            InlineKeyboardButton leave = new InlineKeyboardButton() { CallbackData = QueryHelpers.AddQueryString(CommandKeys.LeaveChatCalendarConfirmation, leaveDict), Text = resources["LEAVE_BUTTON"] };
            InlineKeyboardButton back = new InlineKeyboardButton() { CallbackData = QueryHelpers.AddQueryString(CommandKeys.OpenChat, qParams), Text = resources["BACK_BUTTON"] };

            InlineKeyboardMarkup result = new InlineKeyboardMarkup(new InlineKeyboardButton[][] {
                new[]
                {
                    leave,
                    back
                }
            });
            return result;
        }
    }
}
