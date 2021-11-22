namespace Zebble
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    namespace Device
    {
        public partial class Contacts
        {
            /// <summary>
            /// Returns all the contacts in the device operating system.
            /// </summary>
            public static Task<IEnumerable<Contact>> GetAll(
                OnError errorAction = OnError.Alert)
                => Search(new ContactSearchParams(), errorAction);

            /// <summary>
            /// Returns the contacts in the device operating system which match the specified search query.        
            /// </summary>
            public static async Task<IEnumerable<Contact>> Search(ContactSearchParams searchParams, OnError errorAction = OnError.Alert)
            {
                try
                {
                    if (await Permissions.Check(Permission.Contacts) != PermissionResult.Granted)
                        if (await Permissions.Request(Permission.Contacts) != PermissionResult.Granted)
                        {
                            await errorAction.Apply("Permission for reading device contacts not granted");
                            return null;
                        }

                    return await DoReadContacts(searchParams);
                }
                catch (Exception ex)
                {
                    await errorAction.Apply(ex, "Failed to read contacts: " + ex.Message);
                    return null;
                }
            }
        }
    }

    public class ContactSearchParams
    {
        public string NameKeywords;
        public bool IncludePrefix;
        public bool IncludeSuffix;
        public bool IncludeNickName;
        public bool IncludeNotes;
        public bool IncludeOrganizations;
        public bool IncludeRelationships;
        public bool IncludeWebsites;
        public bool IncludeImAccounts;
        public bool IncludeAddresses;
        public bool IncludeImage;
    }
}