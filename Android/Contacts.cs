namespace Zebble.Device
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using CommonColumns = Android.Provider.ContactsContract.CommonDataKinds.CommonColumns;
    using InstantMessaging = Android.Provider.ContactsContract.CommonDataKinds.Im;
    using OrganizationData = Android.Provider.ContactsContract.CommonDataKinds.Organization;
    using Relation = Android.Provider.ContactsContract.CommonDataKinds.Relation;
    using StructuredName = Android.Provider.ContactsContract.CommonDataKinds.StructuredName;
    using StructuredPostal = Android.Provider.ContactsContract.CommonDataKinds.StructuredPostal;
    using WebsiteData = Android.Provider.ContactsContract.CommonDataKinds.Website;

    partial class Contacts
    {
        static ContentResolver ContentResolver => UIRuntime.CurrentActivity.ContentResolver;
        const int BATCH_SIZE = 256;

        static Task<List<Contact>> DoReadContacts(string searchText = "")
        {
            if (searchText.HasValue())
            {
                var ids = new string[1];
                ids[0] = searchText;
                return Task.FromResult(SelectContacts(ContentResolver, ids).ToList());
            }
            else
                return Task.FromResult(SelectContacts(ContentResolver).ToList());
        }

        static IEnumerable<Contact> SelectContacts(ContentResolver content)
        {
            var uri = ContactsContract.Contacts.ContentUri;

            ICursor cursor = null;
            try
            {
                cursor = content.Query(uri, null, null, null, null);
                if (cursor == null) yield break;
                foreach (var contact in SelectContacts(cursor, content, BATCH_SIZE))
                    yield return contact;
            }
            finally
            {
                cursor?.Close();
            }
        }

        static IEnumerable<Contact> SelectContacts(ICursor cursor, ContentResolver content, int batchSize)
        {
            if (cursor == null) yield break;

            var column = ContactsContract.ContactsColumns.LookupKey;

            var ids = new string[batchSize];
            var columnIndex = cursor.GetColumnIndex(column);

            var uniques = new HashSet<string>();

            var counter = 0;
            while (cursor.MoveToNext())
            {
                if (counter == batchSize)
                {
                    counter = 0;
                    foreach (var c in SelectContacts(content, ids)) yield return c;
                }

                var id = cursor.GetString(columnIndex);
                if (id == null || uniques.Contains(id)) continue;

                uniques.Add(id);
                ids[counter++] = id;
            }

            if (counter > 0)
            {
                foreach (var c in SelectContacts(content, ids.Take(counter).ToArray()))
                    yield return c;
            }
        }

        static IEnumerable<Contact> SelectContacts(ContentResolver content, string[] ids)
        {
            ICursor cursor = null;

            var column = ContactsContract.ContactsColumns.LookupKey;

            var whereb = new StringBuilder();
            for (var i = 0; i < ids.Length; i++)
            {
                if (i > 0) whereb.Append(" OR ");

                whereb.Append(column);
                whereb.Append("=?");
            }

            var xCounter = 0;
            var map = new Dictionary<string, Contact>(ids.Length);

            try
            {
                Contact currentContact = null;

                cursor = content.Query(ContactsContract.Data.ContentUri, null, whereb.ToString(), ids, ContactsContract.ContactsColumns.LookupKey);
                if (cursor == null) yield break;

                var idIndex = cursor.GetColumnIndex(column);
                var dnIndex = cursor.GetColumnIndex(ContactsContract.ContactsColumns.DisplayName);
                while (cursor.MoveToNext())
                {
                    var id = cursor.GetString(idIndex);
                    if (currentContact == null || currentContact.Id != id)
                    {
                        // We need to yield these in the original ID order
                        if (currentContact != null)
                        {
                            if (currentContact.Id == ids[xCounter])
                            {
                                yield return currentContact;
                                xCounter++;
                            }
                            else
                                map.Add(currentContact.Id, currentContact);
                        }

                        currentContact = new Contact
                        {
                            Id = id,
                            IsAggregate = true,
                            Tag = content,
                            DisplayName = cursor.GetString(dnIndex),
                            PhotoUri = GetString(cursor, ContactsContract.Contacts.InterfaceConsts.PhotoUri),
                            PhotoUriThumbnail = GetString(cursor, ContactsContract.Contacts.InterfaceConsts.PhotoThumbnailUri)
                        };
                    }

                    FillContactWithRow(currentContact, cursor);
                }

                if (currentContact != null)
                    map.Add(currentContact.Id, currentContact);

                for (; xCounter < ids.Length; xCounter++)
                {
                    if (map.TryGetValue(ids[xCounter], out Contact tContact))
                        yield return tContact;
                }
            }
            finally
            {
                cursor?.Close();
            }
        }

        static Contact SelectContacts(ContentResolver content, ICursor cursor)
        {
            var column = ContactsContract.ContactsColumns.LookupKey;

            var id = cursor.GetString(cursor.GetColumnIndex(column));

            var contact = new Contact
            {
                Id = id,
                IsAggregate = true,
                Tag = content,
                DisplayName = GetString(cursor, ContactsContract.ContactsColumns.DisplayName)
            };

            FillContactExtras(content, id, contact);

            return contact;
        }

        static void FillContactExtras(ContentResolver content, string recordId, Contact contact)
        {
            if (recordId.LacksValue()) return;

            var column = ContactsContract.ContactsColumns.LookupKey;

            ICursor cursor = null;
            try
            {
                cursor = content.Query(ContactsContract.Data.ContentUri, null, column + " = ?", new[] { recordId }, null);
                if (cursor == null) return;
                while (cursor.MoveToNext()) FillContactWithRow(contact, cursor);
            }
            finally
            {
                cursor?.Close();
            }
        }

        static void FillContactWithRow(Contact contact, ICursor cursor)
        {
            var dataType = cursor.GetString(cursor.GetColumnIndex(ContactsContract.DataColumns.Mimetype));
            switch (dataType)
            {
                case ContactsContract.CommonDataKinds.Nickname.ContentItemType:
                    contact.Nickname = cursor.GetString(cursor.GetColumnIndex(ContactsContract.CommonDataKinds.Nickname.Name));
                    break;

                case StructuredName.ContentItemType:
                    contact.FirstName = cursor.GetString(cursor.GetColumnIndex(StructuredName.GivenName));
                    contact.MiddleName = cursor.GetString(cursor.GetColumnIndex(StructuredName.MiddleName));
                    contact.LastName = cursor.GetString(cursor.GetColumnIndex(StructuredName.FamilyName));
                    contact.Suffix = cursor.GetString(cursor.GetColumnIndex(StructuredName.Suffix));
                    contact.Prefix = cursor.GetString(cursor.GetColumnIndex(StructuredName.Prefix));
                    break;

                case ContactsContract.CommonDataKinds.Phone.ContentItemType:
                    contact.PhoneNumbers.Add(GetPhone(cursor));
                    break;

                case ContactsContract.CommonDataKinds.Email.ContentItemType:
                    contact.Emails.Add(GetEmail(cursor));
                    break;

                case ContactsContract.CommonDataKinds.Note.ContentItemType:
                    contact.Notes = GetNote(cursor);
                    break;

                case ContactsContract.CommonDataKinds.Organization.ContentItemType:
                    contact.Organizations.Add(GetOrganization(cursor));
                    break;

                case StructuredPostal.ContentItemType:
                    contact.Addresses.Add(GetAddress(cursor));
                    break;

                case InstantMessaging.ContentItemType:
                    contact.InstantMessagingAccounts.Add(GetImAccount(cursor));
                    break;

                case WebsiteData.ContentItemType:
                    contact.WebSites.Add(GetWebsite(cursor));
                    break;

                case Relation.ContentItemType:
                    contact.RelationShips.Add(GetRelationship(cursor));
                    break;
                default:
                    break;
            }
        }

        internal static string GetNote(ICursor cursor) => GetString(cursor, ContactsContract.DataColumns.Data1);

        internal static string GetRelationship(ICursor cursor) => cursor.GetString(cursor.GetColumnIndex(Relation.Name));

        internal static Contact.InstantMessagingAccount GetImAccount(ICursor cursor)
        {
            return new Contact.InstantMessagingAccount
            {
                Service = GetString(cursor, ContactsContract.RawContacts.InterfaceConsts.AccountType),
                UserId = GetString(cursor, ContactsContract.RawContacts.InterfaceConsts.AccountName)
            };
        }

        internal static Contact.Address GetAddress(ICursor cursor)
        {
            var result = new Contact.Address
            {
                Country = cursor.GetString(cursor.GetColumnIndex(StructuredPostal.Country)),
                Region = cursor.GetString(cursor.GetColumnIndex(StructuredPostal.Region)),
                City = cursor.GetString(cursor.GetColumnIndex(StructuredPostal.City)),
                PostalCode = cursor.GetString(cursor.GetColumnIndex(StructuredPostal.Postcode))
            };

            var kind = (AddressDataKind)cursor.GetInt(cursor.GetColumnIndex(CommonColumns.Type));
            var street = cursor.GetString(cursor.GetColumnIndex(StructuredPostal.Street));
            var pobox = cursor.GetString(cursor.GetColumnIndex(StructuredPostal.Pobox));
            if (street != null)
                result.StreetAddress = street;
            if (pobox != null)
            {
                if (street != null)
                    result.StreetAddress += Environment.NewLine;

                result.StreetAddress += pobox;
            }

            return result;
        }

        internal static Contact.Phone GetPhone(ICursor cursor)
        {
            return new Contact.Phone
            {
                Number = GetString(cursor, ContactsContract.CommonDataKinds.Phone.Number),
                Type = ((PhoneDataKind)cursor.GetInt(cursor.GetColumnIndex(CommonColumns.Type))).ToString()
            };
        }

        internal static string GetEmail(ICursor cursor) => cursor.GetString(cursor.GetColumnIndex(ContactsContract.DataColumns.Data1));

        internal static Contact.Organization GetOrganization(ICursor cursor)
        {
            return new Contact.Organization
            {
                CompanyName = cursor.GetString(cursor.GetColumnIndex(OrganizationData.Company)),
                JobTitle = cursor.GetString(cursor.GetColumnIndex(OrganizationData.Title))
            };
        }

        internal static string GetWebsite(ICursor cursor) => cursor.GetString(cursor.GetColumnIndex(WebsiteData.Url));

        internal static string GetString(ICursor cursor, string colName)
        {
            return cursor.GetString(cursor.GetColumnIndex(colName));
        }
    }
}