﻿using BirthdayBot.BLL.Inputs.UserSettings;
using BirthdayBot.BLL.Menus.Settings;
using BirthdayBot.BLL.Resources;
using BirthdayBot.Core.Resources;
using BirthdayBot.DAL.Entities;
using BirthdayBot.DAL.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using RapidBots.Types.Core;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using RapidBots.Types.Attributes;
using Telegram.Bot.Types.Enums;

namespace BirthdayBot.BLL.Commands.UserSettings.Confidentiality
{
    [ChatType(ChatType.Private)]
    public class AgeConfidentialityChange : Command
    {
        private readonly BotClient botClient;

        public AgeConfidentialityChange(BotClient botClient)
        {
            this.botClient = botClient;
        }

        public override string Key => CommandKeys.AgeConfidentialityChange;

        public override async Task Execute(Update update, TelegramUser user = null, IServiceScope actionScope = null)
        {
            var repository = actionScope.ServiceProvider.GetService<IRepository>();
            var actionsManager = actionScope.ServiceProvider.GetService<ActionManager>();
            var resources = actionScope.ServiceProvider.GetService<IStringLocalizer<SharedResources>>();

            TUser dbUser = (user as TUser) ?? await repository.GetAsync<TUser>(false, u => u.Id == update.CallbackQuery.From.Id);

            dbUser.CurrentStatus = actionsManager.FindInputStatusByType<AgeConfidentialitySettingsInput>();
            await repository.UpdateAsync(dbUser);

            try{await botClient.AnswerCallbackQueryAsync(update.CallbackQuery.Id);}catch{}
            try
            {
                await botClient.DeleteMessageAsync(update.CallbackQuery.Message.Chat.Id, update.CallbackQuery.Message.MessageId);
            }
            catch
            { }

            ConfidentialityTypeSelectMenu menu = new ConfidentialityTypeSelectMenu(resources, dbUser.Settings.BirthYearConfidentiality);
            await botClient.SendTextMessageAsync(update.CallbackQuery.Message.Chat.Id, menu.GetDefaultTitle(), replyMarkup: menu.GetMarkup(), parseMode: Telegram.Bot.Types.Enums.ParseMode.Html);
        }
    }
}
