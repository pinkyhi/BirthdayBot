using BirthdayBot.BLL.Menus.Settings;
using BirthdayBot.BLL.Resources;
using BirthdayBot.Core.Resources;
using BirthdayBot.DAL.Entities;
using BirthdayBot.DAL.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using RapidBots.Types.Core;
using System.Collections.Generic;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using RapidBots.Types.Attributes;
using Telegram.Bot.Types.Enums;

namespace BirthdayBot.BLL.Commands.UserSettings.Notifications
{
    [ChatType(ChatType.Private)]
    public class StrongNotificationsSettings : Command
    {
        private readonly BotClient botClient;

        public StrongNotificationsSettings(BotClient botClient)
        {
            this.botClient = botClient;
        }

        public override string Key => CommandKeys.StrongNotificationsSettings;

        public override async Task Execute(Update update, TelegramUser user = null, IServiceScope actionScope = null)
        {
            var repository = actionScope.ServiceProvider.GetService<IRepository>();
            var actionsManager = actionScope.ServiceProvider.GetService<ActionManager>();
            var resources = actionScope.ServiceProvider.GetService<IStringLocalizer<SharedResources>>();

            TUser dbUser = (user as TUser) ?? await repository.GetAsync<TUser>(false, u => u.Id == update.CallbackQuery.From.Id);

            var menuDictionary = new Dictionary<int, int>();
            menuDictionary.Add(((int)NotificationsDelaysKeys.Strong0), dbUser.Settings.StrongNotification_0);
            menuDictionary.Add(((int)NotificationsDelaysKeys.Strong1), dbUser.Settings.StrongNotification_1);
            menuDictionary.Add(((int)NotificationsDelaysKeys.Strong2), dbUser.Settings.StrongNotification_2);


            var menu = new NotificationsSettingsChangeMenu(resources, menuDictionary);
            try{await botClient.AnswerCallbackQueryAsync(update.CallbackQuery.Id);}catch{}
            try
            {
                await botClient.DeleteMessageAsync(update.CallbackQuery.Message.Chat.Id, update.CallbackQuery.Message.MessageId);
            }
            catch
            { }
            await botClient.SendTextMessageAsync(update.CallbackQuery.Message.Chat.Id, menu.GetDefaultTitle(actionScope), replyMarkup: menu.GetMarkup(actionScope), parseMode: Telegram.Bot.Types.Enums.ParseMode.Html);
        }
    }
}
