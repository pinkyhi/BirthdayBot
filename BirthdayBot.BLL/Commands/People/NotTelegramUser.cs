using AutoMapper;
using BirthdayBot.BLL.Commands.Notes;
using BirthdayBot.BLL.Inputs.Start;
using BirthdayBot.BLL.Menus;
using BirthdayBot.BLL.Resources;
using BirthdayBot.Core.Resources;
using BirthdayBot.DAL.Entities;
using BirthdayBot.DAL.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using RapidBots.Types.Attributes;
using RapidBots.Types.Core;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace BirthdayBot.BLL.Commands.People
{
    [ChatType(ChatType.Private)]
    public class NotTelegramUser : Command
    {
        private readonly BotClient botClient;

        public NotTelegramUser(IMapper mapper, BotClient botClient)
        {
            this.botClient = botClient;
        }

        public override string Key => CommandKeys.NotTelegramUser;

        public override async Task Execute(Update update, TelegramUser user = null, IServiceScope actionScope = null)
        {
            var repository = actionScope.ServiceProvider.GetService<IRepository>();
            var actionsManager = actionScope.ServiceProvider.GetService<ActionManager>();
            var resources = actionScope.ServiceProvider.GetService<IStringLocalizer<SharedResources>>();

            await botClient.SendTextMessageAsync(update.CallbackQuery.From.Id, resources["NOT_TELEGRAM_USER_TEXT"], parseMode: ParseMode.Html);
            await actionsManager.FindCommandByType<AddNote>().Execute(update, user, actionScope);
        }
    }
}
