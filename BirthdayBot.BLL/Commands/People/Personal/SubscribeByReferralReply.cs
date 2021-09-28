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

namespace BirthdayBot.BLL.Commands.People.Personal
{
    [ChatType(ChatType.Private)]
    [ExpectedParams("userId")]
    public class SubscribeByReferralReply : Command
    {
        private readonly BotClient botClient;

        public SubscribeByReferralReply(BotClient botClient)
        {
            this.botClient = botClient;
        }

        public override string Key => CommandKeys.SubscribeByReferralReply;

        public override async Task Execute(Update update, TelegramUser user = null, IServiceScope actionScope = null)
        {
            //Initialisation
            long targetId = Convert.ToInt64(update.GetParams()["userId"]);
            var repository = actionScope.ServiceProvider.GetService<IRepository>();
            var actionsManager = actionScope.ServiceProvider.GetService<ActionManager>();
            var resources = actionScope.ServiceProvider.GetService<IStringLocalizer<SharedResources>>();
            TUser dbUser = user as TUser ?? await repository.GetAsync<TUser>(true, u => u.Id == update.CallbackQuery.From.Id, include: u => u.Include(x => x.Subscriptions).ThenInclude(x => x.Target));
            if (dbUser.Subscriptions == null)
            {
                await repository.LoadCollectionAsync(dbUser, x => x.Subscriptions);
                await repository.LoadCollectionAsync(dbUser, x => x.Subscribers);
            }

            // Logic
            try
            {
                var target = await repository.GetAsync<TUser>(true, x => x.Id == targetId && x.Id != dbUser.Id && x.RegistrationDate != null);
                
                if (target != null)
                {
                    await botClient.DeleteMessageAsync(update.CallbackQuery.From.Id, update.CallbackQuery.Message.MessageId);

                    if (dbUser.Subscriptions.FirstOrDefault(x => x.TargetId == target.Id) != null)
                    {
                        await botClient.SendTextMessageAsync(update.CallbackQuery.From.Id, resources["ADD_PERSONAL_INPUT_DUPLICATE"], parseMode: Telegram.Bot.Types.Enums.ParseMode.Html);
                    }
                    if (dbUser.Subscriptions.Count < Limitations.SubsLimit)
                    {
                        dbUser.Subscriptions.Add(new Subscription() { IsStrong = false, Subscriber = dbUser, Target = target });
                        await repository.UpdateAsync(dbUser);
                        await botClient.SendTextMessageAsync(update.CallbackQuery.From.Id, resources["SUBSCRIBE_ON_MEMBER_SUCCESS"], parseMode: Telegram.Bot.Types.Enums.ParseMode.Html);
                    }
                    else
                    {
                        await botClient.SendTextMessageAsync(update.CallbackQuery.From.Id, resources["SUBSCRIPTIONS_LIMIT"], parseMode: Telegram.Bot.Types.Enums.ParseMode.Html);
                    }
                }
                else
                {
                    await botClient.SendTextMessageAsync(update.CallbackQuery.From.Id, resources["REFERRAL_TARGET_UNKNOWN"], parseMode: Telegram.Bot.Types.Enums.ParseMode.Html);
                    return;
                }
            }
            catch
            {
                await botClient.SendTextMessageAsync(update.CallbackQuery.From.Id, resources["SUBSCRIBE_REFERRAL_ERROR"], parseMode: Telegram.Bot.Types.Enums.ParseMode.Html);
                return;
            }
        }
    }
}
