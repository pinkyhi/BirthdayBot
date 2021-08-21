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
    public class ChatTitleChange : RapidBots.Types.Core.Action
    {
        public ChatTitleChange()
        {
        }

        public async override Task Execute(Update update, TelegramUser user = null, IServiceScope actionScope = null)
        {
            var repository = actionScope.ServiceProvider.GetService<IRepository>();
            var mapper = actionScope.ServiceProvider.GetService<IMapper>();
            var resources = actionScope.ServiceProvider.GetService<IStringLocalizer<SharedResources>>();
            try
            {
                var chat = await repository.GetAsync<DAL.Entities.Chat>(false, x => x.Id == update.Message.Chat.Id);
                chat.Title = update.Message.NewChatTitle;
                await repository.UpdateAsync(chat);
            }
            catch
            {
                return;
            }
        }

        public override bool ValidateUpdate(Update update)
        {
            return update.Message?.NewChatTitle != null;
        }
    }
}
