﻿using HubSpotr.Core.Extensions;
using HubSpotr.Core.Model;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Device.Location;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;

namespace HubSpotr.WindowsPhone
{
    public partial class DiscoveryPage : PhoneApplicationPage
    {
        private readonly GeoCoordinateWatcher coordinateWatcher;

        private readonly ObservableCollection<Hub> hubs;

        private GeoCoordinate lastCoordinate;

        public DiscoveryPage()
        {
            InitializeComponent();

            this.hubs = new ObservableCollection<Hub>();
            lb1.ItemsSource = this.hubs;

            this.coordinateWatcher = new GeoCoordinateWatcher(GeoPositionAccuracy.High);
            this.coordinateWatcher.PositionChanged += coordinateWatcher_PositionChanged;
            this.coordinateWatcher.StatusChanged += coordinateWatcher_StatusChanged;
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

        private void coordinateWatcher_StatusChanged(object sender, GeoPositionStatusChangedEventArgs e)
        {
        }

        private void coordinateWatcher_PositionChanged(object sender, GeoPositionChangedEventArgs<GeoCoordinate> e)
        {
            this.lastCoordinate = e.Position.Location;

            var referenceHub = new Hub
            {
                Lat = e.Position.Location.Latitude,
                Lng = e.Position.Location.Longitude
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

        private void lb1_SelectionChanged(object sender, SelectionChangedEventArgs e)
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

    }
}