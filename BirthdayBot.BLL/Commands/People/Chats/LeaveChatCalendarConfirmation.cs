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
using RapidBots.Constants;
using Telegram.Bot.Types.ReplyMarkups;
using Microsoft.AspNetCore.WebUtilities;

namespace BirthdayBot.BLL.Commands.People.Chats
{
    [ChatType(ChatType.Private)]
    [ExpectedParams("chi", "chsP")]
    public class LeaveChatCalendarConfirmation : Command
    {
        private readonly BotClient botClient;

        public LeaveChatCalendarConfirmation(BotClient botClient)
        {
            this.botClient = botClient;
        }

        public override string Key => CommandKeys.LeaveChatCalendarConfirmation;

        public override async Task Execute(Update update, TelegramUser user = null, IServiceScope actionScope = null)
        {
            var resources = actionScope.ServiceProvider.GetService<IStringLocalizer<SharedResources>>();
            var repository = actionScope.ServiceProvider.GetService<IRepository>();
            var actionsManager = actionScope.ServiceProvider.GetService<ActionManager>();

            TUser dbUser = (user as TUser) ?? await repository.GetAsync<TUser>(true, u => u.Id == update.CallbackQuery.From.Id, x => x.Include(x => x.ChatMembers));

            if (dbUser?.ChatMembers == null)
            {
                await repository.LoadCollectionAsync(dbUser, x => x.ChatMembers);
            }

            var qParams = new Dictionary<string, string>();

            var updateParams = update.GetParams();
            long chi = Convert.ToInt64(updateParams["chi"]);
            string chsP = updateParams["chsP"];

            if (!dbUser.ChatMembers.Any(x => x.ChatId == chi))
            {
                throw new ArgumentException();
            }

            dbUser.ChatMembers.Remove(dbUser.ChatMembers.FirstOrDefault(x => x.ChatId == chi));
            await repository.UpdateAsync(dbUser);

            await botClient.AnswerCallbackQueryAsync(update.CallbackQuery.Id, resources["CHAT_LEAVED_TEXT"]);

            update.CallbackQuery.Data = QueryHelpers.AddQueryString(CommandKeys.AddByChats, CallbackParams.Page, chsP);
            await actionsManager.FindCommandByType<AddByChats>().Execute(update, user, actionScope);

        }
    }
}
