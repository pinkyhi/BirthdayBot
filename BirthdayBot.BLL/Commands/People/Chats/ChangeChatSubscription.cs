using BirthdayBot.Core.Resources;
using BirthdayBot.DAL.Entities;
using BirthdayBot.DAL.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using RapidBots.Types.Core;
using System;
using System.Linq;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using RapidBots.Types.Attributes;
using Telegram.Bot.Types.Enums;
using BirthdayBot.Core.Const;
using BirthdayBot.BLL.Resources;
using RapidBots.Extensions;

namespace BirthdayBot.BLL.Commands.People.Chats
{
    [ChatType(ChatType.Private)]
    [ExpectedParams("chatId")]
    public class ChangeChatSubscription : Command
    {
        private readonly BotClient botClient;

        public ChangeChatSubscription(BotClient botClient)
        {
            this.botClient = botClient;
        }

        public override string Key => CommandKeys.ChangeChatSubscription;

        public override async Task Execute(Update update, TelegramUser user = null, IServiceScope actionScope = null)
        {
            //Initialisation
            long chatId = Convert.ToInt64(update.GetParams()["chatId"]);
            var repository = actionScope.ServiceProvider.GetService<IRepository>();
            var actionsManager = actionScope.ServiceProvider.GetService<ActionManager>();
            var resources = actionScope.ServiceProvider.GetService<IStringLocalizer<SharedResources>>();

            var member = await repository.GetAsync<DAL.Entities.ChatMember>(true, x => x.ChatId == chatId && x.UserId == update.CallbackQuery.From.Id);
            member.IsSubscribedOnCalendar = member.IsSubscribedOnCalendar == true ? false : true;
            await repository.UpdateAsync(member);
            await botClient.AnswerCallbackQueryAsync(update.CallbackQuery.Id, resources["CHAT_SUBSCRIPTION_CHANGE"]);
            await botClient.DeleteMessageAsync(update.CallbackQuery.Message.Chat.Id, update.CallbackQuery.Message.MessageId);
        }
    }
}
