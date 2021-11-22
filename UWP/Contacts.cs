namespace Zebble.Device
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Windows.ApplicationModel.Contacts;
    using Olive;

    partial class Contacts
    {
        static async Task<List<Contact>> DoReadContacts(ContactSearchParams searchParams)
        {
            var store = await ContactManager.RequestStoreAsync(ContactStoreAccessType.AllContactsReadOnly);

            IReadOnlyList<Windows.ApplicationModel.Contacts.Contact> contacts;

            if (searchParams.NameKeywords.HasValue())
                contacts = await store.FindContactsAsync(searchParams.NameKeywords);

            else contacts = await store.FindContactsAsync();

            return contacts.Select(x => Extract(x, searchParams)).ToList();
        }

        static Contact Extract(Windows.ApplicationModel.Contacts.Contact contact, ContactSearchParams searchParams)
        {
            var phones = contact.Phones.Select(p => new Contact.Phone
            {
                Number = p.Number,
                Type = p.Kind.ToString()
            }).ToList();

            var result = new Contact
            {
                Id = contact.Id,
                FirstName = contact.FirstName,
                MiddleName = contact.MiddleName,
                LastName = contact.LastName,
                DisplayName = contact.DisplayName,
                PhoneNumbers = phones,
                Emails = contact.Emails.Select(e => e.Address).ToList(),

                Prefix = searchParams.IncludePrefix ? contact.HonorificNamePrefix : null,
                Suffix = searchParams.IncludeSuffix ? contact.HonorificNameSuffix : null,
                Nickname = searchParams.IncludeNickName ? contact.Nickname : null,
                Notes = searchParams.IncludeNotes ? contact.Notes : null,
                Relationships = searchParams.IncludeRelationships ? contact.SignificantOthers.Select(r => r.Name).ToList() : null,
                Websites = searchParams.IncludeWebsites ? contact.Websites.Select(w => w.Uri?.ToString()).ToList() : null,
            };

            if(searchParams.IncludeOrganizations)
                result.Organizations = contact.JobInfo.Select(og => new Contact.Organization
                {
                    JobTitle = og.Title,
                    CompanyName = og.CompanyName
                }).ToList();

            if (searchParams.IncludeImAccounts)
                result.InstantMessagingAccounts = contact.ConnectedServiceAccounts.Select(ims => new Contact.InstantMessagingAccount
                {
                    Service = ims.ServiceName,
                    UserId = ims.Id
                }).ToList();

            if(searchParams.IncludeAddresses)
                result.Addresses = contact.Addresses.Select(ad => new Contact.Address
                {
                    City = ad.Locality,
                    Country = ad.Country,
                    PostalCode = ad.PostalCode,
                    Region = ad.Region,
                    StreetAddress = ad.StreetAddress
                }).ToList();

            return result;
        }
    }
}