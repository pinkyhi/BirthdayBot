using AutoMapper;
using BirthdayBot.Core.Resources;
using BirthdayBot.DAL.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using RapidBots.Types.Core;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace BirthdayBot.BLL.Actions
{
    public class RemovedFromChat : RapidBots.Types.Core.Action
    {
        private readonly BotClient botClient;

        public RemovedFromChat(BotClient botClient)
        {
            this.botClient = botClient;
        }

        public async override Task Execute(Update update, TelegramUser user = null, IServiceScope actionScope = null)
        {
            var repository = actionScope.ServiceProvider.GetService<IRepository>();
            var mapper = actionScope.ServiceProvider.GetService<IMapper>();
            var resources = actionScope.ServiceProvider.GetService<IStringLocalizer<SharedResources>>();
            try
            {
                var chat = await repository.GetAsync<DAL.Entities.Chat>(false, x => update.MyChatMember.Chat.Id == x.Id);
                await repository.DeleteAsync(chat);
            }
            catch
            {
                await botClient.SendTextMessageAsync(update.MyChatMember.Chat.Id, resources["ADDED_TO_CHAT_ERROR"]);
            }
            await botClient.SendTextMessageAsync(update.MyChatMember.Chat.Id, resources["ADDED_TO_CHAT_TEXT"]);
        }

        public override bool ValidateUpdate(Update update)
        {
            if (update.Type == UpdateType.MyChatMember && update.MyChatMember.NewChatMember.User.Id == botClient.Me.Id && update.MyChatMember.OldChatMember.User.Id == botClient.Me.Id)
            {
                if ((update.MyChatMember.NewChatMember.Status == ChatMemberStatus.Left || update.MyChatMember.NewChatMember.Status == ChatMemberStatus.Kicked || update.MyChatMember.NewChatMember.Status == ChatMemberStatus.Restricted) && (update.MyChatMember.OldChatMember.Status == ChatMemberStatus.Member || update.MyChatMember.OldChatMember.Status == ChatMemberStatus.Administrator))
                {
                    return true;
                }
            }
            return false;
        }
    }
}
