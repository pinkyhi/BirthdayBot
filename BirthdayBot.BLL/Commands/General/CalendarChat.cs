using BirthdayBot.BLL.Resources;
using BirthdayBot.Core.Resources;
using BirthdayBot.DAL.Entities;
using BirthdayBot.DAL.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using RapidBots.Types.Attributes;
using RapidBots.Types.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace BirthdayBot.BLL.Commands.General
{
    [ChatType(ChatType.Group, ChatType.Supergroup)]
    public class CalendarChat : Command
    {
        private readonly BotClient botClient;

        public CalendarChat(BotClient botClient)
        {
            this.botClient = botClient;
        }

        public override string Key => CommandKeys.Calendar;

        public override async Task Execute(Update update, TelegramUser user = null, IServiceScope actionScope = null)
        {
            var repository = actionScope.ServiceProvider.GetService<IRepository>();
            var actionsManager = actionScope.ServiceProvider.GetService<ActionManager>();
            var resources = actionScope.ServiceProvider.GetService<IStringLocalizer<SharedResources>>();

            TUser dbUser = (user as TUser) ?? await repository.GetAsync<TUser>(true, u => u.Id == update.Message.From.Id, include: u => u.Include(x => x.ChatMembers).ThenInclude(x => x.User));

            long chatId = update.Message.Chat.Id;
            var chatMemberCount = await botClient.GetChatMembersCountAsync(chatId) - 1;
            var chat = await repository.GetAsync<DAL.Entities.Chat>(false, c => c.Id == chatId, x => x.Include(x => x.ChatMembers).ThenInclude(x => x.User));

            var users = chat.ChatMembers.Select(x => new { Name = $"@{x.User.Username}" ?? $"{x.User.FirstName} {x.User.LastName}", Date = x.User.BirthDate, DateStr = x.User.GetConfidentialDateString() }).GroupBy(x => x.Date.Month - 1);
            string format = "{0} - {1};\n";
            int monthNow = DateTime.Now.Month;

            List<string> monthes = new List<string>(){
                resources["JANUARY"],
                resources["FEBRUARY"],
                resources["MARCH"],
                resources["APRIL"],
                resources["MAY"],
                resources["JUNE"],
                resources["JULY"],
                resources["AUGUST"],
                resources["SEPTEMBER"],
                resources["OCTOBER"],
                resources["NOVEMBER"],
                resources["DECEMBER"]
            };

            var resultStr = $"{resources["CHAT_CALENDAR_MENU_TEXT", chat.ChatMembers.Count, chatMemberCount]}\n";
            for(int i = 0; i < 12; i++)
            {
                int month = (monthNow + i) % 12;
                var usersNow = users.FirstOrDefault(x => x.Key == month);
                if(usersNow == null)
                {
                    continue;
                }
                var strs = $"<b>{monthes[month]}</b>\n";
                foreach(var userNow in usersNow)
                {
                    strs += $"{string.Format(format, userNow.Name, userNow.DateStr)}";
                }
                resultStr += strs;
            }
            InlineKeyboardButton joinChatCalendar = new InlineKeyboardButton() { Text = resources["JOIN_CHAT_CALENDAR_BUTTON"], Url = string.Format("https://t.me/yourdate_bot?start={0}", update.Message?.Chat?.Id ?? update.CallbackQuery.Message.Chat.Id) };
            await botClient.SendTextMessageAsync(update.Message?.Chat?.Id ?? update.CallbackQuery.Message.Chat.Id, resultStr, parseMode: ParseMode.Html, disableNotification: true, replyMarkup: new InlineKeyboardMarkup(joinChatCalendar));
        }
    }
}
