﻿using BirthdayBot.BLL.Menus.People;
using BirthdayBot.BLL.Resources;
using BirthdayBot.Core.Resources;
using BirthdayBot.DAL.Entities;
using BirthdayBot.DAL.Interfaces;
using RapidBots.Types.Attributes;
using Telegram.Bot.Types.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using RapidBots.Types.Core;
using System;
using System.Linq;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using RapidBots.Extensions;
using System.Collections.Generic;

namespace BirthdayBot.BLL.Commands.People.Chats
{
    [ChatType(ChatType.Private)]
    [ExpectedParams("chi", "chp", "targetId")]
    public class ChatSubscriptionPreview : Command
    {
        private readonly BotClient botClient;

        public ChatSubscriptionPreview(BotClient botClient)
        {
            this.botClient = botClient;
        }

        public override string Key => CommandKeys.ChatSubscriptionPreview;

        public override async Task Execute(Update update, TelegramUser user = null, IServiceScope actionScope = null)
        {
            var resources = actionScope.ServiceProvider.GetService<IStringLocalizer<SharedResources>>();
            var repository = actionScope.ServiceProvider.GetService<IRepository>();

            TUser dbUser = (user as TUser) ?? await repository.GetAsync<TUser>(false, u => u.Id == update.CallbackQuery.From.Id, include: u => u.Include(x => x.Subscriptions).ThenInclude(x => x.Target).Include(x => x.Subscribers));

            if (dbUser?.Subscriptions == null)
            {
                await repository.LoadCollectionAsync(dbUser, x => x.Subscriptions);
                await repository.LoadCollectionAsync(dbUser, x => x.Subscribers);
            }

            var qParams = new Dictionary<string, string>();
            var updateParams = update.GetParams();
            long targetId = Convert.ToInt32(updateParams["targetId"]);
            qParams.Add("targetId", updateParams["targetId"]);

            qParams.Add("chi", updateParams["chi"]);
            qParams.Add("chp", updateParams["chp"]);


            var target = await repository.GetAsync<TUser>(true, x => x.Id == targetId);

            if (dbUser.Subscriptions.FirstOrDefault(x => x.TargetId == target.Id) != null)
            {
                await botClient.AnswerCallbackQueryAsync(update.CallbackQuery.Id, text: resources["SUBSCRIBE_ON_MEMBER_DUPLICATE"], showAlert: true);
                return;
            }
            var subscription = new Subscription() { IsStrong = false, Subscriber = dbUser, Target = target };

            SubscriptionPreviewMenu menu = new SubscriptionPreviewMenu(resources, qParams, subscription);

            try { await botClient.AnswerCallbackQueryAsync(update.CallbackQuery.Id); } catch { }
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
