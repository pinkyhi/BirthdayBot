using AutoMapper;
using BirthdayBot.BLL.Inputs.Start;
using BirthdayBot.BLL.Menus;
using BirthdayBot.BLL.Resources;
using BirthdayBot.Core.Resources;
using BirthdayBot.DAL.Entities;
using BirthdayBot.DAL.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using Newtonsoft.Json;
using RapidBots.Extensions;
using RapidBots.Types.Attributes;
using RapidBots.Types.Core;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace BirthdayBot.BLL.Commands
{
    [ChatType(ChatType.Private)]
    [ExpectedParams("0")]
    public class StartFromChat : Command
    {
        private readonly IMapper mapper;
        private readonly BotClient botClient;

        public StartFromChat(IMapper mapper, BotClient botClient)
        {
            this.botClient = botClient;
            this.mapper = mapper;
        }

        public override string Key => CommandKeys.Start;

        public override async Task Execute(Update update, TelegramUser user = null, IServiceScope actionScope = null)
        {
            long chatId = Convert.ToInt64(update.GetParams()["0"]);
            var repository = actionScope.ServiceProvider.GetService<IRepository>();
            var actionsManager = actionScope.ServiceProvider.GetService<ActionManager>();
            var resources = actionScope.ServiceProvider.GetService<IStringLocalizer<SharedResources>>();

            TUser dbUser = user as TUser ?? await repository.GetAsync<TUser>(true, u => u.Id == (update.Message?.From.Id ?? update.CallbackQuery.From.Id));

            if (dbUser == null)
            {
                TUser newUser = mapper.Map<TUser>(update.Message?.From ?? update.CallbackQuery?.From);
                dbUser = await repository.AddAsync(newUser);
            }

            // Zeroing
            dbUser.CurrentStatus = null;
            dbUser.MiddlewareData = null;
            await repository.UpdateAsync(dbUser);

            var openerMessage = await botClient.SendTextMessageAsync(update.Message?.Chat?.Id ?? update.CallbackQuery.Message.Chat.Id, resources["MENU_OPENER_TEXT"], replyMarkup: new ReplyKeyboardRemove(), parseMode: Telegram.Bot.Types.Enums.ParseMode.Html, disableNotification: true);
            await botClient.DeleteMessageAsync(openerMessage.Chat.Id, openerMessage.MessageId);

            if (dbUser.RegistrationDate == null)
            {
                var data = new Dictionary<string, string>();
                data.Add("fromChat", chatId.ToString());
                dbUser.MiddlewareData = JsonConvert.SerializeObject(data);
                await repository.UpdateAsync(dbUser);
                dbUser.CurrentStatus = actionsManager.FindInputStatusByType<BirthYearInput>();
                await repository.UpdateAsync(dbUser);
                await botClient.SendTextMessageAsync(update.Message?.Chat?.Id ?? update.CallbackQuery.Message.Chat.Id, resources["BIRTH_YEAR_INPUT"], replyMarkup: new ReplyKeyboardRemove() { Selective = false }, parseMode: Telegram.Bot.Types.Enums.ParseMode.Html);
            }
            else
            {
                var chat = await repository.GetAsync<DAL.Entities.Chat>(true, x => x.Id == chatId, x => x.Include(u => u.ChatMembers).ThenInclude(x => x.User));
                chat.ChatMembers.Add(new DAL.Entities.ChatMember() { User = dbUser, AddingDate = DateTime.Now.Date });
                try
                {
                    await repository.UpdateAsync(chat);
                }
                catch (InvalidOperationException)
                {}
                StartMenu menu = new StartMenu(resources);
                await botClient.SendTextMessageAsync(update.Message?.Chat?.Id ?? update.CallbackQuery.Message.Chat.Id, resources["SUCCESS_START_FROM_CHAT", chat.Title], parseMode: ParseMode.Html);
                var chatMemberCount = await botClient.GetChatMembersCountAsync(chatId) - 1;
                if(chatMemberCount == chat.ChatMembers.Count)
                {
                    await botClient.SendTextMessageAsync(chatId, resources["ALL_USERS_ADDED_TEXT", chat.Title], parseMode: ParseMode.Html);
                }
                await botClient.SendTextMessageAsync(update.Message?.Chat?.Id ?? update.CallbackQuery.Message.Chat.Id, menu.GetDefaultTitle(actionScope, dbUser.Username ?? dbUser.FirstName), replyMarkup: menu.GetMarkup(actionScope), parseMode: ParseMode.Html);
            }
        }
    }
}
