using HubSpotr.Core.Extensions;
using HubSpotr.Core.Model;
using HubSpotr.WindowsPhone.ViewModels;
using Microsoft.Phone.Controls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Device.Location;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Navigation;
using Windows.Devices.Geolocation;

namespace HubSpotr.WindowsPhone
{
    public partial class HubPage : PhoneApplicationPage
    {
        private readonly Geolocator geolocator;

        private Timer timer;

        public HubPage()
        {
            this.DataContext = App.Hub;

            InitializeComponent();

            tbHubName.Text = App.Hub.Name;

            this.geolocator = new Geolocator
            {
                DesiredAccuracyInMeters = 10,
                MovementThreshold = 10,
            };

            this.geolocator.PositionChanged += OnPositionChanged;
            this.geolocator.StatusChanged += OnLocationStatusChanged;
        }

        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            await App.Hub.Source.Join();

            List<Post> posts = await App.Hub.Source.Posts();

            foreach (var post in posts)
            {
                post.Picture += "?width=73&height=73";
                App.Hub.Posts.Add(new PostViewModel(post));
            }

            this.timer = new Timer(QueryPosts, null, 0, 2000);
            
            base.OnNavigatedTo(e);
        }

        protected async override void OnNavigatedFrom(NavigationEventArgs e)
        {
            await App.Hub.Source.Leave();

            base.OnNavigatedFrom(e);
        }

        protected override void OnBackKeyPress(CancelEventArgs e)
        {
            MessageBoxResult response = MessageBox.Show("You are about to leave this hub", "leaving", MessageBoxButton.OK);

            if (response != MessageBoxResult.OK)
                e.Cancel = true;
            else
                NavigationService.Navigate(new Uri("/DiscoveryPage.xaml", UriKind.Relative));
        }

        private async void QueryPosts(object state)
        {
            PostViewModel latestPost = App.Hub.Posts.OrderByDescending(p => p.At).FirstOrDefault();

            List<Post> newPosts = latestPost != null ? await App.Hub.Source.NewPosts(latestPost.At) : await App.Hub.Source.Posts();

            Dispatcher.BeginInvoke(() =>
            {
                foreach (var post in newPosts)
                {
                    post.Picture += "?width=73&height=73";
                    App.Hub.Posts.Insert(0, new PostViewModel(post));
                }
            });
        }

        private void OnLocationStatusChanged(Geolocator sender, StatusChangedEventArgs e)
        {
        }

        private void OnPositionChanged(Geolocator sender, PositionChangedEventArgs e)
        {
            GeoCoordinate location = new GeoCoordinate(e.Position.Coordinate.Latitude, e.Position.Coordinate.Longitude);

            double distanceToHubCenter = location.GetDistanceTo(new GeoCoordinate(App.Hub.Lat, App.Hub.Lng));

            if (distanceToHubCenter > App.Hub.Radius)
            {
                MessageBox.Show("You fell out of this hubs range", "exiting", MessageBoxButton.OK);
                NavigationService.GoBack();
            }
        }

        private void Post(object sender, RoutedEventArgs e)
        {
            string message = tbMessage.Text;

            tbMessage.Text = string.Empty;

            if (message == null || (message = message.Trim()) == string.Empty)
                return;

            new Post
            {
                HubId = App.Hub.Id,
                Message = message
            }.Post();
        }
    }
}