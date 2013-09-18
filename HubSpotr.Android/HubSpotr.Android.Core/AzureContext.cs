using Microsoft.WindowsAzure.MobileServices;

namespace HubSpotr.Core
{
    public static class AzureContext
    {
        private static MobileServiceClient client;

        public static MobileServiceClient Client 
        {
            get
            {
                return client ?? (client = new MobileServiceClient("https://hubspot.azure-mobile.net/", "LGxVOIjsExmFuFbBRRSrkmGHXEosZP55"));
            }
        }
    }
}