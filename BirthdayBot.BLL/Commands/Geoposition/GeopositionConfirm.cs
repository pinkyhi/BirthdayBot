using AutoMapper;
using BirthdayBot.BLL.Menus;
using BirthdayBot.BLL.Resources;
using BirthdayBot.Core.Resources;
using BirthdayBot.DAL.Entities;
using BirthdayBot.DAL.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using Newtonsoft.Json;
using RapidBots.GoogleGeoCode.Types;
using RapidBots.Types.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace BirthdayBot.BLL.Commands.Geoposition
{
    public class GeopositionConfirm : ICommand
    {
        private readonly IMapper mapper;
        private readonly BotClient botClient;

        public GeopositionConfirm(IMapper mapper, BotClient botClient)
        {
            this.botClient = botClient;
            this.mapper = mapper;
        }

        public string Key => CommandKeys.GeopositionConfirm;

        public async Task Execute(Update update, TelegramUser user = null, IServiceScope actionScope = null)
        {
            var repository = actionScope.ServiceProvider.GetService<IRepository>();
            var actionsManager = actionScope.ServiceProvider.GetService<ActionManager>();
            var resources = actionScope.ServiceProvider.GetService<IStringLocalizer<SharedResources>>();

            TUser dbUser = user as TUser ?? await repository.GetAsync<TUser>(false, u => u.Id == update.CallbackQuery.From.Id);
            GoogleGeoCodeResponse geocodeResponse = JsonConvert.DeserializeObject<GoogleGeoCodeResponse>(dbUser.MiddlewareData);
            dbUser.Addresses = mapper.Map<IEnumerable<RapidBots.GoogleGeoCode.Types.Address>, IEnumerable<DAL.Entities.Address>>(geocodeResponse.Results).ToList();
            dbUser.MiddlewareData = null;
            dbUser.RegistrationDate = DateTime.Now;

            await repository.UpdateAsync(dbUser);

            StartMenu menu = new StartMenu(resources);
            await botClient.SendTextMessageAsync(update.CallbackQuery.Message.Chat.Id, menu.GetDefaultTitle(actionScope, dbUser.Username), replyMarkup: menu.GetMarkup(actionScope));  
        }
    }
}
