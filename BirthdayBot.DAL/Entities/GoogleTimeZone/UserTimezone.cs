using AutoMapper;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace BirthdayBot.DAL.Entities.GoogleTimeZone
{
    [AutoMap(typeof(RapidBots.GoogleGeoCode.Types.TimeZoneResponse), ReverseMap = true)]
    [Owned]
    public class UserTimezone
    {
        public long DstOffset { get; set; }

        public long RawOffset { get; set; }

        public string TimeZoneId { get; set; }

        public string TimeZoneName { get; set; }
    }
}
