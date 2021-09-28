using AutoMapper;
using BirthdayBot.DAL.Entities.GoogleTimeZone;
using RapidBots.Types.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
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
                return ClearYear(this.BirthDate.ToShortDateString()); ;
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
                return ClearYear(user.BirthDate.ToShortDateString());
            }
            else if(user.Settings.BirthYearConfidentiality == Core.Enums.ConfidentialType.Mutual)
            {
                bool amISubscribed = this.Subscriptions?.Any(x => x.TargetId == user.Id) ?? user.Subscribers?.Any(x => x.SubscriberId == this.Id) == true;
                bool isHeSubscribed = user.Subscriptions?.Any(x => x.TargetId == this.Id) ?? this.Subscribers?.Any(x => x.SubscriberId == user.Id) == true;
                if (amISubscribed && isHeSubscribed)
                {
                    return user.BirthDate.ToShortDateString();
                }
                else
                {
                    return ClearYear(user.BirthDate.ToShortDateString());
                }
            }
            else
            {
                return ClearYear(user.BirthDate.ToShortDateString());
            }
        }

        private string ClearYear(string date)
        {
            return Regex.Replace(date, @"\d\d\d\d", "xxxx");
        }
    }
}
