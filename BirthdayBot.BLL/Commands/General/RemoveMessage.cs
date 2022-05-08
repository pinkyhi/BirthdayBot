using BirthdayBot.BLL.Resources;
using BirthdayBot.Core.Resources;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using RapidBots.Types.Core;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using RapidBots.Types.Attributes;
using Telegram.Bot.Types.Enums;

namespace BirthdayBot.BLL.Commands.General
{
    [ChatType(ChatType.Private)]
    class RemoveMessage : Command
    {
        private readonly BotClient botClient;

        public RemoveMessage(BotClient botClient)
        {
            this.botClient = botClient;
        }

        public override string Key => CommandKeys.RemoveMessage;

        public override async Task Execute(Update update, TelegramUser user = null, IServiceScope actionScope = null)
        {
            await botClient.DeleteMessageAsync(update.CallbackQuery.Message.Chat.Id, update.CallbackQuery.Message.MessageId);
        }
    }
}
