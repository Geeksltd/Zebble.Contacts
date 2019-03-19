namespace Zebble.Device
{
    using System.Collections.Generic;

    public class Contact
    {
        public string Id { get; set; }

        public bool IsAggregate { get; internal set; }
        public string Prefix { get; set; }
        public string Suffix { get; set; }
        public string Notes { get; set; } = string.Empty;

        public string FirstName { get; set; }
        public string MiddleName { get; set; }
        public string LastName { get; set; }
        public string DisplayName { get; set; }
        public string Nickname { get; set; }
        public string PhotoUri { get; set; }
        public string PhotoUriThumbnail { get; set; }
        public byte[] PhotoData { get; set; }
        public byte[] PhotoDataThumbnail { get; set; }

        public List<Phone> PhoneNumbers { get; internal set; } = new List<Phone>();
        public List<Organization> Organizations { get; internal set; } = new List<Organization>();
        public List<string> Emails { get; internal set; } = new List<string>();
        public List<string> WebSites { get; internal set; } = new List<string>();
        public List<string> RelationShips { get; internal set; } = new List<string>();
        public List<Address> Addresses { get; internal set; } = new List<Address>();
        public List<InstantMessagingAccount> InstantMessagingAccounts { get; internal set; } = new List<InstantMessagingAccount>();
        public object Tag { get; internal set; }

        public class Phone
        {
            public string Number { get; set; }
            public string Type { get; set; }
        }

        public class Organization
        {
            public string JobTitle { get; set; }
            public string CompanyName { get; set; }
        }

        public class Address
        {
            public string StreetAddress { get; set; }
            public string City { get; set; }
            public string Region { get; set; }
            public string Country { get; set; }
            public string PostalCode { get; set; }
        }

        public class InstantMessagingAccount
        {
            public string Service { get; set; }
            public string UserId { get; set; }
        }
    }
}
