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
using System.Threading.Tasks;

namespace BirthdayBot.Quartz.Jobs
{
    public class PersonalBirthdayNotificationJob : IJob
    {
        private readonly ILogger<PersonalBirthdayNotificationJob> logger;
        private readonly IStringLocalizer<SharedResources> resources;
        private readonly IRepository repository;
        private readonly BotClient botClient;
        private readonly RapidBotsOptions options;

        public PersonalBirthdayNotificationJob(ILogger<PersonalBirthdayNotificationJob> logger, IStringLocalizer<SharedResources> resources, IRepository repository, BotClient botClient, RapidBotsOptions options)
        {
            this.logger = logger;
            this.resources = resources;
            this.repository = repository;
            this.botClient = botClient;
            this.options = options;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            logger.LogInformation($"PersonalBirthdayNotification job started at: {DateTime.Now}");
            if (DateTime.UtcNow.Date == context.FireTimeUtc.UtcDateTime.Date)
            {
                try
                {
                    DateTime uNow = DateTime.Now.ToUniversalTime();
                    var utcHour = uNow.Hour;
 
                    var notesEnum = await repository.GetRangeAsync<Note>(false, x =>
                    {
                        var hoursOffset = Convert.ToInt32((x.User.Timezone.DstOffset + x.User.Timezone.RawOffset) / 3600);
                        DateTime now = uNow.AddHours(hoursOffset);

                        if (x.LastNotificationTime.HasValue && x.LastNotificationTime.Value.Date.Equals(now.Date))
                        {
                            return false;
                        }
                        var hourInCountry = (utcHour + hoursOffset + 24) % 24;
                        if (hourInCountry >= 0)
                        {
                            var clearDate = x.Date.AddYears(now.Year - x.Date.Year);
                            if (x.Date.Month == 2 && x.Date.Day == 29)
                            {
                                if (clearDate.Day == 28)
                                {
                                    clearDate = clearDate.AddDays(1);
                                }
                            }
                            int dayDifference = Convert.ToInt32(Math.Floor((double)(clearDate.Date.Ticks - now.Date.Ticks) / TimeSpan.TicksPerDay));
                            if (x.IsStrong)
                            {
                                return x.User.Settings.StrongNotification_0 == dayDifference || x.User.Settings.StrongNotification_1 == dayDifference || x.User.Settings.StrongNotification_2 == dayDifference;
                            }
                            else
                            {
                                return x.User.Settings.CommonNotification_0 == dayDifference;
                            }
                        }
                        return false;
                    }, x => x.Include(x => x.User));
                    var notes = new List<Note>(notesEnum);
                    var subsEnum = await repository.GetRangeAsync<Subscription>(false, x =>
                    {
                        var hoursOffset = Convert.ToInt32((x.Target.Timezone.DstOffset + x.Target.Timezone.RawOffset) / 3600);
                        DateTime now = uNow.AddHours(hoursOffset);

                        if (x.LastNotificationTime.HasValue && x.LastNotificationTime.Value.Date.Equals(now.Date))
                        {
                            return false;
                        }
                        var hourInCountry = (utcHour + Convert.ToInt32((x.Target.Timezone.DstOffset + x.Target.Timezone.RawOffset) / 3600) + 24) % 24;
                        if (hourInCountry >= 0)
                        {
                            var clearDate = x.Target.BirthDate.AddYears(now.Year - x.Target.BirthDate.Year);
                            if (x.Target.BirthDate.Month == 2 && x.Target.BirthDate.Day == 29)
                            {
                                if (clearDate.Day == 28)
                                {
                                    clearDate = clearDate.AddDays(1);
                                }
                            }
                            int dayDifference = Convert.ToInt32(Math.Floor((double)(clearDate.Date.Ticks - now.Date.Ticks) / TimeSpan.TicksPerDay));
                            if (x.IsStrong)
                            {
                                return x.Subscriber.Settings.StrongNotification_0 == dayDifference || x.Subscriber.Settings.StrongNotification_1 == dayDifference || x.Subscriber.Settings.StrongNotification_2 == dayDifference;
                            }
                            else
                            {
                                return x.Subscriber.Settings.CommonNotification_0 == dayDifference;
                            }
                        }
                        return false;
                    }, x => x.Include(x => x.Target).Include(x => x.Subscriber));
                    var subs = new List<Subscription>(subsEnum);

                    // Sending notifications
                    foreach (var note in notes)
                    {
                        var hoursOffset = Convert.ToInt32((note.User.Timezone.DstOffset + note.User.Timezone.RawOffset) / 3600);
                        DateTime now = uNow.AddHours(hoursOffset);
                        CultureInfo.CurrentCulture = new CultureInfo(note.User?.LanguageCode ?? options.DefaultLanguageCode);
                        CultureInfo.CurrentUICulture = new CultureInfo(note.User?.LanguageCode ?? options.DefaultLanguageCode);
                        try
                        {
                            await botClient.SendTextMessageAsync(note.UserId, resources["PERSONAL_NOTE_NOTIFICATION_TEXT", note.Title, note.Date.ToShortDateString()], parseMode: Telegram.Bot.Types.Enums.ParseMode.Html);
                            note.LastNotificationTime = now;
                        }
                        catch (Exception ex)
                        {
                            logger.LogError(ex.ToString());
                        }
                    }
                    repository.UpdateRange(notes);
                    foreach (var sub in subs)
                    {
                        var hoursOffset = Convert.ToInt32((sub.Target.Timezone.DstOffset + sub.Target.Timezone.RawOffset) / 3600);
                        DateTime now = uNow.AddHours(hoursOffset);
                        CultureInfo.CurrentCulture = new CultureInfo(sub.Subscriber?.LanguageCode ?? options.DefaultLanguageCode);
                        CultureInfo.CurrentUICulture = new CultureInfo(sub.Subscriber?.LanguageCode ?? options.DefaultLanguageCode);
                        try
                        {
                            await botClient.SendTextMessageAsync(sub.SubscriberId, resources["PERSONAL_SUB_NOTIFICATION_TEXT", sub.Target.Username ?? $"{sub.Target.FirstName} {sub.Target.LastName}", sub.Subscriber.GetAnotherUserDateString(sub.Target)], parseMode: Telegram.Bot.Types.Enums.ParseMode.Html);
                            sub.LastNotificationTime = now;
                        }
                        catch (Exception ex)
                        {
                            logger.LogError(ex.ToString());
                        }
                    }
                    repository.UpdateRange(subs);
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
