using BirthdayBot.BLL.Resources;
using BirthdayBot.Core.Resources;
using BirthdayBot.DAL.Entities;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using RapidBots.Constants;
using RapidBots.Types.Menus;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Telegram.Bot.Types.ReplyMarkups;

namespace BirthdayBot.BLL.Menus.People
{
    public class OpenChatMenu : PaginationMenu
    {
        private readonly IStringLocalizer<SharedResources> resources;
        private readonly string chatsPage;
        private readonly TUser dbUser;

        public OpenChatMenu(IStringLocalizer<SharedResources> resources, string chatsPage, TUser dbUser) : base(8, 1, CommandKeys.Notes)
        {
            this.resources = resources;
            this.chatsPage = chatsPage;
            this.dbUser = dbUser;
        }

        public override string GetDefaultTitle(IServiceScope actionScope = null, params string[] values)
        {
            return resources["CHATS_TEXT", values];
        }

        public IReplyMarkup GetMarkup(int page, List<ChatMember> source, IServiceScope actionScope = null)
        {
            var result = new List<List<InlineKeyboardButton>>();

            var pageButtons = this.GetPage(page, source, x =>
            {
                var qParams = new Dictionary<string, string>();
                qParams.Add("chatId", $"{x.Chat.Id}");
                qParams.Add("chatPage", $"{page}");
                var subscription = dbUser.Subscriptions.FirstOrDefault(y => y.TargetId == x.UserId);
                if (subscription != null)
                {
                    qParams.Add("targetId", subscription.TargetId.ToString());
                    return new InlineKeyboardButton() { Text = $"{resources["TICK_ICON"]} {x.User.Username ?? string.Format("{0} {1}", x.User.FirstName, x.User.LastName)}", CallbackData = QueryHelpers.AddQueryString(CommandKeys.OpenSubscription, qParams) };
                }
                else
                {
                    qParams.Add("targetId", x.User.Id.ToString());
                    return new InlineKeyboardButton() { Text = x.User.Username ?? string.Format("{0} {1}", x.User.FirstName, x.User.LastName), CallbackData = QueryHelpers.AddQueryString(CommandKeys.ChatSubscriptionPreview, qParams) };
                }
            });

            var backBut = new InlineKeyboardButton() { CallbackData = QueryHelpers.AddQueryString(CommandKeys.AddByChats, CallbackParams.Page, $"{chatsPage}"), Text = resources["BACK_BUTTON"] };

            result.AddRange(pageButtons);
            result.Add(new List<InlineKeyboardButton>() { backBut });
            return new InlineKeyboardMarkup(result);
        }
    }
}
