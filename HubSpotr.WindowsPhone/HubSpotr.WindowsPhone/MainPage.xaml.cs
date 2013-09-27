using HubSpotr.Core;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Net.NetworkInformation;
using Microsoft.WindowsAzure.MobileServices;
using System;
using System.ComponentModel;
using System.IO.IsolatedStorage;
using System.Threading;
using System.Windows;
using System.Windows.Navigation;
using System.Windows.Threading;

namespace HubSpotr.WindowsPhone
{
    public partial class MainPage : PhoneApplicationPage
    {
        private DispatcherTimer timer;

        public MainPage()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (!DeviceNetworkInformation.IsNetworkAvailable)
            {
                MessageBox.Show("HubSpotr requires an internet connection", "Sorry", MessageBoxButton.OK);
                NavigationService.GoBack();
                return;
            }

            this.timer = new DispatcherTimer();
            this.timer.Interval = new TimeSpan(0, 0, 3);
            this.timer.Tick += SplashEnded;
            this.timer.Start();
        }

        protected override void OnBackKeyPress(CancelEventArgs e)
        {
            while (NavigationService.CanGoBack)
                NavigationService.RemoveBackEntry();

            base.OnBackKeyPress(e);
        }

        private void SplashEnded(object sender, EventArgs e)
        {
            this.timer.Stop();
            this.timer.Tick -= SplashEnded;

            if (LoginExisting())
                NavigationService.Navigate(new Uri("/DiscoveryPage.xaml", UriKind.Relative));
            else
            {
                bLoginF.Visibility = Visibility.Visible;
                pbLoading.IsVisible = false;
            }
        }

        private bool LoginExisting()
        {
            IsolatedStorageSettings settings = IsolatedStorageSettings.ApplicationSettings;

            if (!settings.Contains("id") || !settings.Contains("token"))
                return false;

            var id = (string)settings["id"];
            var token = (string)settings["token"];

            AzureContext.Client.CurrentUser = new MobileServiceUser(id)
            {
                MobileServiceAuthenticationToken = token
            };

            return true;
        }


        private async void NewLogin(object sender, EventArgs e)
        {
            try
            {
                MobileServiceUser user = await AzureContext.Client.LoginAsync(MobileServiceAuthenticationProvider.Facebook);

                IsolatedStorageSettings settings = IsolatedStorageSettings.ApplicationSettings;

                settings.Add("id", user.UserId);
                settings.Add("token", user.MobileServiceAuthenticationToken);
                settings.Save();

                NavigationService.Navigate(new Uri("/DiscoveryPage.xaml", UriKind.Relative));
            }
            catch
            {
                MessageBox.Show("You must authenticate to use HubSpotr", "Failed", MessageBoxButton.OK);
            }
        }
    }
}