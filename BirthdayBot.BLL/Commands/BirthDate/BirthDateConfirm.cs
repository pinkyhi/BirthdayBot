﻿using AutoMapper;
using BirthdayBot.BLL.Inputs.Start;
using BirthdayBot.BLL.Resources;
using BirthdayBot.Core.Resources;
using BirthdayBot.Core.Types;
using BirthdayBot.DAL.Entities;
using BirthdayBot.DAL.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using RapidBots.Types.Core;
using System;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace BirthdayBot.BLL.Commands.BirthDate
{
    public class BirthDateConfirm : ICommand
    {
        private readonly IMapper mapper;
        private readonly BotClient botClient;
        private readonly ClientSettings clientSettings;

        public BirthDateConfirm(IMapper mapper, BotClient botClient, ClientSettings clientSetting)
        {
            this.clientSettings = clientSetting;
            this.botClient = botClient;
            this.mapper = mapper;
        }

        public string Key => CommandKeys.BirthDateConfirm;

        public async Task Execute(Update update, TelegramUser user = null, IServiceScope actionScope = null)
        {
            var repository = actionScope.ServiceProvider.GetService<IRepository>();
            var actionsManager = actionScope.ServiceProvider.GetService<ActionManager>();
            var resources = actionScope.ServiceProvider.GetService<IStringLocalizer<SharedResources>>();

            TUser dbUser = user as TUser ?? await repository.GetAsync<TUser>(false, u => u.Id == update.CallbackQuery.From.Id);
            dbUser.CurrentStatus = actionsManager.FindInputStatusByType<GeopositionInput>();
            await repository.UpdateAsync(dbUser);

            await botClient.AnswerCallbackQueryAsync(update.CallbackQuery.Id);
            try
            {
                await botClient.DeleteMessageAsync(update.CallbackQuery.Message.Chat.Id, update.CallbackQuery.Message.MessageId);
            }
            catch
            { }
            KeyboardButton locationButton = new KeyboardButton(resources["SHARE_LOCATION_BUTTON"]) { RequestLocation = true };

            // Output
            if (dbUser.Limitations.StartLocationInputAttempts == 0)
            {
                if ((DateTime.Now - dbUser.Limitations.StartLocationInputBlockDate).Value.TotalDays > clientSettings.StartLocationInputBlockDays)
                {
                    dbUser.Limitations.StartLocationInputBlockDate = null;
                    dbUser.Limitations.StartLocationInputAttempts = clientSettings.StartLocationInputAttempts;
                    await repository.UpdateAsync(dbUser);
                }
            }
            await botClient.SendTextMessageAsync(update.CallbackQuery.Message.Chat.Id, resources["START_LOCATION_INPUT", dbUser.Limitations.StartLocationInputAttempts], parseMode: Telegram.Bot.Types.Enums.ParseMode.Markdown, replyMarkup: new ReplyKeyboardMarkup(locationButton) { ResizeKeyboard = true });
        }
    }
}