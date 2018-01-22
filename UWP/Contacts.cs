namespace Zebble.Device
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Windows.ApplicationModel.Contacts;

    partial class Contacts
    {
        static async Task<List<Contact>> DoReadContacts(string searchText)
        {
            var store = await ContactManager.RequestStoreAsync(ContactStoreAccessType.AllContactsReadOnly);

            IReadOnlyList<Windows.ApplicationModel.Contacts.Contact> contacts;

            if (searchText.HasValue())
                contacts = await store.FindContactsAsync(searchText);
            else contacts = await store.FindContactsAsync();

            return contacts.Select(Extract).ToList();
        }

        static Contact Extract(Windows.ApplicationModel.Contacts.Contact contact)
        {
            var org = contact.JobInfo.Select(og => new Contact.Organization
            {
                JobTitle = og.Title,
                CompanyName = og.CompanyName
            }).ToList();

            var phones = contact.Phones.Select(p => new Contact.Phone
            {
                Number = p.Number,
                Type = p.Kind.ToString()
            }).ToList();

            var postalAddresses = contact.Addresses.Select(ad => new Contact.Address
            {
                City = ad.Locality,
                Country = ad.Country,
                PostalCode = ad.PostalCode,
                Region = ad.Region,
                StreetAddress = ad.StreetAddress
            });

            var im = contact.ConnectedServiceAccounts.Select(ims => new Contact.InstantMessagingAccount
            {
                Service = ims.ServiceName,
                UserId = ims.Id
            });

            return new Contact
            {
                DisplayName = contact.DisplayName,
                FirstName = contact.FirstName,
                Emails = contact.Emails.Select(e => e.Address).ToList(),
                Id = contact.Id,
                LastName = contact.LastName,
                MiddleName = contact.MiddleName,
                Nickname = contact.Nickname,
                Notes = contact.Notes,
                PhoneNumbers = phones,

                Prefix = contact.HonorificNamePrefix,
                Suffix = contact.HonorificNameSuffix,
                WebSites = contact.Websites.Select(w => w.Uri?.ToString()).ToList(),
                Addresses = postalAddresses.ToList(),
                InstantMessagingAccounts = im.ToList(),
                Organizations = org,

                RelationShips = contact.SignificantOthers.Select(r => r.Name).ToList()
            };
        }
    }
}