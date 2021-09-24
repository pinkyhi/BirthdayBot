using AutoMapper;
using BirthdayBot.DAL;
using BirthdayBot.DAL.Interfaces;
using BirthdayBot.DAL.Repositories;
using BirthdayBot.Quartz.Jobs;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using Quartz;
using RapidBots.Automapper;
using RapidBots.Localization;
using System;
using System.Globalization;

namespace BirthdayBot.Extensions
{
    public static class ServiceProviderExtension
    {
        public static void AddDataAccess(this IServiceCollection services, string connectionString)
        {
            services.AddDbContext<AppDbContext>(options => {
                options.UseSqlServer(connectionString);
                options.EnableSensitiveDataLogging();
            });
            ActivatorUtilities.CreateInstance(services.BuildServiceProvider(), typeof(AppDbContext));
            services.AddScoped<IRepository, Repository>();
        }

        public static void AddAutoMapper(this IServiceCollection services)
        {
            var mapperConfig = new MapperConfiguration(mc =>
            {
                mc.AddProfile(new RapidBotsProfile());
                mc.AddMaps(AppDomain.CurrentDomain.GetAssemblies());
            });

            IMapper mapper = mapperConfig.CreateMapper();
            services.AddSingleton(mapper);
        }

        public static void AddLocalizationSettings(this IServiceCollection services, string resourcesPath = "")
        {
            services.AddScoped(typeof(IStringLocalizer<>), typeof(RapidLocalizer<>));
            services.AddLocalization(options => options.ResourcesPath = resourcesPath);
            
            string enCulture = "en";
            services.Configure<RequestLocalizationOptions>(options =>
            {
                var supportedCultures = new[]
                {
                    new CultureInfo(enCulture),
                    new CultureInfo("ru"),
                    new CultureInfo("uk")
                };


                options.SupportedUICultures = supportedCultures;
                options.SupportedCultures = supportedCultures;
                options.DefaultRequestCulture = new RequestCulture(culture: enCulture, uiCulture: enCulture);
            });
        }

        public static void AddQuartzHelper(this IServiceCollection services, string connectionString)
        {
            services.AddTransient<PersonalBirthdayNotificationJob>();

            services.AddQuartz(q =>
            {
                q.UseMicrosoftDependencyInjectionJobFactory();
                q.UsePersistentStore(s =>
                {
                    s.UseSqlServer(connectionString);
                    s.UseProperties = true;
                    s.UseJsonSerializer();
                });
                q.SchedulerId = "Scheduler-Core";

                var persNotJobKey = new JobKey("PersonalBirthdayNotificationJob");
                var chatNotJobKey = new JobKey("ChatBirthdayNotificationJob");
                var chatCheckCount = new JobKey("ChatMembersCheckJob");

                q.AddJob<PersonalBirthdayNotificationJob>(opts => {
                    opts.WithIdentity(persNotJobKey);
                });
                q.AddJob<ChatBirthdayNotificationJob>(opts => {
                    opts.WithIdentity(chatNotJobKey);
                });
                q.AddJob<ChatMembersCheckJob>(opts => {
                    opts.WithIdentity(chatCheckCount);
                });

                q.AddTrigger(opts => opts
                    .ForJob(persNotJobKey)
                    .WithIdentity("PersonalBirthdayNotification-trigger")
                    .WithCronSchedule("0 0/1 * 1/1 * ? *"));
                q.AddTrigger(opts => opts
                    .ForJob(chatNotJobKey)
                    .WithIdentity("ChatBirthdayNotificationJob-trigger")
                    .WithCronSchedule("0 0/1 * 1/1 * ? *"));
                q.AddTrigger(opts => opts
                    .ForJob(chatCheckCount)
                    .WithIdentity("ChatMembersCheckJob-trigger")
                    .WithCronSchedule("0 0/1 * 1/1 * ? *"));
            });
            services.AddQuartzServer(options =>
            {
                options.WaitForJobsToComplete = true;
            });
        }
    }
}
