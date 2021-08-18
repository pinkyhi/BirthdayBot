using BirthdayBot.BLL.Menus;
using BirthdayBot.BLL.Menus.Settings;
using BirthdayBot.Core.Resources;
using BirthdayBot.Core.Types;
using BirthdayBot.DAL.Entities;
using BirthdayBot.DAL.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using Newtonsoft.Json;
using RapidBots.GoogleGeoCode;
using RapidBots.GoogleGeoCode.Types;
using RapidBots.Types.Core;
using System;
using System.Globalization;
using System.Net.Http;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace BirthdayBot.BLL.Inputs.Start
{
    class GeopositionInput : IInput
    {
        private readonly BotClient botClient;
        private readonly GoogleOptions geocodeOptions;
        private readonly ClientSettings clientSettings;

        public GeopositionInput(BotClient botClient, GoogleOptions geocodeOptions, ClientSettings clientSettings)
        {
            this.botClient = botClient;
            this.geocodeOptions = geocodeOptions;
            this.clientSettings = clientSettings;
        }

        public int Status => 3;

        public async Task Execute(Update update, TelegramUser user = null, IServiceScope actionScope = null)
        {
            //Initialisation
            var repository = actionScope.ServiceProvider.GetService<IRepository>();
            var actionsManager = actionScope.ServiceProvider.GetService<ActionManager>();
            var resources = actionScope.ServiceProvider.GetService<IStringLocalizer<SharedResources>>();

            TUser dbUser = user as TUser;
            if (dbUser?.Addresses == null)
            {
                var tempDbUser = await repository.GetAsync<TUser>(false, u => u.Id == update.Message.From.Id, include: u => u.Include(x => x.Addresses));
                dbUser.Addresses = tempDbUser.Addresses;
            }

            if (dbUser.RegistrationDate != null && update.Message.Text.Trim().Equals(resources["BACK_BUTTON"]))
            {
                dbUser.CurrentStatus = null;
                dbUser.MiddlewareData = null;
                await repository.UpdateAsync(dbUser);
                ProfileSettingsMenu changeMenu = new ProfileSettingsMenu(resources);
                await botClient.SendTextMessageAsync(update.Message.Chat.Id, resources["REPLY_KEYBOARD_REMOVE_TEXT"], replyMarkup: new ReplyKeyboardRemove());
                await botClient.SendTextMessageAsync(update.Message.Chat.Id, changeMenu.GetDefaultTitle(actionScope, dbUser.BirthDate.ToShortDateString(), dbUser.Addresses[0].Formatted_Address), replyMarkup: changeMenu.GetMarkup(actionScope));

                return;
            }
            if (dbUser.RegistrationDate == null && dbUser.Limitations.StartLocationInputAttempts == 0)
            {
                if((DateTime.Now - dbUser.Limitations.StartLocationInputBlockDate).Value.TotalDays < clientSettings.StartLocationInputBlockDays)
                {
                    var blockEnd = dbUser.Limitations.StartLocationInputBlockDate.Value.AddDays(clientSettings.StartLocationInputBlockDays);
                    await botClient.SendTextMessageAsync(update.Message.Chat.Id, resources["INPUT_BLOCK", blockEnd.ToLongDateString()], parseMode: Telegram.Bot.Types.Enums.ParseMode.Markdown, replyMarkup: new ReplyKeyboardRemove() { Selective = false });
                    
                    dbUser.CurrentStatus = null;
                    await repository.UpdateAsync(dbUser);
                    return;
                }
                else
                {
                    dbUser.Limitations.StartLocationInputBlockDate = null;
                    dbUser.Limitations.StartLocationInputAttempts = clientSettings.StartLocationInputAttempts;
                    await repository.UpdateAsync(dbUser);
                }
            }
            else if (dbUser.RegistrationDate != null && dbUser.Limitations.ChangeLocationInputAttempts == 0)
            {
                if ((DateTime.Now - dbUser.Limitations.ChangeLocationInputBlockDate).Value.TotalDays < clientSettings.ChangeLocationInputBlockDays)
                {
                    var blockEnd = dbUser.Limitations.ChangeLocationInputBlockDate.Value.AddDays(clientSettings.ChangeLocationInputBlockDays);
                    await botClient.SendTextMessageAsync(update.Message.Chat.Id, resources["INPUT_BLOCK", blockEnd.ToLongDateString()], parseMode: Telegram.Bot.Types.Enums.ParseMode.Markdown, replyMarkup: new ReplyKeyboardRemove() { Selective = false });

                    dbUser.CurrentStatus = null;
                    await repository.UpdateAsync(dbUser);
                }
                else
                {
                    dbUser.Limitations.ChangeLocationInputBlockDate = null;
                    dbUser.Limitations.ChangeLocationInputAttempts = clientSettings.ChangeLocationInputAttempts;
                    await repository.UpdateAsync(dbUser);
                }

                if (dbUser?.Addresses == null)
                {
                    dbUser = await repository.GetAsync<TUser>(false, u => u.Id == update.Message.From.Id, include: u => u.Include(x => x.Addresses));
                }

                ProfileSettingsMenu changeMenu = new ProfileSettingsMenu(resources);

                await botClient.SendTextMessageAsync(update.Message.Chat.Id, changeMenu.GetDefaultTitle(actionScope, dbUser.BirthDate.ToShortDateString(), dbUser.Addresses[0].Formatted_Address), replyMarkup: changeMenu.GetMarkup(actionScope));
                return;
            }

            GoogleGeoCodeResponse geocodeResponse;
            // Logic
            try
            {
                
                HttpClient client = new HttpClient();
                HttpRequestMessage request = new HttpRequestMessage();
                request.Method = HttpMethod.Get;

                if (update.Message.Location != null)
                {
                    string lat = update.Message.Location.Latitude.ToString(CultureInfo.InvariantCulture);
                    string lng = update.Message.Location.Longitude.ToString(CultureInfo.InvariantCulture);
                    request.RequestUri = GoogleHepler.BuildReverseGeocodeUri(string.Format("{0}, {1}", lat, lng), geocodeOptions);
                }
                else
                {
                    string addressStr = update.Message.Text.Trim();
                    if (string.IsNullOrEmpty(addressStr))
                    {
                        throw new ArgumentException();
                    }

                    request.RequestUri = GoogleHepler.BuildGeocodeUri(update.Message.Text, geocodeOptions);
                }

                HttpResponseMessage response = await client.SendAsync(request);

                // Change status
                if(dbUser.RegistrationDate == null)
                {
                    dbUser.Limitations.StartLocationInputAttempts--;
                    if (dbUser.Limitations.StartLocationInputAttempts == 0)
                    {
                        dbUser.Limitations.StartLocationInputBlockDate = DateTime.Now;
                    }
                }
                else
                {
                    dbUser.Limitations.ChangeLocationInputAttempts--;
                    if (dbUser.Limitations.ChangeLocationInputAttempts == 0)
                    {
                        dbUser.Limitations.ChangeLocationInputBlockDate = DateTime.Now;
                    }
                }

                await repository.UpdateAsync(dbUser);

                if (response.IsSuccessStatusCode)
                {
                    string json = await response.Content.ReadAsStringAsync();
                    geocodeResponse = JsonConvert.DeserializeObject<GoogleGeoCodeResponse>(json);
                    if (geocodeResponse.Status.Equals("ZERO_RESULTS"))
                    {
                        throw new ArgumentException();
                    }
                    dbUser.MiddlewareData = json;
                }
                else
                {
                    throw new ArgumentException();
                }

                // Change status
                dbUser.CurrentStatus = null;
                await repository.UpdateAsync(dbUser);
            }
            catch
            {
                if(dbUser.RegistrationDate == null)
                {
                    await botClient.SendTextMessageAsync(update.Message.Chat.Id, resources["GEOPOSITION_INPUT_ERROR", dbUser.Limitations.StartLocationInputAttempts], parseMode: Telegram.Bot.Types.Enums.ParseMode.Markdown, replyMarkup: new ReplyKeyboardRemove() { Selective = false });
                }
                else
                {
                    await botClient.SendTextMessageAsync(update.Message.Chat.Id, resources["GEOPOSITION_INPUT_ERROR", dbUser.Limitations.ChangeLocationInputAttempts], parseMode: Telegram.Bot.Types.Enums.ParseMode.Markdown, replyMarkup: new ReplyKeyboardRemove() { Selective = false });
                }
                return;
            }

            GeopositionConfirmationMenu menu = new GeopositionConfirmationMenu(resources);

            // Output
            await botClient.SendTextMessageAsync(update.Message.Chat.Id, geocodeResponse.GetFirstAddress(), replyMarkup: new ReplyKeyboardRemove() { Selective=false });
            await botClient.SendTextMessageAsync(update.Message.Chat.Id, menu.GetDefaultTitle(), parseMode: Telegram.Bot.Types.Enums.ParseMode.Markdown, replyMarkup: menu.GetMarkup());
        }
    }
}
