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
                return client ?? (client = new MobileServiceClient("http://hubspot.azure-mobile.net/", "LGxVOIjsExmFuFbBRRSrkmGHXEosZP55"));
            }
        }
    }
}