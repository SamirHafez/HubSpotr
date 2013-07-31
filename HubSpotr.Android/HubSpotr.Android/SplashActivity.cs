using Android.App;
using Android.Content;
using Android.Net;
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
        }

        protected override void OnResume()
        {
            base.OnResume();
            CheckRequirements();

            if (LoginExisting())
                StartActivity(typeof(DiscoveryActivity));
            else
            {
                Button facebookLoginButton = FindViewById<Button>(Resource.Id.button_LoginFace);
                ProgressBar progressBar = FindViewById<ProgressBar>(Resource.Id.splashProgress);

                progressBar.Visibility = ViewStates.Invisible;
                facebookLoginButton.Visibility = ViewStates.Visible;
            }
        }


        private bool CheckRequirements()
        {
            var conManager = (ConnectivityManager)GetSystemService(Context.ConnectivityService);

            if (!conManager.GetNetworkInfo(ConnectivityType.Mobile).IsConnected &&
                !conManager.GetNetworkInfo(ConnectivityType.Wifi).IsConnected)
            {
                new AlertDialog.Builder(this)
                               .SetTitle("Sorry")
                               .SetMessage("HubSpotr requires an internet connection to function")
                               .SetCancelable(false)
                               .SetPositiveButton("OK", (o, a) => Finish())
                               .Show();
                return false;
            }

            return true;
        }

        private bool LoginExisting()
        {
            ISharedPreferences userSettings = GetSharedPreferences("user", FileCreationMode.Private);

            if (!userSettings.Contains("token"))
                return false;

            string id = userSettings.GetString("id", string.Empty);
            string token = userSettings.GetString("token", string.Empty);

            AzureContext.Client.CurrentUser = new MobileServiceUser(id)
            {
                MobileServiceAuthenticationToken = token
            };

            return true;
        }

        private async void NewLogin(object sender, System.EventArgs e)
        {
            try
            {
                MobileServiceUser user = await AzureContext.Client.LoginAsync(this, MobileServiceAuthenticationProvider.Facebook);

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