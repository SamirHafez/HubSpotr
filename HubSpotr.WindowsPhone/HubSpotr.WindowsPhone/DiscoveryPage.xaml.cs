using HubSpotr.Core.Extensions;
using HubSpotr.Core.Model;
using HubSpotr.WindowsPhone.Controls;
using HubSpotr.WindowsPhone.ViewModels;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Maps.Controls;
using Microsoft.Phone.Maps.Toolkit;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Device.Location;
using System.IO.IsolatedStorage;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Windows.Devices.Geolocation;

namespace HubSpotr.WindowsPhone
{
    public partial class DiscoveryPage : PhoneApplicationPage
    {
        private Geolocator geolocator;

        public DiscoveryPage()
        {
            this.DataContext = App.Hubs;

            InitializeComponent();

            App.Hubs.CollectionChanged += (o, e) => spNoResults.Visibility = App.Hubs.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
        }

        protected override void OnBackKeyPress(CancelEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show("Are you sure you want to leave?", "Confirm", MessageBoxButton.OKCancel);

            if (result == MessageBoxResult.OK)
                Application.Current.Terminate();
            else
                e.Cancel = true;
        }

        private void mLocation_Loaded(object sender, RoutedEventArgs e)
        {
            this.geolocator = new Geolocator
            {
                DesiredAccuracyInMeters = 10,
                MovementThreshold = 10,
            };

            this.geolocator.PositionChanged += OnPositionChanged;
        }

        private void OnPositionChanged(Geolocator sender, PositionChangedEventArgs e)
        {
            var geoCoordinate = new GeoCoordinate(e.Position.Coordinate.Latitude, e.Position.Coordinate.Longitude);

            Dispatcher.BeginInvoke(() =>
            {
                pbLoading.Visibility = Visibility.Collapsed;

                mLocation.Visibility = Visibility.Visible;

                mLocation.SetView(geoCoordinate, 16, MapAnimationKind.None);

                mLocation.Layers.Clear();

                var pinLayer = new MapLayer();
                Pushpin p = new Pushpin();
                pinLayer.Add(new MapOverlay
                {
                    GeoCoordinate = geoCoordinate,
                    PositionOrigin = new Point(.5, .5),
                    Content = new Ellipse
                    {
                        Width = 5,
                        Height = 5,
                        Fill = (SolidColorBrush)Application.Current.Resources["HubSpotr_Black"],
                    }
                });

                mLocation.Layers.Add(pinLayer);

                var referenceHub = new Hub
                {
                    Lat = e.Position.Coordinate.Latitude,
                    Lng = e.Position.Coordinate.Longitude
                };

                RefreshHubs(referenceHub, 10);
            });
        }

        private async void RefreshHubs(Hub reference, int quantity)
        {
            spNoResults.Visibility = Visibility.Collapsed;

            List<Hub> nearHubs = await reference.NearHubs(quantity);

            App.Hubs.Clear();

            foreach (var hub in nearHubs)
            {
                App.Hubs.Add(new HubViewModel(hub));

                var pinLayer = new MapLayer();
                pinLayer.Add(new MapOverlay
                {
                    GeoCoordinate = new GeoCoordinate(hub.Lat, hub.Lng),
                    PositionOrigin = new Point(.5, .5),
                    Content = new Ellipse
                    {
                        Width = (hub.Radius / 2.39) * 2,
                        Height = (hub.Radius / 2.39) * 2,
                        Fill = (SolidColorBrush)Application.Current.Resources["HubSpotr_Pink"],
                        Opacity = .5
                    }
                });
                pinLayer.Add(new MapOverlay
                {
                    GeoCoordinate = new GeoCoordinate(hub.Lat, hub.Lng),
                    PositionOrigin = new Point(.5, .5),
                    Content = new TextBlock
                    {
                        Text = hub.Name,
                        Foreground = new SolidColorBrush(Colors.White)
                    }
                });

                mLocation.Layers.Add(pinLayer);
            }
        }

        private void ApplicationBarMenuItem_Click(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri("/CreateHubPage.xaml", UriKind.Relative));
        }

        private void Logout(object sender, EventArgs e)
        {
            MessageBoxResult result = MessageBox.Show("Are you sure?", "logout", MessageBoxButton.OKCancel);

            if (result != MessageBoxResult.OK)
                return;

            IsolatedStorageSettings settings = IsolatedStorageSettings.ApplicationSettings;

            settings.Remove("id");
            settings.Remove("token");
            settings.Save();

            NavigationService.Navigate(new Uri("/MainPage.xaml", UriKind.Relative));
        }

        private void JoinHub(object sender, System.Windows.Input.GestureEventArgs e)
        {
            var selected = (HubViewModel)((HubControl)sender).DataContext;

            App.Hub = selected;
            NavigationService.Navigate(new Uri("/HubPage.xaml", UriKind.Relative));
        }

        private void spNoResults_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            NavigationService.Navigate(new Uri("/CreateHubPage.xaml", UriKind.Relative));
        }
    }
}