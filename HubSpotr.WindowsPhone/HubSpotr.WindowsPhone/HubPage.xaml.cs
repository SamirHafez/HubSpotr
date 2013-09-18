using HubSpotr.Core.Model;
using HubSpotr.Core.Extensions;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using System;
using System.Linq;
using System.Collections.ObjectModel;
using System.Device.Location;
using System.Collections.Generic;
using System.Windows;
using System.Threading;

namespace HubSpotr.WindowsPhone
{
    public partial class HubPage : PhoneApplicationPage
    {
        private readonly GeoCoordinateWatcher coordinateWatcher;

        private readonly Hub hub;

        private ObservableCollection<Post> posts;

        private Timer timer;

        public HubPage()
        {
            InitializeComponent();

            this.hub = (Hub)PhoneApplicationService.Current.State["hub"];
            tbHubName.Text = this.hub.Name;

            this.coordinateWatcher = new GeoCoordinateWatcher(GeoPositionAccuracy.High);
            this.coordinateWatcher.PositionChanged += coordinateWatcher_PositionChanged;
            this.coordinateWatcher.StatusChanged += coordinateWatcher_StatusChanged;
            this.coordinateWatcher.Start();
        }

        protected override void OnBackKeyPress(System.ComponentModel.CancelEventArgs e)
        {
            MessageBoxResult response = MessageBox.Show("You are about to leave this hub", "Leaving", MessageBoxButton.OK);

            if (response != MessageBoxResult.OK)
                e.Cancel = true;

            base.OnBackKeyPress(e);
        }

        private async void OnTimedEvent(object state)
        {
            List<Post> newPosts = await this.hub.NewPosts(this.posts.OrderByDescending(p => p.At).First().At);

            foreach (var post in newPosts)
                await Dispatcher.InvokeAsync(() => this.posts.Insert(0, post));
        }

        private async void PhoneApplicationPage_Loaded(object sender, RoutedEventArgs e)
        {
            List<Post> posts = await this.hub.Posts();

            this.posts = new ObservableCollection<Post>(posts);

            lb1.ItemsSource = this.posts;

            this.timer = new Timer(OnTimedEvent, null, 0, 2000);
        }

        private void coordinateWatcher_StatusChanged(object sender, GeoPositionStatusChangedEventArgs e)
        {
        }

        private void coordinateWatcher_PositionChanged(object sender, GeoPositionChangedEventArgs<GeoCoordinate> e)
        {
            GeoCoordinate location = e.Position.Location;

            double distanceToHubCenter = location.GetDistanceTo(new GeoCoordinate(this.hub.Lat, this.hub.Lng));

            if (distanceToHubCenter > this.hub.Radius)
            {
                MessageBox.Show("You fell out of this hubs range", "Sorry", MessageBoxButton.OK);
                NavigationService.GoBack();
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            string message = tbMessage.Text;

            if (message == null || (message = message.Trim()) == string.Empty)
                return;

            var post = new Post
            {
                HubId = this.hub.Id,
                Message = message
            };

            post.Post();
        }
    }
}