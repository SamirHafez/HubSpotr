using HubSpotr.Core.Model;
using Microsoft.WindowsAzure.MobileServices;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HubSpotr.Core.Extensions
{
    public static class HubExtensions
    {
        public static Task Create(this Hub hub)
        {
            IMobileServiceTable<Hub> hubs = AzureContext.Client.GetTable<Hub>();

            return hubs.InsertAsync(hub);
        }

        public static Task<List<Post>> NewPosts(this Hub hub, DateTime latest)
        {
            IMobileServiceTable<Post> posts = AzureContext.Client.GetTable<Post>();

            return posts.Where(p => p.HubId == hub.Id && p.At > latest)
                        .OrderByDescending(p => p.At)
                        .ToListAsync();
        }

        public static Task<List<Post>> Posts(this Hub hub, int take = 10, int skip = 0)
        {
            IMobileServiceTable<Post> posts = AzureContext.Client.GetTable<Post>();

            return posts.Where(p => p.HubId == hub.Id)
                        .OrderByDescending(p => p.At)
                        .Skip(skip)
                        .Take(take)
                        .ToListAsync();
        }

        public static Task<List<Hub>> NearHubs(this Hub hub, int take = 5)
        {
            IMobileServiceTable<Hub> hubs = AzureContext.Client.GetTable<Hub>();

            string lat = hub.Lat.ToString().Replace(',', '.');
            string lng = hub.Lng.ToString().Replace(',', '.');
            string filter = string.Format("{0}, {1}", lat, lng);

            return hubs.Where(h => h.Filter == filter)
                       .OrderByDescending(h => h.Participants)
                       .Take(take)
                       .ToListAsync();
        }

        public static Task Join(this Hub hub)
        {
            IMobileServiceTable<Hub> hubs = AzureContext.Client.GetTable<Hub>();

            hub.Participants++;

            return hubs.UpdateAsync(hub);
        }

        public static Task Leave(this Hub hub)
        {
            IMobileServiceTable<Hub> hubs = AzureContext.Client.GetTable<Hub>();

            hub.Participants--;

            return hubs.UpdateAsync(hub);
        }
    }
}