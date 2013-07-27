using Android.App;
using Android.Content;
using Android.OS;
using HubSpotr.Core;
using Microsoft.WindowsAzure.MobileServices;
using Windows.Data.Json;

namespace HubSpotr.Android
{
    [Activity(Label = "HubSpotr", MainLauncher = true, NoHistory = true, Icon = "@drawable/HubSpot")]
    public class SplashActivity : Activity
    {
        protected async override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            SetContentView(Resource.Layout.Splash);

            ISharedPreferences userSettings = GetSharedPreferences("user", FileCreationMode.Private);

            if (!userSettings.Contains("token"))
                StartActivity(typeof(LoginActivity));

            string token = userSettings.GetString("token", string.Empty);
            var json = new JsonObject();
            json.Add("access_token", JsonValue.CreateStringValue(token));

            try
            {
                MobileServiceUser user = await AzureContext.Client.LoginAsync(this, MobileServiceAuthenticationProvider.Facebook, json);

                CommonData.User = user;
                StartActivity(typeof(DiscoveryActivity));
            }
            catch
            {
                new AlertDialog.Builder(this)
                               .SetTitle("Error")
                               .SetMessage("Authentication failed. Try to login again")
                               .SetCancelable(false)
                               .SetPositiveButton("OK", (o, a) => StartActivity(typeof(LoginActivity)))
                               .Show();
            }
        }
    }
}