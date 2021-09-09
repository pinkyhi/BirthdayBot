using BirthdayBot.BLL.Menus.Settings;
using BirthdayBot.BLL.Resources;
using BirthdayBot.Core.Resources;
using BirthdayBot.DAL.Entities;
using BirthdayBot.DAL.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using RapidBots.Types.Attributes;
using Telegram.Bot.Types.Enums;
using RapidBots.Types.Core;
using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace BirthdayBot.BLL.Commands.UserSettings
{
    [ChatType(ChatType.Private)]
    public class ProfileSettings : Command
    {
        private readonly BotClient botClient;

        public ProfileSettings(BotClient botClient)
        {
            this.botClient = botClient;
        }

        public override string Key => CommandKeys.ProfileSettings;

        public override async Task Execute(Update update, TelegramUser user = null, IServiceScope actionScope = null)
        {
            var resources = actionScope.ServiceProvider.GetService<IStringLocalizer<SharedResources>>();
            var repository = actionScope.ServiceProvider.GetService<IRepository>();

            TUser dbUser = user as TUser;
            if (dbUser.Addresses == null)
            {
                await repository.LoadCollectionAsync(dbUser, x => x.Addresses);
            }
            dbUser.CurrentStatus = null;
            dbUser.MiddlewareData = null;
            await repository.UpdateAsync(dbUser);

            ProfileSettingsMenu menu = new ProfileSettingsMenu(resources);
            try{await botClient.AnswerCallbackQueryAsync(update.CallbackQuery.Id);}catch{}
            try
            {
                await botClient.DeleteMessageAsync(update.CallbackQuery.Message.Chat.Id, update.CallbackQuery.Message.MessageId);
            }
            catch
            { }
            await botClient.SendTextMessageAsync(update.CallbackQuery.Message.Chat.Id, menu.GetDefaultTitle(actionScope, dbUser.BirthDate.ToShortDateString(), dbUser.Addresses[0].Formatted_Address, dbUser.Timezone.TimeZoneName), replyMarkup: menu.GetMarkup(actionScope), parseMode: Telegram.Bot.Types.Enums.ParseMode.Html);
        }
    }
}
