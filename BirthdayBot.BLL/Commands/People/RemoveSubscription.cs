using BirthdayBot.BLL.Menus.Notes;
using BirthdayBot.BLL.Resources;
using BirthdayBot.Core.Resources;
using BirthdayBot.DAL.Entities;
using BirthdayBot.DAL.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using RapidBots.Constants;
using RapidBots.Extensions;
using RapidBots.Types.Attributes;
using RapidBots.Types.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace BirthdayBot.BLL.Commands.People
{
    [ChatType(ChatType.Private)]
    [ExpectedParams("targetId", CallbackParams.Page)]
    [ExpectedParams("chi", "chp", "targetId")]
    public class RemoveSubscription : Command
    {
        private readonly BotClient botClient;

        public RemoveSubscription(BotClient botClient)
        {
            this.botClient = botClient;
        }

        public override string Key => CommandKeys.RemoveSubscription;

        public override async Task Execute(Update update, TelegramUser user = null, IServiceScope actionScope = null)
        {
            var resources = actionScope.ServiceProvider.GetService<IStringLocalizer<SharedResources>>();
            var repository = actionScope.ServiceProvider.GetService<IRepository>();

            TUser dbUser = (user as TUser) ?? await repository.GetAsync<TUser>(true, u => u.Id == update.CallbackQuery.From.Id, include: u => u.Include(x => x.Subscriptions));

            if (dbUser?.Subscriptions == null)
            {
                await repository.LoadCollectionAsync(dbUser, x => x.Subscriptions);
            }

            var qParams = new Dictionary<string, string>();
            var updateParams = update.GetParams();
            long targetId = Convert.ToInt32(updateParams["targetId"]);
            qParams.Add("targetId", updateParams["targetId"]);
            if (updateParams.ContainsKey(CallbackParams.Page))
            {
                qParams.Add(CallbackParams.Page, updateParams[CallbackParams.Page]);
            }
            else
            {
                qParams.Add("chi", updateParams["chi"]);
                qParams.Add("chp", updateParams["chp"]);
            }

            var target = dbUser.Subscriptions.First(x => x.TargetId == targetId);
            if(target == null)
            {
                throw new ArgumentException();
            }

            SubscriptionRemoveConfirmation menu = new SubscriptionRemoveConfirmation(resources, qParams);

            try{await botClient.AnswerCallbackQueryAsync(update.CallbackQuery.Id);}catch{}
            try
            {
                await botClient.DeleteMessageAsync(update.CallbackQuery.Message.Chat.Id, update.CallbackQuery.Message.MessageId);
            }
            catch
            { }
            await botClient.SendTextMessageAsync(update.CallbackQuery.Message.Chat.Id, menu.GetDefaultTitle(actionScope), replyMarkup: menu.GetMarkup(actionScope), parseMode: Telegram.Bot.Types.Enums.ParseMode.Html);
        }
    }
}
