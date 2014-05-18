using HubSpotr.Core.Model;
using HubSpotr.Core.Extensions;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Net.NetworkInformation;
using Microsoft.WindowsAzure.MobileServices;
using System;
using System.ComponentModel;
using System.IO.IsolatedStorage;
using System.Windows;
using System.Windows.Navigation;
using System.Threading.Tasks;
using Facebook.Client;
using Facebook;

namespace HubSpotr.WindowsPhone
{
    public partial class MainPage : PhoneApplicationPage
    {
        private bool newLogin;

        public MainPage()
        {
            InitializeComponent();

            newLogin = false;
        }

        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (!DeviceNetworkInformation.IsNetworkAvailable)
            {
                MessageBox.Show("HubSpotr requires an internet connection", "Sorry", MessageBoxButton.OK);
                Application.Current.Terminate();
            }

            if(await GetCredentials())
                NavigationService.Navigate(new Uri("/DiscoveryPage.xaml", UriKind.Relative));
            else if(!newLogin)
            {
                pbLoading.Visibility = Visibility.Collapsed;
                bLoginF.Visibility = Visibility.Visible;
            }
        }

        protected override void OnBackKeyPress(CancelEventArgs e)
        {
            Application.Current.Terminate();
        }

        public async static Task<bool> GetCredentials()
        {
            IsolatedStorageSettings settings = IsolatedStorageSettings.ApplicationSettings;

            if (!settings.Contains("id") || !settings.Contains("token"))
                return false;

            var id = (string)settings["id"];
            var token = (string)settings["token"];

            App.MobileServiceClient.CurrentUser = new MobileServiceUser(id)
            {
                MobileServiceAuthenticationToken = token
            };

            var dbUser = await new User
            {
                Id = id
            }.Get();

            if (dbUser == null)
                return false;

            try
            {
                await App.FacebookSessionClient.LoginAsync("email,publish_stream,friends_about_me");
            }
            catch
            {
                return false;
            }

            App.User = new ViewModels.UserViewModel(dbUser);

            return true;
        }

        private async void NewLogin(object sender, EventArgs e)
        {
            pbLoading.Visibility = Visibility.Visible;
            bLoginF.Visibility = Visibility.Collapsed;
            newLogin = true;

            try
            {
                FacebookSession fbSession = await App.FacebookSessionClient.LoginAsync("email,publish_stream,friends_about_me");

                var token = Newtonsoft.Json.Linq.JObject.FromObject(new
                {
                    access_token = fbSession.AccessToken
                });

                MobileServiceUser user = await App.MobileServiceClient.LoginAsync(MobileServiceAuthenticationProvider.Facebook, token);

                var client = new FacebookClient(fbSession.AccessToken);
                dynamic me = await client.GetTaskAsync("me");

                var dbUser = await new User
                {
                    Id = user.UserId,
                    Name = me.name,
                    Email = me.email
                }.GetOrCreate();

                App.User = new ViewModels.UserViewModel(dbUser);

                Dispatcher.BeginInvoke(() =>
                {
                    IsolatedStorageSettings settings = IsolatedStorageSettings.ApplicationSettings;

                    settings.Add("id", user.UserId);
                    settings.Add("token", user.MobileServiceAuthenticationToken);
                    settings.Save();

                    NavigationService.Navigate(new Uri("/DiscoveryPage.xaml", UriKind.Relative));
                });
            }
            catch
            {
                MessageBox.Show("Login failed.", "Failed", MessageBoxButton.OK);

                pbLoading.Visibility = Visibility.Collapsed;
                bLoginF.Visibility = Visibility.Visible;
            }
        }
    }
}