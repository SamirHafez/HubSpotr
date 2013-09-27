using HubSpotr.Core.Extensions;
using HubSpotr.Core.Model;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Device.Location;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Navigation;

namespace HubSpotr.WindowsPhone
{
    public partial class HubPage : PhoneApplicationPage
    {
        private readonly GeoCoordinateWatcher coordinateWatcher;

        private readonly Hub hub;

        public ObservableCollection<Post> Posts { get; set; }

        private Timer timer;

        public HubPage()
        {
            this.DataContext = this;

            InitializeComponent();

            Posts = new ObservableCollection<Post>();

            this.hub = (Hub)PhoneApplicationService.Current.State["hub"];
            tbHubName.Text = this.hub.Name;

            this.coordinateWatcher = new GeoCoordinateWatcher(GeoPositionAccuracy.High);
            this.coordinateWatcher.PositionChanged += coordinateWatcher_PositionChanged;
            this.coordinateWatcher.StatusChanged += coordinateWatcher_StatusChanged;
        }

        private async void PhoneApplicationPage_Loaded(object sender, RoutedEventArgs e)
        {
            await this.hub.Join();

            List<Post> posts = await this.hub.Posts();

            foreach (var post in posts)
            {
                post.Picture += "?width=73&height=73";
                Posts.Add(post);
            }

            this.timer = new Timer(OnTimedEvent, null, 0, 2000);

            this.coordinateWatcher.Start();
        }

        protected async override void OnNavigatedFrom(NavigationEventArgs e)
        {
            this.coordinateWatcher.Stop();
            await this.hub.Leave();

            base.OnNavigatedFrom(e);
        }

        protected override void OnBackKeyPress(CancelEventArgs e)
        {
            MessageBoxResult response = MessageBox.Show("You are about to leave this hub", "Leaving", MessageBoxButton.OK);

            if (response != MessageBoxResult.OK)
                e.Cancel = true;

            base.OnBackKeyPress(e);
        }

        private async void OnTimedEvent(object state)
        {
            Post latestPost = Posts.OrderByDescending(p => p.At).FirstOrDefault();

            List<Post> newPosts = latestPost != null ? await this.hub.NewPosts(latestPost.At) : await this.hub.Posts();

            Dispatcher.BeginInvoke(() =>
            {
                foreach (var post in newPosts)
                {
                    post.Picture += "?width=73&height=73";
                    Posts.Insert(0, post);
                }
            });
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
                this.coordinateWatcher.Stop();
                MessageBox.Show("You fell out of this hubs range", "Sorry", MessageBoxButton.OK);
                NavigationService.GoBack();
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            string message = tbMessage.Text;

            if (message == null || (message = message.Trim()) == string.Empty)
                return;

            new Post
            {
                HubId = this.hub.Id,
                Message = message
            }.Post();
        }
    }
}