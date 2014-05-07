using Microsoft.Phone.Controls;
using Microsoft.Phone.Net.NetworkInformation;
using Microsoft.WindowsAzure.MobileServices;
using System;
using System.ComponentModel;
using System.IO.IsolatedStorage;
using System.Windows;
using System.Windows.Navigation;
using System.Windows.Threading;

namespace HubSpotr.WindowsPhone
{
    public partial class MainPage : PhoneApplicationPage
    {
        private DispatcherTimer timer;
        private bool hasCredentials;

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
                Application.Current.Terminate();
            }

            this.timer = new DispatcherTimer();
            this.timer.Interval = new TimeSpan(0, 0, 0, 1);
            this.timer.Tick += TryLogin;
            this.timer.Start();
            this.hasCredentials = GetCredentials();
        }

        protected override void OnBackKeyPress(CancelEventArgs e)
        {
            Application.Current.Terminate();
        }

        private void TryLogin(object sender, EventArgs e)
        {
            this.timer.Stop();
            this.timer.Tick -= TryLogin;

            if (hasCredentials)
                NavigationService.Navigate(new Uri("/DiscoveryPage.xaml", UriKind.Relative));
            else
            {
                pbLoading.Visibility = Visibility.Collapsed;
                bLoginF.Visibility = Visibility.Visible;
            }
        }

        public static bool GetCredentials()
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

            return true;
        }


        private async void NewLogin(object sender, EventArgs e)
        {
            try
            {
                MobileServiceUser user = await App.MobileServiceClient.LoginAsync(MobileServiceAuthenticationProvider.Facebook);

                IsolatedStorageSettings settings = IsolatedStorageSettings.ApplicationSettings;

                settings.Add("id", user.UserId);
                settings.Add("token", user.MobileServiceAuthenticationToken);
                settings.Save();

                NavigationService.Navigate(new Uri("/DiscoveryPage.xaml", UriKind.Relative));
            }
            catch
            {
                MessageBox.Show("Login failed.", "Failed", MessageBoxButton.OK);
            }
        }
    }
}