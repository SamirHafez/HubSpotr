using HubSpotr.Core.Model;
using Microsoft.WindowsAzure.MobileServices;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HubSpotr.Core.Extensions
{
    public static class HubExtensions
    {
        private static MobileServiceClient mobileService = AzureContext.Client;

        public static Task Create(this Hub hub)
        {
            IMobileServiceTable<Hub> hubs = mobileService.GetTable<Hub>();

            return hubs.InsertAsync(hub);
        }

        public static Task<List<Post>> NewPosts(this Hub hub, DateTime latest)
        {
            IMobileServiceTable<Post> posts = mobileService.GetTable<Post>();

            return posts.Where(p => p.HubId == hub.Id && p.At > latest)
                        .OrderByDescending(p => p.At)
                        .ToListAsync();
        }

        public static Task<List<Post>> Posts(this Hub hub, int take = 10, int skip = 0)
        {
            IMobileServiceTable<Post> posts = mobileService.GetTable<Post>();

            return posts.Where(p => p.HubId == hub.Id)
                        .OrderByDescending(p => p.At)
                        .Skip(skip)
                        .Take(take)
                        .ToListAsync();
        }

        public static Task<List<Hub>> NearHubs(this Hub hub, int take = 5)
        {
            IMobileServiceTable<Hub> hubs = mobileService.GetTable<Hub>();

            var filter = string.Format("{0}, {1}, {2}", hub.Lat.ToString().Replace(',', '.'), hub.Lng.ToString().Replace(',', '.'), Convert.ToInt32(hub.Radius));
            return hubs.Where(p => p.Filter == filter)
                        .Take(take)
                        .ToListAsync();
        }

        public static Task Join(this Hub hub)
        {
            IMobileServiceTable<Hub> hubs = mobileService.GetTable<Hub>();

            hub.Participants++;

            return hubs.UpdateAsync(hub);
        }

        public static Task Leave(this Hub hub)
        {
            IMobileServiceTable<Hub> hubs = mobileService.GetTable<Hub>();

            hub.Participants--;

            return hubs.UpdateAsync(hub);
        }
    }
}