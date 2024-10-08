﻿using AutoMapper;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace BirthdayBot.DAL.Entities
{
    [AutoMap(typeof(RapidBots.GoogleGeoCode.Types.Address), ReverseMap = true)]
    public class Address
    {
        [Key]
        public long Id { get; set; }

        public long UserId { get; set; }

        public TUser User { get; set; }

        public string Formatted_Address { get; set; }

        public string[] Types { get; set; }

        public List<Address_ComponentConnector> AddressComponentConnectors { get; set; }
    }
}
