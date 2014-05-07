using HubSpotr.Core.Extensions;
using HubSpotr.Core.Model;
using HubSpotr.WindowsPhone.ViewModels;
using Microsoft.Phone.Controls;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Device.Location;
using System.Linq;
using System.Windows;
using System.Windows.Navigation;
using System.Windows.Threading;
using Windows.Devices.Geolocation;
using Microsoft.WindowsAzure.MobileServices;
using Microsoft.Phone.Notification;

namespace HubSpotr.WindowsPhone
{
    public partial class HubPage : PhoneApplicationPage
    {
        private bool isInRange;

        public HubPage()
        {
            InitializeComponent();
        }

        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            string hubId;
            if (NavigationContext.QueryString.TryGetValue("hubId", out hubId))
            {
                if (!MainPage.GetCredentials())
                {
                    NavigationService.Navigate(new Uri("/MainPage.xaml", UriKind.Relative));
                    return;
                }

                int intHubId = int.Parse(hubId);
                App.Hub = new HubViewModel((await App.MobileServiceClient.GetTable<Hub>().Where(h => h.Id == intHubId).Take(1).ToListAsync()).First());
            }

            this.DataContext = App.Hub;

            await App.MobileServiceClient.GetPush().RegisterNativeAsync(App.CurrentChannel.ChannelUri.ToString(), new string[] { App.Hub.Id.ToString() });
            App.CurrentChannel.ChannelUriUpdated += OnNotificationChannelEvent;
            App.CurrentChannel.ShellToastNotificationReceived += QueryPosts;

            App.Geolocator.PositionChanged += OnPositionChanged;

            await App.Hub.Source.Join();

            this.isInRange = true;

            QueryPosts(this, null);

            base.OnNavigatedTo(e);
        }

        protected async override void OnNavigatedFrom(NavigationEventArgs e)
        {
            App.Geolocator.PositionChanged -= OnPositionChanged;

            App.CurrentChannel.ChannelUriUpdated -= OnNotificationChannelEvent;
            App.CurrentChannel.ShellToastNotificationReceived -= QueryPosts;
            await App.MobileServiceClient.GetPush().UnregisterNativeAsync();

            await App.Hub.Source.Leave();

            base.OnNavigatedFrom(e);
        }

        private async void OnNotificationChannelEvent(object sender, NotificationChannelUriEventArgs e)
        {
            var push = App.MobileServiceClient.GetPush();
            await push.UnregisterNativeAsync();

            if (App.Hub != null)
                await push.RegisterNativeAsync(App.CurrentChannel.ChannelUri.ToString(), new string[] { App.Hub.Id.ToString() });
        }

        protected override void OnBackKeyPress(CancelEventArgs e)
        {
            MessageBoxResult response = MessageBox.Show("Are you sure you want to leave this hub?", "confirm", MessageBoxButton.OK);

            if (response != MessageBoxResult.OK)
                e.Cancel = true;
            else
                NavigationService.Navigate(new Uri("/DiscoveryPage.xaml", UriKind.Relative));
        }

        private async void QueryPosts(object sender, Microsoft.Phone.Notification.NotificationEventArgs e)
        {
            Dispatcher.BeginInvoke(() =>
            {
                pbLoading.Visibility = Visibility.Visible;
            });

            PostViewModel latestPost = App.Hub.Posts.OrderByDescending(p => p.At).FirstOrDefault();

            IList<Post> newPosts = Enumerable.Empty<Post>().ToList();
            try
            {
                newPosts = latestPost != null ? await App.Hub.Source.NewPosts(latestPost.At) : await App.Hub.Source.Posts();
            }
            catch(Exception ex)
            {

            }

            Dispatcher.BeginInvoke(() =>
            {
                foreach (var post in newPosts.Reverse())
                {
                    post.Picture += "?width=73&height=73";
                    App.Hub.Posts.Insert(0, new PostViewModel(post));
                }

                pbLoading.Visibility = Visibility.Collapsed;
            });
        }

        private void OnLocationStatusChanged(Geolocator sender, StatusChangedEventArgs e)
        {
        }

        private void OnPositionChanged(Geolocator sender, PositionChangedEventArgs e)
        {
            if (!isInRange)
                return;

            GeoCoordinate location = new GeoCoordinate(e.Position.Coordinate.Latitude, e.Position.Coordinate.Longitude);

            double distanceToHubCenter = location.GetDistanceTo(new GeoCoordinate(App.Hub.Lat, App.Hub.Lng));

            if (distanceToHubCenter > App.Hub.Radius)
            {
                isInRange = false;
                Dispatcher.BeginInvoke(() =>
                {
                    MessageBox.Show("You fell out of this hubs range. Exiting.", "out of range", MessageBoxButton.OK);
                    NavigationService.Navigate(new Uri("/DiscoveryPage.xaml", UriKind.Relative));
                });
            }
        }

        private void Post(object sender, RoutedEventArgs e)
        {
            string message = tbMessage.Text;

            tbMessage.Text = string.Empty;

            if (message == null || (message = message.Trim()) == string.Empty)
                return;

            pbLoading.Visibility = Visibility.Visible;

            new Post
            {
                HubId = App.Hub.Id,
                Message = message
            }.Post();

            pbLoading.Visibility = Visibility.Collapsed;
        }
    }
}