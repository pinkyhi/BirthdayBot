﻿using AutoMapper;
using BirthdayBot.BLL.Inputs.Start;
using BirthdayBot.BLL.Menus;
using BirthdayBot.BLL.Resources;
using BirthdayBot.Core.Resources;
using BirthdayBot.DAL.Entities;
using BirthdayBot.DAL.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using RapidBots.Extensions;
using RapidBots.Types.Attributes;
using RapidBots.Types.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace BirthdayBot.BLL.Commands.General
{
    [ChatType(ChatType.Group, ChatType.Supergroup)]
    public class CalendarChat : Command
    {
        private readonly BotClient botClient;

        public CalendarChat(BotClient botClient)
        {
            this.botClient = botClient;
        }

        public override string Key => CommandKeys.Calendar;

        public override async Task Execute(Update update, TelegramUser user = null, IServiceScope actionScope = null)
        {
            var repository = actionScope.ServiceProvider.GetService<IRepository>();
            var actionsManager = actionScope.ServiceProvider.GetService<ActionManager>();
            var resources = actionScope.ServiceProvider.GetService<IStringLocalizer<SharedResources>>();

            TUser dbUser = (user as TUser) ?? await repository.GetAsync<TUser>(false, u => u.Id == update.CallbackQuery.Message.Chat.Id, include: u => u.Include(x => x.ChatMembers).ThenInclude(x => x.User));

            long chatId = update.Message.Chat.Id;
            var chat = await repository.GetAsync<DAL.Entities.Chat>(false, c => c.Id == chatId, x => x.Include(x => x.ChatMembers).ThenInclude(x => x.User));

            var users = chat.ChatMembers.Select(x => new { Name = $"@{x.User.Username}" ?? $"{x.User.FirstName} {x.User.LastName}", Date = x.User.BirthDate }).GroupBy(x => x.Date.Month - 1);
            string format = "{0} - {1};\n";
            int monthNow = DateTime.Now.Month;

            List<string> monthes = new List<string>(){
                resources["JANUARY"],
                resources["FEBRUARY"],
                resources["MARCH"],
                resources["APRIL"],
                resources["MAY"],
                resources["JUNE"],
                resources["JULY"],
                resources["AUGUST"],
                resources["SEPTEMBER"],
                resources["OCTOBER"],
                resources["NOVEMBER"],
                resources["DECEMBER"]
            };

            var resultStr = $"{resources["CHAT_CALENDAR_MENU_TEXT"]}\n";
            for(int i = 0; i < 12; i++)
            {
                int month = (monthNow + i) % 12;
                var usersNow = users.FirstOrDefault(x => x.Key == month);
                if(usersNow == null)
                {
                    continue;
                }
                var strs = $"<b>{monthes[month]}</b>\n";
                foreach(var userNow in usersNow)
                {
                    strs += $"{string.Format(format, userNow.Name, userNow.Date.ToShortDateString())}";
                }
                resultStr += strs;
            }
            await botClient.SendTextMessageAsync(update.Message?.Chat?.Id ?? update.CallbackQuery.Message.Chat.Id, resultStr, parseMode: ParseMode.Html, disableNotification: true);
        }
    }
}
