namespace Zebble.Device
{
    using AddressBook;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    partial class Contacts
    {
        static ABAddressBook Addressbook = new ABAddressBook();

        static Task<List<Contact>> DoReadContacts(string searchText)
        {
            var people = searchText.HasValue() ? Addressbook.GetPeopleWithName(searchText) : Addressbook.GetPeople();

            return Task.FromResult(people.Select(Extract).ToList());
        }

        static Contact Extract(ABPerson data)
        {
            var org = new List<Contact.Organization> {
                    new Contact.Organization{
                        CompanyName = data.Organization,
                        JobTitle = data.JobTitle
                    }
                };

            var addresses = data.GetAllAddresses().Select(ad => new Contact.Address
            {
                City = ad.Value.City,
                Country = ad.Value.Country,
                PostalCode = ad.Value.Zip,
                Region = ad.Value.State,
                StreetAddress = ad.Value.Street
            }).ToList();

            var imAccounts = data.GetInstantMessageServices().Select(im => new Contact.InstantMessagingAccount
            {
                Service = im.Value.ServiceName,
                UserId = im.Value.Username
            }).ToList();

            var contact = new Contact
            {
                Id = data.Id.ToString(),
                FirstName = data.FirstName,
                MiddleName = data.MiddleName,
                LastName = data.LastName,
                DisplayName = data.ToString(),

                Prefix = data.Prefix,
                Suffix = data.Suffix,
                Nickname = data.Nickname,
                Emails = data.GetEmails().Select(e => e.Value).Trim().ToList(),
                Notes = data.Note,
                Organizations = org,
                PhoneNumbers = data.GetPhones().Select(ph => new Contact.Phone { Number = ph.Value }).ToList(),
                RelationShips = data.GetRelatedNames().Select(r => r.Value).ToList(),
                WebSites = data.GetUrls().Select(w => w.Value).ToList(),

                InstantMessagingAccounts = imAccounts,
                Addresses = addresses
            };

            if (data.HasImage)
            {
                contact.PhotoData = data.GetImage(ABPersonImageFormat.OriginalSize).ToArray();
                contact.PhotoDataThumbnail = data.GetImage(ABPersonImageFormat.Thumbnail).ToArray();
            }

            return contact;
        }
    }
}