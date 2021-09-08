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
            });
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

        public static void AddQuartzHelper(this IServiceCollection services)
        {
            services.AddTransient<PersonalBirthdayNotificationJob>();

            services.AddQuartz(q =>
            {
                q.SchedulerId = "Scheduler-Core";
                var jobKey = new JobKey("NotificationsJob");
                q.UseMicrosoftDependencyInjectionJobFactory();
                q.AddJob<PersonalBirthdayNotificationJob>(opts => {
                    opts.WithIdentity(jobKey);
                });
                // Create a trigger for the job
                q.AddTrigger(opts => opts
                    .ForJob(jobKey) // link to the HelloWorldJob
                    .WithIdentity("NotificationsJob-trigger") // give the trigger a unique name
                    .WithCronSchedule("0/5 * * 1/1 * ? *") // run every 5 seconds
                    .StartNow());
            });
            services.AddQuartzServer(options =>
            {
                options.WaitForJobsToComplete = true;
            });
        }
    }
}
