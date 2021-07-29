using BirthdayBot.BLL.Commands.UserSettings.Notifications;
using BirthdayBot.BLL.Menus;
using BirthdayBot.BLL.Menus.Settings;
using BirthdayBot.Core.Resources;
using BirthdayBot.DAL.Entities;
using BirthdayBot.DAL.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using RapidBots.Types.Core;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace BirthdayBot.BLL.Inputs.UserSettings
{
    public class NotificationsSettingsChangeInput : IInput
    {
        private readonly BotClient botClient;

        public NotificationsSettingsChangeInput(BotClient botClient)
        {
            this.botClient = botClient;
        }

        public int Status => 4;

        public async Task Execute(Update update, TelegramUser user = null, IServiceScope actionScope = null)
        {
            //Initialisation
            var repository = actionScope.ServiceProvider.GetService<IRepository>();
            var actionsManager = actionScope.ServiceProvider.GetService<ActionManager>();
            var resources = actionScope.ServiceProvider.GetService<IStringLocalizer<SharedResources>>();
            TUser dbUser = user as TUser ?? await repository.GetAsync<TUser>(true, u => u.Id == update.Message.From.Id);

            bool isCommon = false;
            // Logic
            try
            {
                int delayKey = Convert.ToInt32(dbUser.MiddlewareData);
                int delay = Convert.ToInt32(update.Message.Text.Trim());
                if (delay < 0 || delay > 28)
                {
                    throw new ArgumentException();
                }

                // Change status
                switch (delayKey)
                {
                    case (int)NotificationsDelaysKeys.Common0:
                        dbUser.Settings.CommonNotification_0 = delay;
                        isCommon = true;
                        break;
                    case (int)NotificationsDelaysKeys.Strong0:
                        dbUser.Settings.StrongNotification_0 = delay;
                        break;
                    case (int)NotificationsDelaysKeys.Strong1:
                        dbUser.Settings.StrongNotification_1 = delay;
                        break;
                    case (int)NotificationsDelaysKeys.Strong2:
                        dbUser.Settings.StrongNotification_2 = delay;
                        break;
                    default:
                        throw new ArgumentException();
                }
                dbUser.CurrentStatus = null;
                dbUser.MiddlewareData = null;
                await repository.UpdateAsync(dbUser);
            }
            catch
            {
                await botClient.SendTextMessageAsync(update.Message.Chat.Id, resources["NOTIFICATIONS_SETTINGS_CHANGE_INPUT_ERROR"], parseMode: Telegram.Bot.Types.Enums.ParseMode.Markdown);
                return;
            }

            // Output
            if (isCommon)
            {
                var menuDictionary = new Dictionary<int, int>();
                menuDictionary.Add(((int)NotificationsDelaysKeys.Common0), dbUser.Settings.CommonNotification_0);

                var menu = new NotificationsSettingsChangeMenu(resources, menuDictionary);
                await botClient.SendTextMessageAsync(update.Message.Chat.Id, menu.GetDefaultTitle(actionScope), replyMarkup: menu.GetMarkup(actionScope));
            }
            else
            {
                var menuDictionary = new Dictionary<int, int>();
                menuDictionary.Add(((int)NotificationsDelaysKeys.Strong0), dbUser.Settings.StrongNotification_0);
                menuDictionary.Add(((int)NotificationsDelaysKeys.Strong1), dbUser.Settings.StrongNotification_1);
                menuDictionary.Add(((int)NotificationsDelaysKeys.Strong2), dbUser.Settings.StrongNotification_2);

                var menu = new NotificationsSettingsChangeMenu(resources, menuDictionary);
                await botClient.SendTextMessageAsync(update.Message.Chat.Id, menu.GetDefaultTitle(actionScope), replyMarkup: menu.GetMarkup(actionScope));
            }
        }
    }
}
