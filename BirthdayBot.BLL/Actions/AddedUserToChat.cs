using AutoMapper;
using BirthdayBot.Core.Resources;
using BirthdayBot.DAL.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using RapidBots.Types.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace BirthdayBot.BLL.Actions
{
    public class AddedUserToChat : RapidBots.Types.Core.Action
    {
        private readonly BotClient botClient;

        public AddedUserToChat(BotClient botClient)
        {
            this.botClient = botClient;
        }

        public async override Task Execute(Update update, TelegramUser user = null, IServiceScope actionScope = null)
        {
            var resources = actionScope.ServiceProvider.GetService<IStringLocalizer<SharedResources>>();
            InlineKeyboardButton joinChatCalendar = new InlineKeyboardButton() { Text = resources["JOIN_CHAT_CALENDAR_BUTTON"], Url = string.Format("https://t.me/birthdayMaster_bot?start={0}", update.Message.Chat.Id) };

            InlineKeyboardMarkup markup = new InlineKeyboardMarkup(new InlineKeyboardButton[][] {
                new[]
                {
                    joinChatCalendar
                }
            }); try
            {
                string mentionsInlineString = "";
                foreach(var chatMember in update.Message.NewChatMembers)
                {
                    mentionsInlineString += string.Format("[{0}](tg://user?id={1})", chatMember.Username ?? chatMember.FirstName ?? chatMember.LastName ?? resources["USER_PLACEHOLDER"], chatMember.Id) + ", ";
                }
                mentionsInlineString = mentionsInlineString.Substring(0, mentionsInlineString.Length - 2);
                await botClient.SendTextMessageAsync(update.Message.Chat.Id, resources["ADDED_USER_TO_CHAT_TEXT", mentionsInlineString], parseMode: ParseMode.Markdown, replyMarkup: markup);
            }
            catch
            {
                await botClient.SendTextMessageAsync(update.Message.Chat.Id, resources["ADDED_USER_TO_CHAT_ERROR"], parseMode: ParseMode.Markdown, replyMarkup: markup);
                return;
            }
        }

        public override bool ValidateUpdate(Update update)
        {
            if(update.Message?.NewChatMembers != null)
            {
                var newMembers = update.Message.NewChatMembers.Where(x => !x.IsBot);
                return newMembers.Count() > 0;
            }
            return false;
        }
    }
}
