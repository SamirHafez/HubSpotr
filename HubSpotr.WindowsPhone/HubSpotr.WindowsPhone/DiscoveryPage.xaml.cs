using HubSpotr.Core.Extensions;
using HubSpotr.Core.Model;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Device.Location;


namespace HubSpotr.WindowsPhone
{
    public partial class DiscoveryPage : PhoneApplicationPage
    {
        private readonly GeoCoordinateWatcher coordinateWatcher;

        private readonly ObservableCollection<Hub> hubs;

        public DiscoveryPage()
        {
            InitializeComponent();

            this.hubs = new ObservableCollection<Hub>();
            lb1.ItemsSource = this.hubs;

            this.coordinateWatcher = new GeoCoordinateWatcher(GeoPositionAccuracy.High);
            this.coordinateWatcher.PositionChanged += coordinateWatcher_PositionChanged;
            this.coordinateWatcher.StatusChanged += coordinateWatcher_StatusChanged;
            this.coordinateWatcher.Start();
        }

        private void coordinateWatcher_StatusChanged(object sender, GeoPositionStatusChangedEventArgs e)
        {
        }

        private void coordinateWatcher_PositionChanged(object sender, GeoPositionChangedEventArgs<GeoCoordinate> e)
        {
            var referenceHub = new Hub
            {
                Lat = e.Position.Location.Latitude,
                Lng = e.Position.Location.Longitude,
                Radius = 1 * 1000
            };

            RefreshHubs(referenceHub, 5);
        }

        private async void RefreshHubs(Hub reference, int quantity)
        {
            List<Hub> nearHubs = await reference.NearHubs(quantity);

            this.hubs.Clear();

            foreach (var hub in nearHubs)
                this.hubs.Add(hub);
        }

        private void lb1_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            var selected = (Hub)e.AddedItems[0];

            PhoneApplicationService.Current.State["hub"] = selected;
            this.coordinateWatcher.Stop();
            NavigationService.Navigate(new Uri("/HubPage.xaml", UriKind.Relative));
        }

    }
}