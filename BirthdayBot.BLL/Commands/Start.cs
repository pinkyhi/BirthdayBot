using AutoMapper;
using BirthdayBot.BLL.Inputs.Start;
using BirthdayBot.BLL.Menus;
using BirthdayBot.BLL.Resources;
using BirthdayBot.Core.Resources;
using BirthdayBot.DAL.Entities;
using BirthdayBot.DAL.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using RapidBots.Types.Core;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace BirthdayBot.BLL.Commands
{
    class Start : ICommand
    {
        private readonly IMapper mapper;
        private readonly BotClient botClient;

        public Start(IMapper mapper, BotClient botClient)
        {
            this.botClient = botClient;
            this.mapper = mapper;
        }

        public string Key => CommandKeys.Start;

        public async Task Execute(Update update, TelegramUser user = null, IServiceScope actionScope = null)
        {
            var repository = actionScope.ServiceProvider.GetService<IRepository>();
            var actionsManager = actionScope.ServiceProvider.GetService<ActionManager>();
            var resources = actionScope.ServiceProvider.GetService<IStringLocalizer<SharedResources>>();

            TUser dbUser = user as TUser ?? await repository.GetAsync<TUser>(false, u => u.Id == update.CallbackQuery.From.Id);
            if (dbUser == null)
            {
                TUser newUser = mapper.Map<TUser>(update.Message.From);
                dbUser = await repository.AddAsync(newUser);
            }

            if (dbUser.RegistrationDate == null)
            {
                dbUser.CurrentStatus = actionsManager.FindInputStatusByType<BirthYearInput>();
                await repository.UpdateAsync(dbUser);
                await botClient.SendTextMessageAsync(update.Message.Chat.Id, resources["BIRTH_YEAR_INPUT"], replyMarkup: new ReplyKeyboardRemove() { Selective = false });
            }
            else
            {
                StartMenu menu = new StartMenu(resources);
                await botClient.SendTextMessageAsync(update.Message.Chat.Id, menu.GetDefaultTitle(actionScope, dbUser.Username), replyMarkup: menu.GetMarkup(actionScope));
            }
        }
    }
}
