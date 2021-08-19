using BirthdayBot.BLL.Inputs.UserSettings;
using BirthdayBot.BLL.Resources;
using BirthdayBot.Core.Resources;
using BirthdayBot.DAL.Entities;
using BirthdayBot.DAL.Interfaces;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using RapidBots.Types.Core;
using System;
using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace BirthdayBot.BLL.Commands.UserSettings.Notifications
{
    public class NotificationSettingsChange : ICommand
    {
        private readonly BotClient botClient;

        public NotificationSettingsChange(BotClient botClient)
        {
            this.botClient = botClient;
        }

        public string Key => CommandKeys.NotificationsSettingsChange;

        public async Task Execute(Update update, TelegramUser user = null, IServiceScope actionScope = null)
        {
            var repository = actionScope.ServiceProvider.GetService<IRepository>();
            var actionsManager = actionScope.ServiceProvider.GetService<ActionManager>();
            var resources = actionScope.ServiceProvider.GetService<IStringLocalizer<SharedResources>>();

            TUser dbUser = (user as TUser) ?? await repository.GetAsync<TUser>(false, u => u.Id == update.CallbackQuery.From.Id);

            string queryString = update.CallbackQuery.Data.Substring(update.CallbackQuery.Data.IndexOf('?') + 1);   // Common action

            var properties = QueryHelpers.ParseNullableQuery(queryString);
            int notificationDelayKey = Convert.ToInt32(properties["property"][0]);
            dbUser.MiddlewareData = notificationDelayKey.ToString();
            dbUser.CurrentStatus = actionsManager.FindInputStatusByType<NotificationsSettingsChangeInput>();
            await repository.UpdateAsync(dbUser);

            try{await botClient.AnswerCallbackQueryAsync(update.CallbackQuery.Id);}catch{}
            try
            {
                await botClient.DeleteMessageAsync(update.CallbackQuery.Message.Chat.Id, update.CallbackQuery.Message.MessageId);
            }
            catch
            { }
            await botClient.SendTextMessageAsync(update.CallbackQuery.Message.Chat.Id, resources["NOTIFICATIONS_SETTINGS_CHANGE_INPUT"]);
        }
    }
}
