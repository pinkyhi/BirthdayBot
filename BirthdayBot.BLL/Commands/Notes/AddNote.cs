﻿using BirthdayBot.BLL.Inputs.Notes;
using BirthdayBot.BLL.Resources;
using RapidBots.Types.Attributes;
using Telegram.Bot.Types.Enums;
using BirthdayBot.Core.Resources;
using BirthdayBot.DAL.Entities;
using BirthdayBot.DAL.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using Newtonsoft.Json;
using RapidBots.Types.Core;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace BirthdayBot.BLL.Commands.Notes
{
    [ChatType(ChatType.Private)]
    public class AddNote : Command
    {
        private readonly BotClient botClient;

        public AddNote(BotClient botClient)
        {
            this.botClient = botClient;
        }

        public override string Key => CommandKeys.AddNote;

        public override async Task Execute(Update update, TelegramUser user = null, IServiceScope actionScope = null)
        {
            var repository = actionScope.ServiceProvider.GetService<IRepository>();
            var actionsManager = actionScope.ServiceProvider.GetService<ActionManager>();
            var resources = actionScope.ServiceProvider.GetService<IStringLocalizer<SharedResources>>();

            TUser dbUser = user as TUser ?? await repository.GetAsync<TUser>(false, u => u.Id == update.CallbackQuery.From.Id);
            dbUser.CurrentStatus = actionsManager.FindInputStatusByType<NoteTitleInput>();
            var newNote = new Note();
            dbUser.MiddlewareData = JsonConvert.SerializeObject(newNote);
            await repository.UpdateAsync(dbUser);

            try{await botClient.AnswerCallbackQueryAsync(update.CallbackQuery.Id);}catch{}
            try
            {
                await botClient.DeleteMessageAsync(update.CallbackQuery.Message.Chat.Id, update.CallbackQuery.Message.MessageId);
            }
            catch
            { }

            // Output

            KeyboardButton backBut = new KeyboardButton() { Text = resources["BACK_BUTTON"] };

            dbUser.CurrentStatus = actionsManager.FindInputStatusByType<NoteTitleInput>();
            await repository.UpdateAsync(dbUser);
            await botClient.SendTextMessageAsync(update.Message?.Chat?.Id ?? update.CallbackQuery.Message.Chat.Id, resources["NOTE_TITLE_INPUT"], replyMarkup: new ReplyKeyboardMarkup(backBut) { ResizeKeyboard = true }, parseMode: Telegram.Bot.Types.Enums.ParseMode.Html);

        }
    }
}
