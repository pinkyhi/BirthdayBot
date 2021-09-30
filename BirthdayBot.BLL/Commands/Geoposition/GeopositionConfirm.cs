using AutoMapper;
using BirthdayBot.BLL.Menus;
using BirthdayBot.BLL.Menus.Settings;
using BirthdayBot.BLL.Resources;
using BirthdayBot.Core.Resources;
using BirthdayBot.DAL.Entities;
using BirthdayBot.DAL.Entities.GoogleTimeZone;
using BirthdayBot.DAL.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using Newtonsoft.Json;
using RapidBots.GoogleGeoCode;
using RapidBots.GoogleGeoCode.Types;
using RapidBots.Types.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using RapidBots.Types.Attributes;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using Microsoft.AspNetCore.WebUtilities;
using BirthdayBot.BLL.Menus.People;

namespace BirthdayBot.BLL.Commands.Geoposition
{
    [ChatType(ChatType.Private)]
    public class GeopositionConfirm : Command
    {
        private readonly IMapper mapper;
        private readonly BotClient botClient;
        private readonly GoogleOptions geocodeOptions;

        public GeopositionConfirm(IMapper mapper, GoogleOptions geocodeOptions, BotClient botClient)
        {
            this.botClient = botClient;
            this.mapper = mapper;
            this.geocodeOptions = geocodeOptions;
        }

        public override string Key => CommandKeys.GeopositionConfirm;

