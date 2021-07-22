using System;
using System.Collections.Generic;
using System.Text;

namespace BirthdayBot.DAL.Entities
{
    public class Note
    {
        public long Id { get; set; }

        public string Title { get; set; }

        public DateTime Date { get; set; }

        public bool IsStrong { get; set; }

        public long UserId { get; set; }

        public TUser User { get; set; }
    }
}
