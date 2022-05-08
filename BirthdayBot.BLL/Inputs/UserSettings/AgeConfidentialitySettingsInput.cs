using BirthdayBot.BLL.Menus.Settings;
using BirthdayBot.Core.Enums;
using BirthdayBot.Core.Resources;
using BirthdayBot.DAL.Entities;
using BirthdayBot.DAL.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using RapidBots.Types.Core;
using System;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using RapidBots.Types.Attributes;
using Telegram.Bot.Types.Enums;

namespace BirthdayBot.BLL.Inputs.UserSettings
{
    [ChatType(ChatType.Private)]
    public class AgeConfidentialitySettingsInput : Input
    {
        private readonly BotClient botClient;

        public AgeConfidentialitySettingsInput(BotClient botClient)
        {
            this.botClient = botClient;
        }

        public override int Status => 5;

        public override async Task Execute(Update update, TelegramUser user = null, IServiceScope actionScope = null)
        {
            //Initialisation
            var repository = actionScope.ServiceProvider.GetService<IRepository>();
            var actionsManager = actionScope.ServiceProvider.GetService<ActionManager>();
            var resources = actionScope.ServiceProvider.GetService<IStringLocalizer<SharedResources>>();
            TUser dbUser = user as TUser ?? await repository.GetAsync<TUser>(true, u => u.Id == update.Message.From.Id);

            // Logic
            try
            {

                var publicType = resources["PUBLIC_CONFIDENTIALITY_TYPE"];
                var privateType = resources["PRIVATE_CONFIDENTIALITY_TYPE"];
                var mutualType = resources["MUTUAL_CONFIDENTIALITY_TYPE"];


                string confidTypeStr = update.Message.Text.Trim();

                ConfidentialType confidentialType = dbUser.Settings.BirthYearConfidentiality;

                if (confidTypeStr.Contains(publicType))
                {
                    confidentialType = ConfidentialType.Public;
                }
                else if (confidTypeStr.Contains(privateType))
                {
                    confidentialType = ConfidentialType.Private;
                }
                else if (confidTypeStr.Contains(mutualType))
                {
                    confidentialType = ConfidentialType.Mutual;
                }
                else
                {
                    throw new ArgumentException();
                }

                // Change status
                dbUser.Settings.BirthYearConfidentiality = confidentialType;
                dbUser.CurrentStatus = null;
                await repository.UpdateAsync(dbUser);
            }
            catch
            {
                await botClient.SendTextMessageAsync(update.Message.Chat.Id, resources["INPUT_ERROR"], parseMode: Telegram.Bot.Types.Enums.ParseMode.Html);
                return;
            }

            // Output
            await botClient.SendTextMessageAsync(update.Message.Chat.Id, resources["YOU_CHOOSED_TEXT", update.Message.Text.Trim()], parseMode: Telegram.Bot.Types.Enums.ParseMode.Html, replyMarkup: new ReplyKeyboardRemove() { Selective = false });
            
            ConfidentialitySettingsMenu menu = new ConfidentialitySettingsMenu(resources);

            await botClient.SendTextMessageAsync(update.Message.Chat.Id, menu.GetDefaultTitle(actionScope, dbUser.Settings.BirthYearConfidentiality.ToString()), replyMarkup: menu.GetMarkup(actionScope), parseMode: Telegram.Bot.Types.Enums.ParseMode.Html);


        }
    }
}
