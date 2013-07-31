using Android.App;
using Android.Content;
using Android.OS;
using Android.Views;
using Android.Widget;
using HubSpotr.Core;
using Microsoft.WindowsAzure.MobileServices;
using System;
using System.Threading.Tasks;
using Windows.Data.Json;

namespace HubSpotr.Android
{
    [Activity(Label = "HubSpotr", MainLauncher = true, NoHistory = true, Icon = "@drawable/ic_launcher")]
    public class SplashActivity : Activity
    {
        protected async override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            this.RequestWindowFeature(WindowFeatures.NoTitle);

            SetContentView(Resource.Layout.Splash);

            Button facebookLoginButton = FindViewById<Button>(Resource.Id.button_LoginFace);
            ProgressBar progressBar = FindViewById<ProgressBar>(Resource.Id.splashProgress);

            progressBar.Visibility = ViewStates.Visible;

            facebookLoginButton.Visibility = ViewStates.Invisible;
            facebookLoginButton.Click += NewLogin;

            if (!await LoginExisting())
            {
                progressBar.Visibility = ViewStates.Invisible;
                facebookLoginButton.Visibility = ViewStates.Visible;
            }
        }

        private async Task<bool> LoginExisting()
        {
            ISharedPreferences userSettings = GetSharedPreferences("user", FileCreationMode.Private);

            if (!userSettings.Contains("token"))
                return false;

            string token = userSettings.GetString("token", string.Empty);
            var json = new JsonObject();
            json.Add("access_token", JsonValue.CreateStringValue(token));

            try
            {
                MobileServiceUser user = await AzureContext.Client.LoginAsync(this, MobileServiceAuthenticationProvider.Facebook, json);

                CommonData.User = user;
                StartActivity(typeof(DiscoveryActivity));

                return true;
            }
            catch(Exception e)
            {
                new AlertDialog.Builder(this)
                               .SetTitle("Error")
                               .SetMessage("Authentication failed. Try to login again")
                               .SetCancelable(false)
                               .SetPositiveButton("OK", (o, a) => { })
                               .Show();

                return false;
            }
        }

        private async void NewLogin(object sender, System.EventArgs e)
        {
            try
            {
                MobileServiceUser user = await AzureContext.Client.LoginAsync(this, MobileServiceAuthenticationProvider.Facebook);

                CommonData.User = user;

                ISharedPreferences userSettings = GetSharedPreferences("user", FileCreationMode.Private);

                ISharedPreferencesEditor editor = userSettings.Edit();
                editor.PutString("id", user.UserId);
                editor.PutString("token", user.MobileServiceAuthenticationToken);
                editor.Commit();

                StartActivity(typeof(DiscoveryActivity));
            }
            catch
            {
                new AlertDialog.Builder(this)
                               .SetTitle("Failed")
                               .SetMessage("You must authenticate to use HubSpotr")
                               .SetCancelable(false)
                               .SetPositiveButton("OK", (o, a) => { })
                               .Show();
            }
        }
    }
}