using BirthdayBot.BLL.Inputs.Start;
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
using Telegram.Bot.Types.ReplyMarkups;
using BirthdayBot.BLL.Menus.Settings;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace BirthdayBot.BLL.Commands.UserSettings.Profile
{
    [ChatType(ChatType.Private)]
    public class ActualizeUser : Command
    {
        private readonly BotClient botClient;

        public ActualizeUser(BotClient botClient)
        {
            this.botClient = botClient;
        }

        public override string Key => CommandKeys.ActualizeUser;

        public override async Task Execute(Update update, TelegramUser user = null, IServiceScope actionScope = null)
        {
            var repository = actionScope.ServiceProvider.GetService<IRepository>();
            var actionsManager = actionScope.ServiceProvider.GetService<ActionManager>();
            var resources = actionScope.ServiceProvider.GetService<IStringLocalizer<SharedResources>>();

            TUser dbUser = user as TUser ?? await repository.GetAsync<TUser>(false, u => u.Id == update.CallbackQuery.From.Id, x => x.Include(x => x.Addresses));
            if (dbUser.Addresses == null)
            {
                await repository.LoadCollectionAsync(dbUser, x => x.Addresses);
            }
            dbUser.Username = update.CallbackQuery.From.Username;
            dbUser.FirstName = update.CallbackQuery.From.FirstName;
            dbUser.LastName = update.CallbackQuery.From.LastName;
            await repository.UpdateAsync(dbUser);

            try { await botClient.AnswerCallbackQueryAsync(update.CallbackQuery.Id, resources["ACTUALIZE_USER_TEXT"]); } catch { }

            // Output
            ProfileSettingsMenu menu = new ProfileSettingsMenu(resources);
            string fAddress = dbUser.Addresses.FirstOrDefault(x => x.Types.Contains("administrative_area_level_1"))?.Formatted_Address ?? dbUser.Addresses.FirstOrDefault(x => x.Types.Contains("country"))?.Formatted_Address ?? ":)";
            await botClient.EditMessageTextAsync(update.CallbackQuery.Message.Chat.Id, update.CallbackQuery.Message.MessageId, menu.GetDefaultTitle(actionScope, dbUser.BirthDate.ToShortDateString(), fAddress, dbUser.Timezone.TimeZoneName), replyMarkup: menu.GetMarkup(actionScope) as InlineKeyboardMarkup, parseMode: Telegram.Bot.Types.Enums.ParseMode.Html);
        }
    }
}
