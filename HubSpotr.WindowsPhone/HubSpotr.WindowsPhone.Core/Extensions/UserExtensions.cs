using HubSpotr.Core;
using HubSpotr.WindowsPhone.Core.Model;
using Microsoft.WindowsAzure.MobileServices;
using System;
using System.Threading.Tasks;

namespace HubSpotr.WindowsPhone.Core.Extensions
{
    public static class UserExtensions
    {
        public async static Task<User> Create(this User user)
        {
            IMobileServiceTable<User> users = AzureContext.Client.GetTable<User>();

            await users.InsertAsync(user);

            return user;
        }

        public async static Task<User> Get(this User user)
        {
            IMobileServiceTable<User> users = AzureContext.Client.GetTable<User>();

            try
            {
                return await users.LookupAsync(user.Id);
            }
            catch
            {
                return null;
            }
        }

        public async static Task<User> GetOrCreate(this User user)
        {
            User dbUser = await Get(user);

            return dbUser ?? await Create(user);
        }
    }
}
