using System;
using System.Collections.Generic;
using System.Text;

namespace BirthdayBot.DAL.Entities
{
    public class UserLimitations
    {
        public int StartLocationInputAttempts { get; set; }

        public DateTime? StartLocationInputBlockDate { get; set; }

        public int LocationChangeAttempts { get; set; }

        public DateTime? LocationChangeBlockDate { get; set; }
    }
}
