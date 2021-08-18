using AutoMapper;
using BirthdayBot.DAL.Entities.GoogleTimeZone;
using RapidBots.Types.Core;
using System;
using System.Collections.Generic;
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
    }
}