        public override async Task Execute(Update update, TelegramUser user = null, IServiceScope actionScope = null)
        {
            var repository = actionScope.ServiceProvider.GetService<IRepository>();
            var actionsManager = actionScope.ServiceProvider.GetService<ActionManager>();
            var resources = actionScope.ServiceProvider.GetService<IStringLocalizer<SharedResources>>();

            TUser dbUser = user as TUser ?? await repository.GetAsync<TUser>(true, u => u.Id == update.CallbackQuery.From.Id, x => x.Include(x => x.Addresses));
            if (dbUser?.Addresses == null)
            {
                await repository.LoadCollectionAsync(dbUser, x => x.Addresses);
            }

            string fromChat = null;
            string refId = null;
            GoogleGeoCodeResponse geocodeResponse = null;
            try
            {
                var data = JsonConvert.DeserializeObject<Dictionary<string, string>>(dbUser.MiddlewareData);
                data.TryGetValue("fromChat", out fromChat);
                data.TryGetValue("refId", out refId);

                geocodeResponse = JsonConvert.DeserializeObject<GoogleGeoCodeResponse>(data["address"]);
            }
            catch (Exception)
            {
                geocodeResponse = JsonConvert.DeserializeObject<GoogleGeoCodeResponse>(dbUser.MiddlewareData);
            }
            dbUser.Addresses = mapper.Map<IEnumerable<RapidBots.GoogleGeoCode.Types.Address>, IEnumerable<DAL.Entities.Address>>(geocodeResponse.Results).ToList();
            dbUser.MiddlewareData = null;
            await repository.UpdateAsync(dbUser);
            try
            {
                HttpClient client = new HttpClient();
                HttpRequestMessage request = new HttpRequestMessage();
                request.Method = HttpMethod.Get;
                var place = geocodeResponse.Results.FirstOrDefault(x => x.Geometry != null);
                request.RequestUri = GoogleHepler.BuildTimezoneUri(string.Format("{0}, {1}", place.Geometry.Location.Lat, place.Geometry.Location.Lng), geocodeOptions);
                HttpResponseMessage response = await client.SendAsync(request);
                if (response.IsSuccessStatusCode)
                {
                    string json = await response.Content.ReadAsStringAsync();
                    var timezoneResponse = JsonConvert.DeserializeObject<TimeZoneResponse>(json);

                    dbUser.Timezone = mapper.Map<UserTimezone>(timezoneResponse);
                    await repository.UpdateAsync(dbUser);
                }
                else
                {
                    throw new ArgumentException();
                }
            }
            catch
            {
                throw new Exception("Timezone can't be completed");
            }
            try{await botClient.AnswerCallbackQueryAsync(update.CallbackQuery.Id);}catch{}
            try
            {
                await botClient.DeleteMessageAsync(update.CallbackQuery.Message.Chat.Id, update.CallbackQuery.Message.MessageId);
            }
            catch
            { }

            if (dbUser.RegistrationDate == null)
            {
                StartMenu menu = new StartMenu(resources);
                dbUser.RegistrationDate = DateTime.Now.Date;
                await repository.UpdateAsync(dbUser);
                try
                {
                    if(fromChat != null)
                    {
                        var chat = await repository.GetAsync<DAL.Entities.Chat>(true, x => x.Id == Convert.ToInt64(fromChat), x => x.Include(u => u.ChatMembers).ThenInclude(x => x.User).ThenInclude(x => x.Subscriptions));
                        chat.ChatMembers.Add(new DAL.Entities.ChatMember() { User = dbUser, AddingDate = DateTime.Now.Date });
                        try
                        {
                            await repository.UpdateAsync(chat);
                        }
                        catch
                        {
                            await botClient.SendTextMessageAsync(update.Message?.Chat?.Id ?? update.CallbackQuery.Message.Chat.Id, menu.GetDefaultTitle(actionScope, dbUser.Username ?? dbUser.FirstName), replyMarkup: menu.GetMarkup(actionScope), parseMode: ParseMode.Html);
                            return;
                        }
                        await botClient.SendTextMessageAsync(update.Message?.Chat?.Id ?? update.CallbackQuery.Message.Chat.Id, resources["SUCCESS_START_FROM_CHAT", chat.Title], parseMode: Telegram.Bot.Types.Enums.ParseMode.Html);
                        var usersToMention = chat.ChatMembers.Where(x => x.IsSubscribedOnCalendar == true && x.UserId != dbUser.Id && x.User.Subscriptions?.Any(x => x.TargetId == dbUser.Id) == false);
                        var umMenu = new ChatCalendarNotificationMenu(resources, dbUser.Id, chat.Id);
                        foreach (var utm in usersToMention)
                        {
                            await botClient.SendTextMessageAsync(utm.UserId, umMenu.GetDefaultTitle(actionScope, dbUser.Username == null ? $"{dbUser.FirstName} {dbUser.LastName}" : $"@{dbUser.Username}", chat.Title), replyMarkup: umMenu.GetMarkup(actionScope));
                        }

                        var chatMemberCount = await botClient.GetChatMembersCountAsync(chat.Id) - 1;
                        if (chatMemberCount == chat.ChatMembers.Count)
                        {
                            await botClient.SendTextMessageAsync(chat.Id, resources["ALL_USERS_ADDED_TEXT", chat.Title], parseMode: ParseMode.Html);
                        }
                    }
                    if(refId != null)
                    {
                        var subsRefBut = new InlineKeyboardButton() { Text = resources["SUBSCRIBE_BUTTON"], CallbackData = QueryHelpers.AddQueryString(CommandKeys.SubscribeByReferralReply, "userId", dbUser.Id.ToString()) };
                        await botClient.SendTextMessageAsync(Convert.ToInt64(refId), resources["REFERRAL_NOTIFICATION", update.CallbackQuery.From.Username == null ? $"{update.CallbackQuery.From.FirstName} {update.CallbackQuery.From.LastName}" : $"@{update.CallbackQuery.From.Username}"], ParseMode.Html, replyMarkup: new InlineKeyboardMarkup(subsRefBut));
                    }
                }
                catch
                { }

                await botClient.SendTextMessageAsync(update.CallbackQuery.Message.Chat.Id, menu.GetDefaultTitle(actionScope, dbUser.Username ?? dbUser.FirstName), replyMarkup: menu.GetMarkup(actionScope), parseMode: Telegram.Bot.Types.Enums.ParseMode.Html);
            }
            else
            {
                ProfileSettingsMenu menu = new ProfileSettingsMenu(resources);
                string fAddress = dbUser.Addresses.FirstOrDefault(x => x.Types.Contains("administrative_area_level_1"))?.Formatted_Address ?? dbUser.Addresses.FirstOrDefault(x => x.Types.Contains("country"))?.Formatted_Address ?? ":)";
                await botClient.SendTextMessageAsync(update.CallbackQuery.Message.Chat.Id, menu.GetDefaultTitle(actionScope, dbUser.BirthDate.ToShortDateString(), fAddress, dbUser.Timezone.TimeZoneName), replyMarkup: menu.GetMarkup(actionScope), parseMode: Telegram.Bot.Types.Enums.ParseMode.Html);
            }

        }
    }
}
