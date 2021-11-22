namespace Zebble.Device
{
    using AddressBook;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Olive;
    using System;

    partial class Contacts
    {
        static ABAddressBook Addressbook = new ABAddressBook();

        static Task<List<Contact>> DoReadContacts(ContactSearchParams searchParams)
        {
            var people = searchParams.NameKeywords.HasValue() ? Addressbook.GetPeopleWithName(searchParams.NameKeywords) : Addressbook.GetPeople();

            return Task.FromResult(people.Select(x => Extract(x, searchParams)).ToList());
        }

        static Contact Extract(ABPerson data, ContactSearchParams searchParams)
        {
            var result = new Contact
            {
                Id = data.Id.ToString(),
                FirstName = data.FirstName,
                MiddleName = data.MiddleName,
                LastName = data.LastName,
                DisplayName = data.ToString(),
                PhoneNumbers = data.GetPhones().Select(ph => new Contact.Phone { Number = ph.Value }).ToList(),
                Emails = data.GetEmails().Select(e => e.Value).Trim().ToList(),

                Prefix = searchParams.IncludePrefix ? data.Prefix : null,
                Suffix = searchParams.IncludeSuffix ? data.Suffix : null,
                Nickname = searchParams.IncludeNickName ? data.Nickname : null,
                Notes = searchParams.IncludeNotes ? data.Note : null,
                Relationships = searchParams.IncludeRelationships ? data.GetRelatedNames().Select(r => r.Value).ToList() : null,
                Websites = searchParams.IncludeWebsites ? data.GetUrls().Select(w => w.Value).ToList() : null
            };

            if (searchParams.IncludeOrganizations)
                result.Organizations = new List<Contact.Organization> {
                    new Contact.Organization{
                        CompanyName = data.Organization,
                        JobTitle = data.JobTitle
                    }
                };

            if (searchParams.IncludeImAccounts)
                result.InstantMessagingAccounts = data.GetInstantMessageServices().Select(im => new Contact.InstantMessagingAccount
                {
                    Service = im.Value.ServiceName,
                    UserId = im.Value.Username
                }).ToList();

            if (searchParams.IncludeAddresses)
                result.Addresses = data.GetAllAddresses().Select(ad => new Contact.Address
                {
                    City = ad.Value.City,
                    Country = ad.Value.Country,
                    PostalCode = ad.Value.Zip,
                    Region = ad.Value.State,
                    StreetAddress = ad.Value.Street
                }).ToList();

            try
            {
                if (searchParams.IncludeImage && data.HasImage)
                {
                    result.PhotoData = data.GetImage(ABPersonImageFormat.OriginalSize).ToArray();
                    result.PhotoDataThumbnail = data.GetImage(ABPersonImageFormat.Thumbnail).ToArray();
                }
            }
            catch { }

            return result;
        }
    }
}