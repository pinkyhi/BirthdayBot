using AutoMapper;
using BirthdayBot.BLL.Commands.Notes;
using BirthdayBot.BLL.Resources;
using BirthdayBot.Core.Resources;
using BirthdayBot.DAL.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using RapidBots.Constants;
using RapidBots.Types.Attributes;
using RapidBots.Types.Core;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace BirthdayBot.BLL.Commands.People.Chats
{
    [ChatType(ChatType.Private)]
    public class AddPeopleFromChat : Command
    {
        private readonly BotClient botClient;

        public AddPeopleFromChat(IMapper mapper, BotClient botClient)
        {
            this.botClient = botClient;
        }

        public override string Key => CommandKeys.AddPeopleFromChat;

        public override async Task Execute(Update update, TelegramUser user = null, IServiceScope actionScope = null)
        {
            var repository = actionScope.ServiceProvider.GetService<IRepository>();
            var actionsManager = actionScope.ServiceProvider.GetService<ActionManager>();
            var resources = actionScope.ServiceProvider.GetService<IStringLocalizer<SharedResources>>();

            await botClient.SendTextMessageAsync(update.CallbackQuery.From.Id, resources["ADD_PEOPLE_FROM_CHAT_TEXT"], parseMode: ParseMode.Html, disableNotification: true);

            update.CallbackQuery.Data = $"{update.CallbackQuery.Data}?{CallbackParams.Page}=0";
            await actionsManager.FindCommandByType<AddByChats>().Execute(update, user, actionScope);
        }
    }
}
