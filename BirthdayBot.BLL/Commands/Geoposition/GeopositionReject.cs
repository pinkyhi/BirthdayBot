using BirthdayBot.BLL.Inputs.Start;
using BirthdayBot.BLL.Resources;
using BirthdayBot.Core.Resources;
using BirthdayBot.DAL.Entities;
using BirthdayBot.DAL.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using RapidBots.Types.Core;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace BirthdayBot.BLL.Commands.Geoposition
{
    public class GeopositionReject : ICommand
    {
        private readonly BotClient botClient;

        public GeopositionReject(BotClient botClient)
        {
            this.botClient = botClient;
        }

        public string Key => CommandKeys.GeopositionReject;

        public async Task Execute(Update update, TelegramUser user = null, IServiceScope actionScope = null)
        {
            var repository = actionScope.ServiceProvider.GetService<IRepository>();
            var actionsManager = actionScope.ServiceProvider.GetService<ActionManager>();
            var resources = actionScope.ServiceProvider.GetService<IStringLocalizer<SharedResources>>();

            TUser dbUser = user as TUser ?? await repository.GetAsync<TUser>(false, u => u.Id == update.CallbackQuery.From.Id);
            dbUser.MiddlewareData = null;
            dbUser.CurrentStatus = actionsManager.FindInputStatusByType<GeopositionInput>();
            await repository.UpdateAsync(dbUser);

            KeyboardButton locationButton = new KeyboardButton(resources["SHARE_LOCATION_BUTTON"]) { RequestLocation = true };

            // Output
            await botClient.AnswerCallbackQueryAsync(update.CallbackQuery.Id);
            try
            {
                await botClient.DeleteMessageAsync(update.CallbackQuery.Message.Chat.Id, update.CallbackQuery.Message.MessageId);
            }
            catch
            {}
            await botClient.SendTextMessageAsync(update.CallbackQuery.Message.Chat.Id, resources["START_LOCATION_INPUT", dbUser.Limitations.StartLocationInputAttempts], parseMode: Telegram.Bot.Types.Enums.ParseMode.Markdown, replyMarkup: new ReplyKeyboardMarkup(locationButton) { ResizeKeyboard = true });
        }
    }
}
