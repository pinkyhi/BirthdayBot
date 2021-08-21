using BirthdayBot.BLL.Menus.Notes;
using BirthdayBot.BLL.Menus.People;
using BirthdayBot.BLL.Menus.Settings;
using BirthdayBot.BLL.Resources;
using BirthdayBot.Core.Resources;
using BirthdayBot.DAL.Entities;
using BirthdayBot.DAL.Interfaces;
using Microsoft.AspNetCore.WebUtilities;
using RapidBots.Types.Attributes;
using Telegram.Bot.Types.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using RapidBots.Constants;
using RapidBots.Types.Core;
using System;
using System.Linq;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using RapidBots.Extensions;

namespace BirthdayBot.BLL.Commands.People
{
    [ChatType(ChatType.Private)]
    [ExpectedParams("property", CallbackParams.Page)]
    public class OpenSubscription : Command
    {
        private readonly BotClient botClient;

        public OpenSubscription(BotClient botClient)
        {
            this.botClient = botClient;
        }

        public override string Key => CommandKeys.OpenSubscription;

        public override async Task Execute(Update update, TelegramUser user = null, IServiceScope actionScope = null)
        {
            var resources = actionScope.ServiceProvider.GetService<IStringLocalizer<SharedResources>>();
            var repository = actionScope.ServiceProvider.GetService<IRepository>();

            TUser dbUser = (user as TUser) ?? await repository.GetAsync<TUser>(false, u => u.Id == update.CallbackQuery.From.Id, include: u => u.Include(x => x.Notes));

            if (dbUser?.Subscriptions == null)
            {
                await repository.LoadCollectionAsync(dbUser, x => x.Subscriptions);
            }

            long targetId = Convert.ToInt32(update.GetParams()["targetId"]);
            int page = Convert.ToInt32(update.GetParams()[CallbackParams.Page]);

            var subscription = dbUser.Subscriptions.First(x => x.TargetId == targetId);

            SubscriptionMenu menu = new SubscriptionMenu(resources, page, subscription);

            try{await botClient.AnswerCallbackQueryAsync(update.CallbackQuery.Id);}catch{}
            try
            {
                await botClient.DeleteMessageAsync(update.CallbackQuery.Message.Chat.Id, update.CallbackQuery.Message.MessageId);
            }
            catch
            { }
            await botClient.SendTextMessageAsync(update.CallbackQuery.Message.Chat.Id, menu.GetDefaultTitle(actionScope), replyMarkup: menu.GetMarkup(actionScope));
        }
    }
}
