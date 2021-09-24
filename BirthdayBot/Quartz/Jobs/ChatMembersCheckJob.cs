using BirthdayBot.Core.Resources;
using BirthdayBot.DAL.Entities;
using BirthdayBot.DAL.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Quartz;
using RapidBots;
using RapidBots.Types.Core;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace BirthdayBot.Quartz.Jobs
{
    public class ChatMembersCheckJob : IJob
    {
        private readonly ILogger<ChatMembersCheckJob> logger;
        private readonly IStringLocalizer<SharedResources> resources;
        private readonly IRepository repository;
        private readonly BotClient botClient;
        private readonly RapidBotsOptions options;

        public ChatMembersCheckJob(ILogger<ChatMembersCheckJob> logger, IStringLocalizer<SharedResources> resources, IRepository repository, BotClient botClient, RapidBotsOptions options)
        {
            this.logger = logger;
            this.resources = resources;
            this.repository = repository;
            this.botClient = botClient;
            this.options = options;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            if (DateTime.UtcNow.Date == context.FireTimeUtc.UtcDateTime.Date)
            {
                try
                {
                    DateTime uNow = DateTime.Now.ToUniversalTime();

                    var chatsEnum = await repository.GetRangeAsync<Chat>(false, x =>
                    {
                        if(x.NotificationsCount < 3)
                        {
                            var utcAdding = x.AddingDate.ToUniversalTime();

                            if ((uNow - utcAdding).TotalHours > x.NotificationsCount * 2)
                            {
                                return true;
                            }
                            else
                            {
                                return false;
                            }
                        }
                        else
                        {
                            return false;
                        }          
                    }, include: x => x.Include(x => x.ChatMembers).ThenInclude(x => x.User));
                    var chats = chatsEnum.ToList();
                    for (int i = 0; i < chats.Count; i++)
                    {
                        var chat = chats[i];

                        try
                        {
                            int chatMemberCount = await botClient.GetChatMembersCountAsync(chat.Id) - 1;
                            
                            if (chatMemberCount != chat.ChatMembers.Count)
                            {
                                string chatAvgLanCode = chat.ChatMembers.Select(x => x.User.LanguageCode).GroupBy(x => x).OrderByDescending(x => x.Count()).First().Key;
                                CultureInfo.CurrentCulture = new CultureInfo(chatAvgLanCode ?? options.DefaultLanguageCode);
                                CultureInfo.CurrentUICulture = new CultureInfo(chatAvgLanCode ?? options.DefaultLanguageCode);

                                InlineKeyboardButton joinChatCalendar = new InlineKeyboardButton() { Text = resources["JOIN_CHAT_CALENDAR_BUTTON"], Url = string.Format("https://t.me/birthdayMaster_bot?start={0}", chat.Id) };

                                chat.NotificationsCount++;
                                switch (chat.NotificationsCount)
                                {
                                    case 1:
                                        await botClient.SendTextMessageAsync(chat.Id, resources["FIRST_CHAT_MEMBERS_COUNT_NOTIFICATION", chat.ChatMembers.Count, chatMemberCount], parseMode: ParseMode.Html, replyMarkup: new InlineKeyboardMarkup(joinChatCalendar));
                                        break;
                                    case 2:
                                        await botClient.SendTextMessageAsync(chat.Id, resources["SECOND_CHAT_MEMBERS_COUNT_NOTIFICATION", chat.ChatMembers.Count, chatMemberCount], parseMode: ParseMode.Html, replyMarkup: new InlineKeyboardMarkup(joinChatCalendar));
                                        break;
                                    case 3:
                                        await botClient.SendTextMessageAsync(chat.Id, resources["THIRD_CHAT_MEMBERS_COUNT_NOTIFICATION", chat.ChatMembers.Count, chatMemberCount], parseMode: ParseMode.Html, replyMarkup: new InlineKeyboardMarkup(joinChatCalendar));
                                        break;
                                }
                            }
                            else
                            {
                                chat.NotificationsCount = 3;
                            }
                        }
                        catch(Exception ex)
                        {
                            logger.LogError(ex.ToString());
                        }
                        
                    }
                    await repository.UpdateRangeAsync(chats);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex.ToString());
                }
            }
            else
            {
                logger.LogWarning($"MISSFIRE: {context.JobDetail}, {context.FireTimeUtc}\n UTC Now: {DateTime.UtcNow}");
            }
        }
    }
}
