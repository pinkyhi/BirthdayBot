using System;
using System.ComponentModel.DataAnnotations;

namespace BirthdayBot.DAL.Entities
{
    public class Note
    {
        [Key]
        public long Id { get; set; }

        public string Title { get; set; }

        public DateTime Date { get; set; }

        public bool IsStrong { get; set; }

        public long UserId { get; set; }

        public TUser User { get; set; }

        public DateTime? LastNotificationTime { get; set; }
    }
}
