using BirthdayBot.BLL.Resources;
using BirthdayBot.Core.Resources;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using RapidBots.Types.Core;
using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace BirthdayBot.BLL.Commands.UserSettings.Notifications
{
    public class NotificationsSettingsInfo : ICommand
    {
        private readonly BotClient botClient;

        public NotificationsSettingsInfo(BotClient botClient)
        {
            this.botClient = botClient;
        }

        public string Key => CommandKeys.NotificationSettingsInfo;

        public async Task Execute(Update update, TelegramUser user = null, IServiceScope actionScope = null)
        {
            var resources = actionScope.ServiceProvider.GetService<IStringLocalizer<SharedResources>>();

            await botClient.AnswerCallbackQueryAsync(update.CallbackQuery.Id, text: resources["NOTIFICATIONS_SETTINGS_INFO"], showAlert: true);
        }
    }
}
