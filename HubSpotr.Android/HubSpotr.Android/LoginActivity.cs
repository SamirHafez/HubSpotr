using Android.App;
using Android.Content;
using Android.OS;
using Android.Widget;
using HubSpotr.Core;
using Microsoft.WindowsAzure.MobileServices;
using System;

namespace HubSpotr.Android
{
    [Activity(Label = "HubSpotr", Icon = "@drawable/HubSpot", NoHistory = true)]
    public class LoginActivity : Activity
    {
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            SetContentView(Resource.Layout.Login);

            Button facebookLoginButton = FindViewById<Button>(Resource.Id.facebookLoginButton);

            facebookLoginButton.Click += TryLogin;
        }

        private async void TryLogin(object sender, EventArgs e)
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
                           .SetMessage("You must authenticate to use HubSpot")
                           .SetPositiveButton("Try again", (o, a) => { })
                           .SetNegativeButton("Cancel", (o, a) => this.Finish())
                           .Show();
            }
        }
    }
}