using BirthdayBot.BLL.Inputs.Start;
using BirthdayBot.BLL.Resources;
using BirthdayBot.Core.Resources;
using BirthdayBot.Core.Types;
using BirthdayBot.DAL.Entities;
using BirthdayBot.DAL.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using RapidBots.Types.Core;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using RapidBots.Types.Attributes;
using Telegram.Bot.Types.Enums;
namespace BirthdayBot.BLL.Commands.UserSettings.Profile
{
    [ChatType(ChatType.Private)]
    public class ChangeLocation : Command
    {
        private readonly BotClient botClient;
        private readonly ClientSettings clientSettings;

        public ChangeLocation(BotClient botClient, ClientSettings clientSetting)
        {
            this.clientSettings = clientSetting;
            this.botClient = botClient;
        }

        public override string Key => CommandKeys.ChangeLocation;

        public override async Task Execute(Update update, TelegramUser user = null, IServiceScope actionScope = null)
        {
            var repository = actionScope.ServiceProvider.GetService<IRepository>();
            var actionsManager = actionScope.ServiceProvider.GetService<ActionManager>();
            var resources = actionScope.ServiceProvider.GetService<IStringLocalizer<SharedResources>>();

            TUser dbUser = user as TUser ?? await repository.GetAsync<TUser>(false, u => u.Id == update.CallbackQuery.From.Id);
            dbUser.CurrentStatus = actionsManager.FindInputStatusByType<GeopositionInput>();
            await repository.UpdateAsync(dbUser);

            try{await botClient.AnswerCallbackQueryAsync(update.CallbackQuery.Id);}catch{}
            try
            {
                await botClient.DeleteMessageAsync(update.CallbackQuery.Message.Chat.Id, update.CallbackQuery.Message.MessageId);
            }
            catch
            { }
            KeyboardButton locationButton = new KeyboardButton(resources["SHARE_LOCATION_BUTTON"]) { RequestLocation = true };

            // Output

            if (dbUser.Limitations.ChangeLocationInputAttempts == 0)
            {
                if ((DateTime.Now - dbUser.Limitations.ChangeLocationInputBlockDate).Value.TotalDays > clientSettings.ChangeLocationInputBlockDays)
                {
                    dbUser.Limitations.ChangeLocationInputBlockDate = null;
                    dbUser.Limitations.ChangeLocationInputAttempts = clientSettings.ChangeLocationInputAttempts;
                    await repository.UpdateAsync(dbUser);
                }
            }

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
