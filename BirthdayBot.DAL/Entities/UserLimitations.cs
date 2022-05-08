using System;

namespace BirthdayBot.DAL.Entities
{
    public class UserLimitations
    {
        public int StartLocationInputAttempts { get; set; }

        public DateTime? StartLocationInputBlockDate { get; set; }

        public int ChangeLocationInputAttempts { get; set; }

        public DateTime? ChangeLocationInputBlockDate { get; set; }
    }
}
