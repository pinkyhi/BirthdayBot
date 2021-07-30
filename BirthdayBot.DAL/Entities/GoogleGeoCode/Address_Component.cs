using AutoMapper;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace BirthdayBot.DAL.Entities
{
    [AutoMap(typeof(RapidBots.GoogleGeoCode.Types.Address_Component), ReverseMap = true)]
    public class Address_Component
    {
        [Key]
        public long Id { get; set; }

        public string Long_Name { get; set; }

        public string Short_Name { get; set; }

        public string[] Types { get; set; }

        public List<Address_ComponentConnector> AddressComponentConnectors { get; set; }
    }
}
