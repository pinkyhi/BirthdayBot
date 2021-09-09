using BirthdayBot.BLL.Resources;
using BirthdayBot.Core.Resources;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using RapidBots.Types.Attributes;
using RapidBots.Types.Core;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace BirthdayBot.BLL.Commands.General
{
    [ChatType(ChatType.Private)]
    public class Help : Command
    {
        private readonly BotClient botClient;

        public Help(BotClient botClient)
        {
            this.botClient = botClient;
        }

        public override string Key => CommandKeys.Help;

        public override async Task Execute(Update update, TelegramUser user = null, IServiceScope actionScope = null)
        {
            var resources = actionScope.ServiceProvider.GetService<IStringLocalizer<SharedResources>>();

            await botClient.SendTextMessageAsync(update.Message?.Chat?.Id ?? update.CallbackQuery.Message.Chat.Id, resources["HELP_PRIVATE_TEXT"], parseMode: ParseMode.Markdown);
        }
    }
}
