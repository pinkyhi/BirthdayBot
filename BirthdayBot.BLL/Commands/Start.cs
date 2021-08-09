using AutoMapper;
using BirthdayBot.BLL.Inputs.Start;
using BirthdayBot.BLL.Menus;
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

namespace BirthdayBot.BLL.Commands
{
    class Start : ICommand
    {
        private readonly IMapper mapper;
        private readonly BotClient botClient;

        public Start(IMapper mapper, BotClient botClient)
        {
            this.botClient = botClient;
            this.mapper = mapper;
        }

        public string Key => CommandKeys.Start;

        public async Task Execute(Update update, TelegramUser user = null, IServiceScope actionScope = null)
        {
            var repository = actionScope.ServiceProvider.GetService<IRepository>();
            var actionsManager = actionScope.ServiceProvider.GetService<ActionManager>();
            var resources = actionScope.ServiceProvider.GetService<IStringLocalizer<SharedResources>>();

            TUser dbUser = user as TUser ?? await repository.GetAsync<TUser>(false, u => u.Id == (update.Message?.From.Id ?? update.CallbackQuery.From.Id));

            if (dbUser == null)
            {
                TUser newUser = mapper.Map<TUser>(update.Message?.From ?? update.CallbackQuery?.From);
                dbUser = await repository.AddAsync(newUser);
            }

            // Zeroing
            dbUser.CurrentStatus = null;
            dbUser.MiddlewareData = null;
            await repository.UpdateAsync(dbUser);

            if(update.Type == Telegram.Bot.Types.Enums.UpdateType.CallbackQuery)
            {
                await botClient.AnswerCallbackQueryAsync(update.CallbackQuery.Id);
                try
                {
                    await botClient.DeleteMessageAsync(update.CallbackQuery.Message.Chat.Id, update.CallbackQuery.Message.MessageId);
                }
                catch
                { }
            }

            var openerMessage = await botClient.SendTextMessageAsync(update.Message?.Chat?.Id ?? update.CallbackQuery.Message.Chat.Id, resources["MENU_OPENER_TEXT"], replyMarkup: new ReplyKeyboardRemove());
            await botClient.DeleteMessageAsync(openerMessage.Chat.Id, openerMessage.MessageId);

            if (dbUser.RegistrationDate == null)
            {
                dbUser.CurrentStatus = actionsManager.FindInputStatusByType<BirthYearInput>();
                await repository.UpdateAsync(dbUser);
                await botClient.SendTextMessageAsync(update.Message?.Chat?.Id ?? update.CallbackQuery.Message.Chat.Id, resources["BIRTH_YEAR_INPUT"], replyMarkup: new ReplyKeyboardRemove() { Selective = false });
            }
            else
            {
                StartMenu menu = new StartMenu(resources);
                await botClient.SendTextMessageAsync(update.Message?.Chat?.Id ?? update.CallbackQuery.Message.Chat.Id, menu.GetDefaultTitle(actionScope, dbUser.Username), replyMarkup: menu.GetMarkup(actionScope));
            }
        }
    }
}
