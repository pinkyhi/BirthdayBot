using Microsoft.Extensions.Logging;
using Quartz;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BirthdayBot.Quartz.Jobs
{
    public class PersonalBirthdayNotificationJob : IJob
    {
        private readonly ILogger<PersonalBirthdayNotificationJob> logger;

        public PersonalBirthdayNotificationJob(ILogger<PersonalBirthdayNotificationJob> logger)
        {
            this.logger = logger;
        }

        public Task Execute(IJobExecutionContext context)
        {
            logger.LogInformation("PersonalBirthdayNotificationJob");
            return new Task(() => { logger.LogInformation("PersonalBirthdayNotificationJob"); });
        }
    }
}
