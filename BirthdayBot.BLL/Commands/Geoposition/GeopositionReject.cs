using BirthdayBot.BLL.Inputs.Start;
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
using Telegram.Bot.Types.ReplyMarkups;
using RapidBots.Types.Attributes;
using Telegram.Bot.Types.Enums;

namespace BirthdayBot.BLL.Commands.Geoposition
{
    [ChatType(ChatType.Private)]
    public class GeopositionReject : Command
    {
        private readonly BotClient botClient;

        public GeopositionReject(BotClient botClient)
        {
            this.botClient = botClient;
        }

        public override string Key => CommandKeys.GeopositionReject;

        public override async Task Execute(Update update, TelegramUser user = null, IServiceScope actionScope = null)
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
            try{await botClient.AnswerCallbackQueryAsync(update.CallbackQuery.Id);}catch{}
            try
            {
                await botClient.DeleteMessageAsync(update.CallbackQuery.Message.Chat.Id, update.CallbackQuery.Message.MessageId);
            }
            catch
            {}
            if(dbUser.RegistrationDate == null)
            {
                await botClient.SendTextMessageAsync(update.CallbackQuery.Message.Chat.Id, resources["START_LOCATION_INPUT", dbUser.Limitations.StartLocationInputAttempts], parseMode: Telegram.Bot.Types.Enums.ParseMode.Html, replyMarkup: new ReplyKeyboardMarkup(locationButton) { ResizeKeyboard = true });
            }
            else
            {
                KeyboardButton backBut = new KeyboardButton() { Text = resources["BACK_BUTTON"] };

                List<List<KeyboardButton>> keyboard = new List<List<KeyboardButton>>()
                {
                    new List<KeyboardButton>()
                    {
                        locationButton
                    },
                    new List<KeyboardButton>()
                    {
                        backBut
                    }
                };
                await botClient.SendTextMessageAsync(update.CallbackQuery.Message.Chat.Id, resources["CHANGE_LOCATION_INPUT", dbUser.Limitations.ChangeLocationInputAttempts], parseMode: Telegram.Bot.Types.Enums.ParseMode.Html, replyMarkup: new ReplyKeyboardMarkup(keyboard) { ResizeKeyboard = true });
            }
        }
    }
}
