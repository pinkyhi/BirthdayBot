using BirthdayBot.BLL.Resources;
using BirthdayBot.Core.Resources;
using BirthdayBot.DAL.Entities;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using RapidBots.Constants;
using RapidBots.Types.Menus;
using System.Collections.Generic;
using System.Linq;
using Telegram.Bot.Types.ReplyMarkups;

namespace BirthdayBot.BLL.Menus.People
{
    public class OpenChatMenu : PaginationMenu
    {
        private readonly IStringLocalizer<SharedResources> resources;
        private readonly string chatsPage;
        private readonly TUser dbUser;
        private readonly long chatId;
        private readonly ChatMember chatMember;

        public OpenChatMenu(Dictionary<string, string> qParams, IStringLocalizer<SharedResources> resources, string chatsPage, TUser dbUser, long chatId, ChatMember chatMember) : base(7, 1, CommandKeys.OpenChat, qParams)
        {
            this.resources = resources;
            this.chatsPage = chatsPage;
            this.dbUser = dbUser;
            this.chatId = chatId;
            this.chatMember = chatMember;
        }

        public override string GetDefaultTitle(IServiceScope actionScope = null, params string[] values)
        {
            return resources["OPEN_CHAT_TEXT", values];
        }

        public IReplyMarkup GetMarkup(int page, List<ChatMember> source, IServiceScope actionScope = null)
        {
            var result = new List<List<InlineKeyboardButton>>();

            var pageButtons = this.GetPage(page, source, x =>
            {
                var qParams = new Dictionary<string, string>();
                qParams.Add("chi", $"{x.Chat.Id}");
                qParams.Add("chp", $"{page}");
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

            var groupParams = new Dictionary<string, string>();
            groupParams.Add(CallbackParams.Page, $"{chatsPage}");
            groupParams.Add("chi", chatId.ToString());
            var subAllBut = new InlineKeyboardButton() { CallbackData = QueryHelpers.AddQueryString(CommandKeys.SubscribeAll, groupParams), Text = resources["SUBSCRIBE_ALL_BUTTON"] };
            var unsubAllBut = new InlineKeyboardButton() { CallbackData = QueryHelpers.AddQueryString(CommandKeys.UnsubscribeAll, groupParams), Text = resources["UNSUBSCRIBE_ALL_BUTTON"] };
            if(page == 0)
            {
                InlineKeyboardButton changeChatCalenSub = null;
                var cccsParams = new Dictionary<string, string>();
                cccsParams.Add("chi", chatId.ToString());
                cccsParams.Add("oneTime", "0");
                if (chatMember.IsSubscribedOnCalendar == true)
                {
                    changeChatCalenSub = new InlineKeyboardButton() { CallbackData = QueryHelpers.AddQueryString(CommandKeys.ChangeChatSubscription, cccsParams), Text = resources["CHAT_SUBSCRIPTION_CHANGE_TO_OFF_BUTTON"] };
                }
                else
                {
                    changeChatCalenSub = new InlineKeyboardButton() { CallbackData = QueryHelpers.AddQueryString(CommandKeys.ChangeChatSubscription, cccsParams), Text = resources["CHAT_SUBSCRIPTION_CHANGE_TO_ON_BUTTON"] };
                }
                result.Add(new List<InlineKeyboardButton>() { changeChatCalenSub });
                if (pageButtons.Count > 1)
                {
                    if (!source.All(x => dbUser.Subscriptions.Any(y => y.TargetId == x.UserId)))
                    {
                        result.Add(new List<InlineKeyboardButton>() { subAllBut });
                    }
                    if (source.Any(x => dbUser.Subscriptions.FirstOrDefault(y => y.TargetId == x.UserId) != null))
                    {
                        result.Add(new List<InlineKeyboardButton>() { unsubAllBut });
                    }
                }
            }
            result.AddRange(pageButtons);
            result.Add(new List<InlineKeyboardButton>() { backBut });
            return new InlineKeyboardMarkup(result);
        }
    }
}
