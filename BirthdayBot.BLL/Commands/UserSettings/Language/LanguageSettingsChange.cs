using BirthdayBot.BLL.Menus.Settings;
using BirthdayBot.BLL.Resources;
using BirthdayBot.Core.Resources;
using BirthdayBot.DAL.Entities;
using BirthdayBot.DAL.Interfaces;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using RapidBots.Types.Core;
using System.Globalization;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using RapidBots.Types.Attributes;
using Telegram.Bot.Types.Enums;
using RapidBots.Extensions;

namespace BirthdayBot.BLL.Commands.UserSettings.Language
{
    [ChatType(ChatType.Private)]
    [ExpectedParams("lang")]
    public class LanguageSettingsChange : Command
    {
        private readonly BotClient botClient;

        public LanguageSettingsChange(BotClient botClient)
        {
            this.botClient = botClient;
        }

        public override string Key => CommandKeys.LanguageSettingsChange;

        public override async Task Execute(Update update, TelegramUser user = null, IServiceScope actionScope = null)
        {
            var repository = actionScope.ServiceProvider.GetService<IRepository>();
            var actionsManager = actionScope.ServiceProvider.GetService<ActionManager>();
            var resources = actionScope.ServiceProvider.GetService<IStringLocalizer<SharedResources>>();

            TUser dbUser = (user as TUser) ?? await repository.GetAsync<TUser>(false, u => u.Id == update.CallbackQuery.From.Id);

            string lang = update.GetParams()["lang"];
            dbUser.LanguageCode = lang;
            await repository.UpdateAsync(dbUser);

            CultureInfo.CurrentCulture = new CultureInfo(lang);
            CultureInfo.CurrentUICulture = new CultureInfo(lang);

            LanguageSettingsMenu menu = new LanguageSettingsMenu(resources);

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
