using HubSpotr.Core.Model;
using Microsoft.WindowsAzure.MobileServices;
using System.Threading.Tasks;

namespace HubSpotr.Core.Extensions
{
    public static class PostExtensions
    {
        private static MobileServiceClient mobileService = AzureContext.Client;

        public static Task Post(this Post post)
        {
            IMobileServiceTable<Post> posts = mobileService.GetTable<Post>();

            return posts.InsertAsync(post);
        }
    }
}