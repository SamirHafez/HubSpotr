using HubSpotr.Core.Model;
using Microsoft.WindowsAzure.MobileServices;
using System.Threading.Tasks;

namespace HubSpotr.Core.Extensions
{
    public static class PostExtensions
    {
        public static Task Post(this Post post)
        {
            IMobileServiceTable<Post> posts = AzureContext.Client.GetTable<Post>();

            return posts.InsertAsync(post);
        }
    }
}