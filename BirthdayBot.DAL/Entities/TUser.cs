using AutoMapper;
using BirthdayBot.DAL.Entities.GoogleTimeZone;
using RapidBots.Types.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using Telegram.Bot.Types;

namespace BirthdayBot.DAL.Entities
{
    [AutoMap(typeof(User), ReverseMap = true)]
    public class TUser : TelegramUser
    {
        public DateTime? RegistrationDate { get; set; }

        public DateTime BirthDate { get; set; }

        public UserSettings Settings { get; set; }

        public UserTimezone Timezone { get; set; }

        public UserLimitations Limitations { get; set; }

        public List<Address> Addresses { get; set; }

        public List<ChatMember> ChatMembers { get; set; }

        public List<Note> Notes { get; set; }

        public List<Subscription> Subscribers { get; set; }

        public List<Subscription> Subscriptions { get; set; }

        public string GetConfidentialDateString()
        {
            if (this.Settings.BirthYearConfidentiality == Core.Enums.ConfidentialType.Public)
            {
                return this.BirthDate.ToShortDateString();
            }
            else
            {
                return this.BirthDate.AddYears((this.BirthDate.Year * -1) + 1).ToShortDateString();
            }
        }

        public string GetAnotherUserDateString(TUser user)
        {
            if(user.Settings.BirthYearConfidentiality == Core.Enums.ConfidentialType.Public)
            {
                return user.BirthDate.ToShortDateString();
            }
            else if(user.Settings.BirthYearConfidentiality == Core.Enums.ConfidentialType.Private)
            {
                return user.BirthDate.AddYears((user.BirthDate.Year * -1) + 1).ToShortDateString();
            }
            else if(user.Settings.BirthYearConfidentiality == Core.Enums.ConfidentialType.Mutual)
            {
                if (this.Subscribers?.Any(x => x.SubscriberId == user.Id) == true && this.Subscriptions?.Any(x => x.TargetId == user.Id) == true)
                {
                    return user.BirthDate.ToShortDateString();
                }
                else
                {
                    return user.BirthDate.AddYears((user.BirthDate.Year * -1) + 1).ToShortDateString();
                }
            }
            else
            {
                return user.BirthDate.AddYears((user.BirthDate.Year * -1) + 1).ToShortDateString();
            }
        }
    }
}
