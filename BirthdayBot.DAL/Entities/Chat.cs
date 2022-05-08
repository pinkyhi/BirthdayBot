using AutoMapper;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Telegram.Bot.Types.Enums;

namespace BirthdayBot.DAL.Entities
{
    [AutoMap(typeof(Telegram.Bot.Types.Chat), ReverseMap = true)]
    public class Chat
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public long Id { get; set; }

        public string Title { get; set; }

        public ChatType Type { get; set; }

        public int NotificationsCount { get; set; }

        public DateTime AddingDate { get; set; }

        public string BigFileUniqueId { get; set; }

        public string SmallFileUniqueId { get; set; }

        public List<ChatMember> ChatMembers { get; set; }
    }
}
