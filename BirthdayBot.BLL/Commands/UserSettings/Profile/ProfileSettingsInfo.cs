using BirthdayBot.BLL.Resources;
using BirthdayBot.Core.Resources;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using RapidBots.Types.Core;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using RapidBots.Types.Attributes;
using Telegram.Bot.Types.Enums;

namespace BirthdayBot.BLL.Commands.UserSettings.Profile
{
    [ChatType(ChatType.Private)]
    class ProfileSettingsInfo : Command
    {
        private readonly BotClient botClient;

        public ProfileSettingsInfo(BotClient botClient)
        {
            this.botClient = botClient;
        }

        public override string Key => CommandKeys.ProfileSettingsInfo;

        public override async Task Execute(Update update, TelegramUser user = null, IServiceScope actionScope = null)
        {
            var resources = actionScope.ServiceProvider.GetService<IStringLocalizer<SharedResources>>();

            await botClient.AnswerCallbackQueryAsync(update.CallbackQuery.Id, text: resources["PROFILE_SETTINGS_INFO"], showAlert: true);
        }
    }
}
