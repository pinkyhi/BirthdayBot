﻿using AutoMapper;
using System;
using Telegram.Bot.Types.Enums;

namespace BirthdayBot.DAL.Entities
{
    [AutoMap(typeof(Telegram.Bot.Types.ChatMember), ReverseMap = true)]
    public class ChatMember
    {
        public long UserId { get; set; }

        public TUser User { get; set; }

        public long ChatId { get; set; }

        public Chat Chat { get; set; }

        public DateTime AddingDate { get; set; }
    }
}
