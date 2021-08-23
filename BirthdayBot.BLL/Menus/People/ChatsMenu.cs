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
    public class ChatsMenu : PaginationMenu
    {
        private readonly IStringLocalizer<SharedResources> resources;
        private readonly string peoplePage;

        public ChatsMenu(IStringLocalizer<SharedResources> resources, string peoplePage) : base(8, 1, CommandKeys.Notes)
        {
            this.resources = resources;
            this.peoplePage = peoplePage;
        }

        public override string GetDefaultTitle(IServiceScope actionScope = null, params string[] values)
        {
            return resources["CHATS_TEXT", values];
        }

        public IReplyMarkup GetMarkup(int page, List<Chat> source, IServiceScope actionScope = null)
        {
            var result = new List<List<InlineKeyboardButton>>();
            
            var pageButtons = this.GetPage(page, source, x =>
            {

                var qParams = new Dictionary<string, string>();
                qParams.Add("peoplePage", $"{peoplePage}");
                qParams.Add("chatsPage", $"{page}");
                qParams.Add("chatId", $"{x.Id}");
                if(x.ChatMembers.Count() > 1)
                {
                    return new InlineKeyboardButton() { Text = x.Title, CallbackData = QueryHelpers.AddQueryString(CommandKeys.OpenSubscription, qParams) };
                }
                else
                {
                    return new InlineKeyboardButton() { Text = $"{resources["WARNING_ICON"]} {x.Title}", CallbackData = QueryHelpers.AddQueryString(CommandKeys.OpenChat, qParams) };
                }
            });

            var backBut = new InlineKeyboardButton() { CallbackData = QueryHelpers.AddQueryString(CommandKeys.AddPeople, "peoplePage", peoplePage), Text = resources["BACK_BUTTON"] };

            result.AddRange(pageButtons);
            result.Add(new List<InlineKeyboardButton>() { backBut });
            return new InlineKeyboardMarkup(result);
        }
    }
}