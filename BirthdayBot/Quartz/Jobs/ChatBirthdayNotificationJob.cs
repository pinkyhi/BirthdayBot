﻿using BirthdayBot.Core.Resources;
using BirthdayBot.DAL.Entities;
using BirthdayBot.DAL.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Quartz;
using RapidBots.Types.Core;
using System;
using System.Threading.Tasks;

namespace BirthdayBot.Quartz.Jobs
{
    public class ChatBirthdayNotificationJob : IJob
    {
        private readonly ILogger<PersonalBirthdayNotificationJob> logger;
        private readonly IStringLocalizer<SharedResources> resources;
        private readonly IRepository repository;
        private readonly BotClient botClient;

        public ChatBirthdayNotificationJob(ILogger<PersonalBirthdayNotificationJob> logger, IStringLocalizer<SharedResources> resources, IRepository repository, BotClient botClient)
        {
            this.logger = logger;
            this.resources = resources;
            this.repository = repository;
            this.botClient = botClient;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            if (DateTime.UtcNow.Date == context.FireTimeUtc.UtcDateTime.Date)
            {
                try
                {
                    DateTime uNow = DateTime.Now.ToUniversalTime();
                    var utcHour = uNow.Hour;

                    var members = await repository.GetRangeAsync<ChatMember>(false, x =>
                    {
                        var hoursOffset = Convert.ToInt32((x.User.Timezone.DstOffset + x.User.Timezone.RawOffset) / 3600);
                        DateTime now = uNow.AddHours(hoursOffset).Date;
                        var hourInCountry = (utcHour + Convert.ToInt32((x.User.Timezone.DstOffset + x.User.Timezone.RawOffset) / 3600)) % 24;
                        if (hourInCountry == 9)
                        {
                            var clearDate = x.User.BirthDate.AddYears(now.Year - x.User.BirthDate.Year);
                            if (x.User.BirthDate.Month == 2 && x.User.BirthDate.Day == 29)
                            {
                                if (clearDate.Day == 28)
                                {
                                    clearDate = clearDate.AddDays(1);
                                }
                            }
                            int dayDifference = Convert.ToInt32(Math.Floor((double)(clearDate.Date.Ticks - now.Date.Ticks) / TimeSpan.TicksPerDay));
                            return dayDifference == 0;
                        }
                        return false;
                    }, x => x.Include(x => x.Chat).Include(x => x.User));

                    foreach (var member in members)
                    {
                        try
                        {
                            await botClient.SendTextMessageAsync(member.ChatId, resources["CHAT_BIRTH_NOTIFICATION_TEXT", member.User.Username ?? $"{member.User.FirstName} {member.User.LastName}", member.User.GetConfidentialDateString(), member.User.Timezone.TimeZoneName], parseMode: Telegram.Bot.Types.Enums.ParseMode.Html);
                        }
                        catch(Exception ex) {logger.LogError(ex.ToString());}
                    }
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
