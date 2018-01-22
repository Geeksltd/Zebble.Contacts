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
            public static Task<IEnumerable<Contact>> GetAll(OnError errorAction = OnError.Alert) => Search(null);

            /// <summary>
            /// Returns the contacts in the device operating system which match the specified search query.        
            /// </summary>
            /// <param name="nameKeywords">If null or empty, all contacts will be returned.</param>
            public static async Task<IEnumerable<Contact>> Search(string nameKeywords, OnError errorAction = OnError.Alert)
            {
                try
                {
                    if (await Device.Permissions.Check(Device.Permission.Contacts) != PermissionResult.Granted)
                        if (await Device.Permissions.Request(Device.Permission.Contacts) != PermissionResult.Granted)
                        {
                            await errorAction.Apply("Permission for reading device contacts not granted");
                            return null;
                        }

                    return await DoReadContacts(nameKeywords);
                }
                catch (Exception ex)
                {
                    await errorAction.Apply(ex, "Failed to read contacts: " + ex.Message);
                    return null;
                }
            }
        }
    }
}