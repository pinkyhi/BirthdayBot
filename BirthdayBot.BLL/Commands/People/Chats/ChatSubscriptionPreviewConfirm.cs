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
using System.Collections.Generic;

namespace BirthdayBot.BLL.Commands.People.Chats
{
    [ChatType(ChatType.Private)]
    [ExpectedParams("chatId", "chatPage", "targetId")]
    public class ChatSubscriptionPreviewConfirm : Command
    {
        private readonly BotClient botClient;

        public ChatSubscriptionPreviewConfirm(BotClient botClient)
        {
            this.botClient = botClient;
        }

        public override string Key => CommandKeys.SubscriptionPreviewConfirm;

        public override async Task Execute(Update update, TelegramUser user = null, IServiceScope actionScope = null)
        {
            var resources = actionScope.ServiceProvider.GetService<IStringLocalizer<SharedResources>>();
            var repository = actionScope.ServiceProvider.GetService<IRepository>();

            TUser dbUser = (user as TUser) ?? await repository.GetAsync<TUser>(false, u => u.Id == update.CallbackQuery.From.Id, include: u => u.Include(x => x.Subscriptions).ThenInclude(x => x.Target));

            if (dbUser?.Subscriptions == null)
            {
                await repository.LoadCollectionAsync(dbUser, x => x.Subscriptions);
                await repository.LoadCollectionAsync(dbUser, x => x.Subscribers);
            }

            var qParams = new Dictionary<string, string>();
            var updateParams = update.GetParams();
            long targetId = Convert.ToInt32(updateParams["targetId"]);
            qParams.Add("targetId", updateParams["targetId"]);
            qParams.Add("chatId", updateParams["chatId"]);
            qParams.Add("chatPage", updateParams["chatPage"]);

            var target = await repository.GetAsync<TUser>(true, x => x.Id == targetId);

            if (dbUser.Subscriptions.FirstOrDefault(x => x.TargetId == target.Id) != null)
            {
                await botClient.AnswerCallbackQueryAsync(update.CallbackQuery.Id, text: resources["SUBSCRIBE_ON_MEMBER_DUPLICATE"], showAlert: true);
            }
            else
            {
                dbUser.Subscriptions.Add(new Subscription() { IsStrong = false, Subscriber = dbUser, Target = target });
                dbUser.CurrentStatus = null;
                await repository.UpdateAsync(dbUser);
                try
                {
                    await botClient.AnswerCallbackQueryAsync(update.CallbackQuery.Id, resources["SUBSCRIBE_ON_MEMBER_SUCCESS"]);
                }
                catch { }
            }
            var subscription = dbUser.Subscriptions.First(x => x.TargetId == targetId);

            SubscriptionMenu menu = new SubscriptionMenu(resources, qParams, subscription);

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
