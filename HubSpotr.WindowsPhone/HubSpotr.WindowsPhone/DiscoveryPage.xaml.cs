using HubSpotr.Core.Extensions;
using HubSpotr.Core.Model;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Controls.Maps;
using Microsoft.Phone.Shell;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Device.Location;
using System.IO.IsolatedStorage;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Navigation;

namespace HubSpotr.WindowsPhone
{
    public partial class DiscoveryPage : PhoneApplicationPage
    {
        private readonly GeoCoordinateWatcher coordinateWatcher;

        public ObservableCollection<Hub> Hubs { get; private set; }

        private GeoCoordinate lastCoordinate;

        public DiscoveryPage()
        {
            this.DataContext = this;

            InitializeComponent();

            Hubs = new ObservableCollection<Hub>();

            Hubs.CollectionChanged += (o, e) => spNoResults.Visibility = Hubs.Count == 0 ? Visibility.Visible : Visibility.Collapsed;

            this.coordinateWatcher = new GeoCoordinateWatcher(GeoPositionAccuracy.High);
            this.coordinateWatcher.PositionChanged += OnPositionChanged;
            this.coordinateWatcher.StatusChanged += OnLocationStatusChanged;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            this.coordinateWatcher.Start();
            base.OnNavigatedTo(e);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            this.coordinateWatcher.Stop();
            base.OnNavigatedFrom(e);
        }

        protected override void OnBackKeyPress(CancelEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show("Are you sure you want to leave?", "Confirm", MessageBoxButton.OKCancel);

            if (result == MessageBoxResult.OK)
                while (NavigationService.CanGoBack)
                    NavigationService.RemoveBackEntry();
            else
                e.Cancel = true;

            base.OnBackKeyPress(e);
        }

        private void OnLocationStatusChanged(object sender, GeoPositionStatusChangedEventArgs e)
        {
        }

        private void OnPositionChanged(object sender, GeoPositionChangedEventArgs<GeoCoordinate> e)
        {
            mLocation.Visibility = Visibility.Visible;

            this.lastCoordinate = e.Position.Location;

            mLocation.SetView(this.lastCoordinate, 15);

            mLocation.Children.Clear();

            mLocation.Children.Add(new Pushpin
            {
                Location = this.lastCoordinate,
                Content = new TextBlock
                {
                    Text = "you",
                    Foreground = (SolidColorBrush)Application.Current.Resources["HubSpotr_Pink"]
                }
            });

            var referenceHub = new Hub
            {
                Lat = e.Position.Location.Latitude,
                Lng = e.Position.Location.Longitude
            };

            RefreshHubs(referenceHub, 10);
        }

        private async void RefreshHubs(Hub reference, int quantity)
        {
            pbLoading.Visibility = Visibility.Visible;
            spNoResults.Visibility = Visibility.Collapsed;

            List<Hub> nearHubs = await reference.NearHubs(quantity);

            pbLoading.Visibility = Visibility.Collapsed;

            Hubs.Clear();

            foreach (var hub in nearHubs)
            {
                Hubs.Add(hub);
                mLocation.Children.Add(new Pushpin
                {
                    Location = new GeoCoordinate(hub.Lat, hub.Lng),
                    Content = new TextBlock 
                    {
                        Text = hub.Name
                    }
                });
            }
        }

        private void JoinHub(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count != 1)
                return;

            var selected = (Hub)e.AddedItems[0];

            PhoneApplicationService.Current.State["hub"] = selected;
            NavigationService.Navigate(new Uri("/HubPage.xaml", UriKind.Relative));
        }

        private void ApplicationBarMenuItem_Click(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri(string.Format("/CreateHubPage.xaml?lat={0}&lng={1}", this.lastCoordinate.Latitude, this.lastCoordinate.Longitude), UriKind.Relative));
        }

        private void Logout(object sender, EventArgs e)
        {
            MessageBoxResult result = MessageBox.Show("Are you sure?", "confirm", MessageBoxButton.OKCancel);

            if (result != MessageBoxResult.OK)
                return;

            IsolatedStorageSettings settings = IsolatedStorageSettings.ApplicationSettings;

            settings.Remove("id");
            settings.Remove("token");
            settings.Save();

            NavigationService.Navigate(new Uri("/MainPage.xaml", UriKind.Relative));
        }

        private void spNoResults_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            ApplicationBarMenuItem_Click(this, EventArgs.Empty);
        }
    }
}