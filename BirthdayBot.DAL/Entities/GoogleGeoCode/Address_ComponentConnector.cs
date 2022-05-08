namespace BirthdayBot.DAL.Entities
{
    public class Address_ComponentConnector
    {
        public long AddressId { get; set; }

        public Address Address { get; set; }

        public long AddressComponentId { get; set; }

        public Address_Component AddressComponent { get; set; }
    }
}
