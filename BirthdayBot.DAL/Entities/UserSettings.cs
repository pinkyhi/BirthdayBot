using BirthdayBot.Core.Enums;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace BirthdayBot.DAL.Entities
{
    public class UserSettings
    {
        public ConfidentialType BirthYearConfidentiality { get; set; }

        public ConfidentialType BirthDateConfidentiality { get; set; }

        [Range(0, 28)]
        public int StrongNotification_0 { get; set; }

        [Range(0, 28)]
        public int StrongNotification_1 { get; set; }

        [Range(0, 28)]
        public int StrongNotification_2 { get; set; }

        [Range(0, 28)]
        public int DefaultNotificationDelay_0 { get; set; }

    }
}
