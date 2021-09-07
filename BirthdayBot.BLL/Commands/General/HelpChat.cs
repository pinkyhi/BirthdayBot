using AutoMapper;
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

namespace BirthdayBot.BLL.Commands.General
{
    [ChatType(ChatType.Group, ChatType.Supergroup)]
    public class HelpChat : Command
    {
        private readonly BotClient botClient;

        public HelpChat(BotClient botClient)
        {
            this.botClient = botClient;
        }

        public override string Key => CommandKeys.Help;

        public override async Task Execute(Update update, TelegramUser user = null, IServiceScope actionScope = null)
        {
            var resources = actionScope.ServiceProvider.GetService<IStringLocalizer<SharedResources>>();

            await botClient.SendTextMessageAsync(update.Message.Chat.Id, resources["HELP_CHAT_TEXT"], parseMode: ParseMode.Html);
        }
    }
}
