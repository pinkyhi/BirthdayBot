using AutoMapper;
using BirthdayBot.BLL.Inputs.Start;
using BirthdayBot.BLL.Menus.Settings;
using BirthdayBot.BLL.Resources;
using BirthdayBot.Core.Resources;
using BirthdayBot.Core.Types;
using BirthdayBot.DAL.Entities;
using BirthdayBot.DAL.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using Newtonsoft.Json;
using RapidBots.Types.Attributes;
using RapidBots.Types.Core;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace BirthdayBot.BLL.Commands.BirthDate
{
    [ChatType(ChatType.Private)]
    public class BirthDateConfirm : Command
    {
        private readonly IMapper mapper;
        private readonly BotClient botClient;
        private readonly ClientSettings clientSettings;

        public BirthDateConfirm(IMapper mapper, BotClient botClient, ClientSettings clientSetting)
        {
            this.clientSettings = clientSetting;
            this.botClient = botClient;
            this.mapper = mapper;
        }

        public override string Key => CommandKeys.BirthDateConfirm;

        public override async Task Execute(Update update, TelegramUser user = null, IServiceScope actionScope = null)
        {
            var repository = actionScope.ServiceProvider.GetService<IRepository>();
            var actionsManager = actionScope.ServiceProvider.GetService<ActionManager>();
            var resources = actionScope.ServiceProvider.GetService<IStringLocalizer<SharedResources>>();

            TUser dbUser = user as TUser ?? await repository.GetAsync<TUser>(false, u => u.Id == update.CallbackQuery.From.Id);
            try
            {
                var data = JsonConvert.DeserializeObject<Dictionary<string, string>>(dbUser.MiddlewareData);
                var date = DateTime.Parse(data["date"]);
                dbUser.BirthDate = date;
                await repository.UpdateAsync(dbUser);
            }
            catch
            {
                dbUser.BirthDate = Convert.ToDateTime(dbUser.MiddlewareData);
                await repository.UpdateAsync(dbUser);
                dbUser.MiddlewareData = null;
            }
            dbUser.CurrentStatus = actionsManager.FindInputStatusByType<GeopositionInput>();
            await repository.UpdateAsync(dbUser);

            try{await botClient.AnswerCallbackQueryAsync(update.CallbackQuery.Id);}catch{}
            try
            {
                await botClient.DeleteMessageAsync(update.CallbackQuery.Message.Chat.Id, update.CallbackQuery.Message.MessageId);
            }
            catch
            { }
            if(dbUser.RegistrationDate == null)
            {
                KeyboardButton locationButton = new KeyboardButton(resources["SHARE_LOCATION_BUTTON"]) { RequestLocation = true };

                // Output
                if (dbUser.Limitations.StartLocationInputAttempts == 0)
                {
                    if ((DateTime.Now - dbUser.Limitations.StartLocationInputBlockDate).Value.TotalDays > clientSettings.StartLocationInputBlockDays)
                    {
                        dbUser.Limitations.StartLocationInputBlockDate = null;
                        dbUser.Limitations.StartLocationInputAttempts = clientSettings.StartLocationInputAttempts;
                        await repository.UpdateAsync(dbUser);
                    }
                }
                await botClient.SendTextMessageAsync(update.CallbackQuery.Message.Chat.Id, resources["START_LOCATION_INPUT", dbUser.Limitations.StartLocationInputAttempts], parseMode: Telegram.Bot.Types.Enums.ParseMode.Markdown, replyMarkup: new ReplyKeyboardMarkup(locationButton) { ResizeKeyboard = true });
            }
            else
            {
                if (dbUser?.Addresses == null)
                {
                    dbUser = await repository.GetAsync<TUser>(false, u => u.Id == update.CallbackQuery.From.Id, include: u => u.Include(x => x.Addresses));
                }

                ProfileSettingsMenu menu = new ProfileSettingsMenu(resources);

                await botClient.SendTextMessageAsync(update.CallbackQuery.Message.Chat.Id, menu.GetDefaultTitle(actionScope, dbUser.BirthDate.ToShortDateString(), dbUser.Addresses[0].Formatted_Address), replyMarkup: menu.GetMarkup(actionScope));
            }
        }
    }
}
