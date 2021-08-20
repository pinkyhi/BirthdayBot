using BirthdayBot.BLL.Resources;
using BirthdayBot.Core.Resources;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using RapidBots.Types.Core;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using RapidBots.Types.Attributes;
using Telegram.Bot.Types.Enums;

namespace BirthdayBot.BLL.Commands.UserSettings.Notifications
{
    [ChatType(ChatType.Private)]
    public class NotificationsSettingsInfo : Command
    {
        private readonly BotClient botClient;

        public NotificationsSettingsInfo(BotClient botClient)
        {
            this.botClient = botClient;
        }

        public override string Key => CommandKeys.NotificationSettingsInfo;

        public override async Task Execute(Update update, TelegramUser user = null, IServiceScope actionScope = null)
        {
            var resources = actionScope.ServiceProvider.GetService<IStringLocalizer<SharedResources>>();

            await botClient.AnswerCallbackQueryAsync(update.CallbackQuery.Id, text: resources["NOTIFICATIONS_SETTINGS_INFO"], showAlert: true);
        }
    }
}
