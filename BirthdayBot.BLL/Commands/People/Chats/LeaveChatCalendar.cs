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

namespace BirthdayBot.BLL.Commands.People.Chats
{
    [ChatType(ChatType.Private)]
    [ExpectedParams("chi", "chp", "chsP")]
    public class LeaveChatCalendar : Command
    {
        private readonly BotClient botClient;

        public LeaveChatCalendar(BotClient botClient)
        {
            this.botClient = botClient;
        }

        public override string Key => CommandKeys.LeaveChatCalendar;

        public override async Task Execute(Update update, TelegramUser user = null, IServiceScope actionScope = null)
        {
            var resources = actionScope.ServiceProvider.GetService<IStringLocalizer<SharedResources>>();
            var repository = actionScope.ServiceProvider.GetService<IRepository>();

            TUser dbUser = (user as TUser) ?? await repository.GetAsync<TUser>(false, u => u.Id == update.CallbackQuery.From.Id, x => x.Include(x => x.ChatMembers));

            if (dbUser?.ChatMembers == null)
            {
                await repository.LoadCollectionAsync(dbUser, x => x.ChatMembers);
            }

            var qParams = new Dictionary<string, string>();

            var updateParams = update.GetParams();
            qParams.Add("chsP", updateParams["chsP"]);
            qParams.Add("chi", updateParams["chi"]);
            qParams.Add(CallbackParams.Page, updateParams["chp"]);
            long chi = Convert.ToInt64(updateParams["chi"]);
            if (!dbUser.ChatMembers.Any(x => x.ChatId == chi))
            {
                throw new ArgumentException();
            }
            var chat = await repository.GetAsync<DAL.Entities.Chat>(false, x => x.Id == chi);

            LeaveChatConfirmMenu menu = new LeaveChatConfirmMenu(resources, qParams, chat);
            bool ex = false;
            try
            {
                await botClient.EditMessageTextAsync(update.CallbackQuery.Message.Chat.Id, update.CallbackQuery.Message.MessageId, menu.GetDefaultTitle(), parseMode: ParseMode.Html);
                await botClient.EditMessageReplyMarkupAsync(update.CallbackQuery.Message.Chat.Id, update.CallbackQuery.Message.MessageId, menu.GetMarkup(actionScope) as InlineKeyboardMarkup);
            }
            catch
            {
                ex = true;
            }

            if(ex == true)
            {
                await botClient.DeleteMessageAsync(update.CallbackQuery.Message.Chat.Id, update.CallbackQuery.Message.MessageId);
                await botClient.SendTextMessageAsync(update.CallbackQuery.Message.Chat.Id, menu.GetDefaultTitle(actionScope), replyMarkup: menu.GetMarkup(actionScope), parseMode: Telegram.Bot.Types.Enums.ParseMode.Html);
            }

        }
    }
}
