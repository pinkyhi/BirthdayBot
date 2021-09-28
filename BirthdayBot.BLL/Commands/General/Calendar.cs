using BirthdayBot.BLL.Menus;
using BirthdayBot.BLL.Resources;
using BirthdayBot.Core.Resources;
using BirthdayBot.DAL.Entities;
using BirthdayBot.DAL.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using RapidBots.Extensions;
using RapidBots.Types.Attributes;
using RapidBots.Types.Core;
using System;
using System.Linq;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace BirthdayBot.BLL.Commands.General
{
    [ChatType(ChatType.Private)]
    [ExpectedParams()]
    [ExpectedParams("month")]
    public class Calendar : Command
    {
        private readonly BotClient botClient;

        public Calendar(BotClient botClient)
        {
            this.botClient = botClient;
        }

        public override string Key => CommandKeys.Calendar;

        public override async Task Execute(Update update, TelegramUser user = null, IServiceScope actionScope = null)
        {
            var repository = actionScope.ServiceProvider.GetService<IRepository>();
            var actionsManager = actionScope.ServiceProvider.GetService<ActionManager>();
            var resources = actionScope.ServiceProvider.GetService<IStringLocalizer<SharedResources>>();

            TUser dbUser = (user as TUser) ?? await repository.GetAsync<TUser>(false, u => u.Id == (update.Message?.From?.Id ?? update.CallbackQuery?.From?.Id), include: u => u.Include(x => x.Subscriptions).ThenInclude(x => x.Target).Include(x => x.Subscribers).Include(x => x.Notes));

            if (dbUser?.Subscriptions == null)
            {
                await repository.LoadCollectionAsync(dbUser, x => x.Subscriptions);
                foreach(var sub in dbUser.Subscriptions)
                {
                    await repository.LoadReferenceAsync(sub, x => x.Target);
                }
                await repository.LoadCollectionAsync(dbUser, x => x.Notes);

            }
            var cParams = update.GetParams();
            string strMonth = null;
            int month = DateTime.Now.Month;
            cParams.TryGetValue("month", out strMonth);
            if(strMonth != null)
            {
                month = Convert.ToInt32(strMonth);
            }
            var subs = dbUser.Subscriptions.Where(x => x.Target.BirthDate.Month == month).Select(x => new { Name = $"@{x.Target.Username}" ?? $"{x.Target.FirstName} {x.Target.LastName}", DateStr = dbUser.GetAnotherUserDateString(x.Target), Date = x.Target.BirthDate });
            var notes = dbUser.Notes.Where(x => x.Date.Month == month).Select(x => new { Name = x.Title, DateStr = x.Date.ToShortDateString(), Date = x.Date});
            var countPerMonth = dbUser.Subscriptions.Select(x => x.Target.BirthDate.Month).Concat(dbUser.Notes.Select(x => x.Date.Month));

            string format = "{0} - {1};";
            int monthNow = DateTime.Now.Month;
            var strs = subs.Concat(notes).OrderBy(x => monthNow - x.Date.Month < 0 ? monthNow - x.Date.Month + 12 : monthNow - x.Date.Month).Select(x => string.Format(format, x.Name, x.DateStr));
            CalendarMenu menu = new CalendarMenu(resources, month, string.Join('\n', strs), countPerMonth.GroupBy(x => x));
            if (update.CallbackQuery != null)
            {
                try { await botClient.AnswerCallbackQueryAsync(update.CallbackQuery.Id); } catch { }
                try
                {
                    await botClient.EditMessageTextAsync(update.CallbackQuery.Message.Chat.Id, update.CallbackQuery.Message.MessageId, menu.GetDefaultTitle(actionScope), parseMode: ParseMode.Html);
                    await botClient.EditMessageReplyMarkupAsync(update.CallbackQuery.Message.Chat.Id, update.CallbackQuery.Message.MessageId, replyMarkup: menu.GetMarkup(actionScope) as InlineKeyboardMarkup);

                }
                catch
                {
                    try
                    {
                        await botClient.DeleteMessageAsync(update.CallbackQuery.Message.Chat.Id, update.CallbackQuery.Message.MessageId);
                    }
                    catch
                    { };
                    await botClient.SendTextMessageAsync(update.Message?.Chat?.Id ?? update.CallbackQuery.Message.Chat.Id, menu.GetDefaultTitle(actionScope), replyMarkup: menu.GetMarkup(actionScope), parseMode: ParseMode.Html);
                }
            }
            else
            {
                await botClient.SendTextMessageAsync(update.Message?.Chat?.Id ?? update.CallbackQuery.Message.Chat.Id, menu.GetDefaultTitle(actionScope), replyMarkup: menu.GetMarkup(actionScope), parseMode: ParseMode.Html);
            }
        }
    }
}
