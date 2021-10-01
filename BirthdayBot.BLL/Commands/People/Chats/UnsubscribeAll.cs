using BirthdayBot.BLL.Menus.People;
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
using BirthdayBot.Core.Const;
using RapidBots.Constants;

namespace BirthdayBot.BLL.Commands.People.Chats
{
    [ChatType(ChatType.Private)]
    [ExpectedParams(CallbackParams.Page, "chi")]
    class UnsubscribeAll : Command
    {
        private readonly BotClient botClient;

        public UnsubscribeAll(BotClient botClient)
        {
            this.botClient = botClient;
        }

        public override string Key => CommandKeys.UnsubscribeAll;

        public override async Task Execute(Update update, TelegramUser user = null, IServiceScope actionScope = null)
        {
            var resources = actionScope.ServiceProvider.GetService<IStringLocalizer<SharedResources>>();
            var repository = actionScope.ServiceProvider.GetService<IRepository>();
            var actionsManager = actionScope.ServiceProvider.GetService<ActionManager>();
            var nextCommand = actionsManager.FindCommandByType<AddByChats>();

            TUser dbUser = (user as TUser) ?? await repository.GetAsync<TUser>(true, u => u.Id == update.CallbackQuery.From.Id, include: u => u.Include(x => x.Subscriptions).Include(x => x.Subscribers));

            if (dbUser?.Subscriptions == null)
            {
                await repository.LoadCollectionAsync(dbUser, x => x.Subscriptions);
                await repository.LoadCollectionAsync(dbUser, x => x.Subscribers);
            }

            var updateParams = update.GetParams();
            long chatId = Convert.ToInt64(updateParams["chi"]);
            var chatMembers = await repository.GetRangeAsync<DAL.Entities.ChatMember>(false, x => x.ChatId == chatId);
            dbUser.Subscriptions.RemoveAll(x => chatMembers.Any(y => y.UserId == x.TargetId));

            dbUser.CurrentStatus = null;
            await repository.UpdateAsync(dbUser);

            await botClient.AnswerCallbackQueryAsync(update.CallbackQuery.Id, resources["UNSUBSCRIBE_ALL_TEXT"]);

            await nextCommand.Execute(update, user, actionScope);
        }
    }
}
