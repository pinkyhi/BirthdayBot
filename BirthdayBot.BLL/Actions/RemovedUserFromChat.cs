using AutoMapper;
using BirthdayBot.Core.Resources;
using BirthdayBot.DAL.Interfaces;
using Microsoft.EntityFrameworkCore;
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

namespace BirthdayBot.BLL.Actions
{
    public class RemovedUserFromChat : RapidBots.Types.Core.Action
    {
        private readonly BotClient botClient;

        public RemovedUserFromChat(BotClient botClient)
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
                var chat = await repository.GetAsync<DAL.Entities.Chat>(true, x => update.Message.Chat.Id == x.Id, x => x.Include(x => x.ChatMembers));
                var member = chat.ChatMembers.FirstOrDefault(x => x.UserId == update.Message.LeftChatMember.Id);
                if(member != null)
                {
                    chat.ChatMembers.Remove(member);
                    await repository.UpdateAsync(chat);
                }
            }
            catch
            {
                return;
            }
        }

        public override bool ValidateUpdate(Update update)
        {
            return update.Message?.LeftChatMember != null && update.Message.LeftChatMember.IsBot == false;
        }
    }
}
