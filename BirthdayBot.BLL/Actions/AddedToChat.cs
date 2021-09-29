using AutoMapper;
using BirthdayBot.Core.Resources;
using BirthdayBot.DAL.Entities;
using BirthdayBot.DAL.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using RapidBots.Types.Core;
using System;
using System.Globalization;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace BirthdayBot.BLL.Actions
{
    public class AddedToChat : RapidBots.Types.Core.Action
    {
        private readonly BotClient botClient;

        public AddedToChat(BotClient botClient)
        {
            this.botClient = botClient;
        }

        public async override Task Execute(Update update, TelegramUser user = null, IServiceScope actionScope = null)
        {
            var repository = actionScope.ServiceProvider.GetService<IRepository>();
            var mapper = actionScope.ServiceProvider.GetService<IMapper>();
            var resources = actionScope.ServiceProvider.GetService<IStringLocalizer<SharedResources>>();

            string telegramUserLanguageCode = update.MyChatMember?.From?.LanguageCode;

            try
            {
                var tUser = await repository.GetAsync<TUser>(false, x => x.Id == update.MyChatMember.From.Id);
                telegramUserLanguageCode = tUser.LanguageCode;
            }
            catch
            {}

            if (!string.IsNullOrEmpty(telegramUserLanguageCode))
            {
                CultureInfo.CurrentCulture = new CultureInfo(telegramUserLanguageCode);
                CultureInfo.CurrentUICulture = new CultureInfo(telegramUserLanguageCode);
            }

            try
            {
                var chat = mapper.Map<DAL.Entities.Chat>(update.MyChatMember.Chat);
                chat.AddingDate = DateTime.Now;
                await repository.AddAsync(chat);
            }
            catch
            {
                await botClient.SendTextMessageAsync(update.MyChatMember.Chat.Id, resources["ADDED_TO_CHAT_ERROR"], parseMode: Telegram.Bot.Types.Enums.ParseMode.Html);
                return;
            }

            InlineKeyboardButton joinChatCalendar = new InlineKeyboardButton() { Text = resources["JOIN_CHAT_CALENDAR_BUTTON"], Url = string.Format("https://t.me/yourdate_bot?start={0}", update.MyChatMember.Chat.Id) };
            await botClient.SendTextMessageAsync(update.MyChatMember.Chat.Id, resources["ADDED_TO_CHAT_TEXT"], replyMarkup: new InlineKeyboardMarkup(joinChatCalendar), parseMode: Telegram.Bot.Types.Enums.ParseMode.Html);
        }

        public override bool ValidateUpdate(Update update)
        {
            if(update.Type == UpdateType.MyChatMember && update.MyChatMember.NewChatMember.User.Id == botClient.Me.Id && update.MyChatMember.OldChatMember.User.Id == botClient.Me.Id)
            {
                if(update.MyChatMember.Chat.Type == ChatType.Group || update.MyChatMember.Chat.Type == ChatType.Supergroup)
                {
                    if ((update.MyChatMember.NewChatMember.Status == ChatMemberStatus.Member || update.MyChatMember.NewChatMember.Status == ChatMemberStatus.Administrator) && (update.MyChatMember.OldChatMember.Status != ChatMemberStatus.Member && update.MyChatMember.OldChatMember.Status != ChatMemberStatus.Administrator))
                    {
                        return true;
                    }
                }
            }
            return false;
        }
    }
}
