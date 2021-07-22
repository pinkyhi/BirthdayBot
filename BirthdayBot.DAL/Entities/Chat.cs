using AutoMapper;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace BirthdayBot.DAL.Entities
{
    [AutoMap(typeof(Telegram.Bot.Types.Chat), ReverseMap = true)]
    public class Chat
    {
        public long Id { get; set; }

        public string Title { get; set; }

        public ChatType Type { get; set; }

        public string BigFileUniqueId { get; set; }

        public string SmallFileUniqueId { get; set; }

        public List<ChatMember> ChatMembers { get; set; }
    }
}
