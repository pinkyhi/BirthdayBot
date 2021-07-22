using System;
using System.Collections.Generic;
using System.Text;

namespace BirthdayBot.DAL.Entities
{
    public class Subscription
    {
        public long SubscriberId { get; set; }

        public long TargetId { get; set; }

        public TUser Subscriber { get; set; }

        public TUser Target { get; set; }

        public bool IsStrong { get; set; }
    }
}
