﻿using BirthdayBot.BLL.Menus.Settings;
using BirthdayBot.BLL.Resources;
using BirthdayBot.Core.Resources;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using RapidBots.Types.Core;
using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace BirthdayBot.BLL.Commands.UserSettings
{
    public class UserSettings : ICommand
    {
        private readonly BotClient botClient;

        public UserSettings(BotClient botClient)
        {
            this.botClient = botClient;
        }

        public string Key => CommandKeys.UserSettings;

        public async Task Execute(Update update, TelegramUser user = null, IServiceScope actionScope = null)
        {
            var resources = actionScope.ServiceProvider.GetService<IStringLocalizer<SharedResources>>();

            UserSettingsMenu menu = new UserSettingsMenu(resources);

            try{await botClient.AnswerCallbackQueryAsync(update.CallbackQuery.Id);}catch{}
            try
            {
                await botClient.DeleteMessageAsync(update.CallbackQuery.Message.Chat.Id, update.CallbackQuery.Message.MessageId);
            }
            catch
            { }
            await botClient.SendTextMessageAsync(update.CallbackQuery.Message.Chat.Id, menu.GetDefaultTitle(actionScope), replyMarkup: menu.GetMarkup(actionScope));
        }
    }
}
