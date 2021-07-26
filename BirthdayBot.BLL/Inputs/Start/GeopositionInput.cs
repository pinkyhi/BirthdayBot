using BirthdayBot.BLL.Menus;
using BirthdayBot.Core.Resources;
using BirthdayBot.DAL.Entities;
using BirthdayBot.DAL.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using Newtonsoft.Json;
using RapidBots.GoogleGeoCode;
using RapidBots.GoogleGeoCode.Types;
using RapidBots.Types.Core;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace BirthdayBot.BLL.Inputs.Start
{
    class GeopositionInput : IInput
    {
        private readonly BotClient botClient;
        private readonly GoogleGeoCodeOptions geocodeOptions;
        public GeopositionInput(BotClient botClient, GoogleGeoCodeOptions geocodeOptions)
        {
            this.botClient = botClient;
            this.geocodeOptions = geocodeOptions;
        }

        public int Status => 3;

        public async Task Execute(Update update, TelegramUser user = null, IServiceScope actionScope = null)
        {
            //Initialisation
            var repository = actionScope.ServiceProvider.GetService<IRepository>();
            var actionsManager = actionScope.ServiceProvider.GetService<ActionManager>();
            var resources = actionScope.ServiceProvider.GetService<IStringLocalizer<SharedResources>>();
            TUser dbUser = user as TUser ?? await repository.GetAsync<TUser>(true, u => u.Id == update.Message.From.Id);

            if(dbUser.UserLimitations.StartLocationInputAttempts == 0)
            {
                if((DateTime.Now - dbUser.UserLimitations.StartLocationInputBlockDate).Value.TotalDays < 0) // TODO: CHANGE 0 TO VALUE FROM DYNAMIC SETTINGS
                {
                    await botClient.SendTextMessageAsync(update.Message.Chat.Id, string.Format(resources["GEOPOSITION_INPUT_ERROR"], dbUser.UserLimitations.StartLocationInputAttempts), parseMode: Telegram.Bot.Types.Enums.ParseMode.Markdown);
                    return;
                }
                else
                {
                    dbUser.UserLimitations.StartLocationInputAttempts = 5; // TODO: CHANGE 5 TO VALUE FROM DYNAMIC SETTINGS
                    await repository.UpdateAsync(dbUser);
                }
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
                    request.RequestUri = GoogleGeoCodeHepler.BuildReverseGeocodeUri(string.Format("{0}, {1}", lat, lng), geocodeOptions);
                }
                else
                {
                    string addressStr = update.Message.Text.Trim();
                    if (string.IsNullOrEmpty(addressStr))
                    {
                        throw new ArgumentException();
                    }

                    request.RequestUri = GoogleGeoCodeHepler.BuildGeocodeUri(update.Message.Text, geocodeOptions);
                }

                HttpResponseMessage response = await client.SendAsync(request);

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
                dbUser.UserLimitations.StartLocationInputAttempts--;
                if (dbUser.UserLimitations.StartLocationInputAttempts == 0)
                {
                    dbUser.UserLimitations.StartLocationInputBlockDate = DateTime.Now;
                }
                dbUser.CurrentStatus = null;
                await repository.UpdateAsync(dbUser);
            }
            catch
            {
                await botClient.SendTextMessageAsync(update.Message.Chat.Id, string.Format(resources["GEOPOSITION_INPUT_ERROR"], dbUser.UserLimitations.StartLocationInputAttempts), parseMode: Telegram.Bot.Types.Enums.ParseMode.Markdown);
                return;
            }

            GeopositionConfirmationMenu menu = new GeopositionConfirmationMenu(resources);

            // Output
            await botClient.SendTextMessageAsync(update.Message.Chat.Id, geocodeResponse.GetFirstAddress(), replyMarkup: new ReplyKeyboardRemove() { Selective=false });
            await botClient.SendTextMessageAsync(update.Message.Chat.Id, menu.GetDefaultTitle(), parseMode: Telegram.Bot.Types.Enums.ParseMode.Markdown, replyMarkup: menu.GetMarkup());
        }
    }
}
