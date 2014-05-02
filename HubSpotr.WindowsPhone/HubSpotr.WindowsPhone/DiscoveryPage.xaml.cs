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
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Windows.Devices.Geolocation;

namespace HubSpotr.WindowsPhone
{
    public partial class DiscoveryPage : PhoneApplicationPage
    {
        public DiscoveryPage()
        {
            this.DataContext = App.Hubs;

            InitializeComponent();

            App.Hubs.CollectionChanged += (o, e) => spNoResults.Visibility = App.Hubs.Count == 0 ? Visibility.Visible : Visibility.Collapsed;

            if (App.Hubs.Count > 0)
                pbLoading.Visibility = Visibility.Collapsed;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            App.Geolocator.PositionChanged += OnPositionChanged;
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            App.Geolocator.PositionChanged -= OnPositionChanged;
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
        }

        private void OnPositionChanged(Geolocator sender, PositionChangedEventArgs e)
        {
            var geoCoordinate = new GeoCoordinate(e.Position.Coordinate.Latitude, e.Position.Coordinate.Longitude);
            var accuracy = e.Position.Coordinate.Accuracy;

            Dispatcher.BeginInvoke(() =>
            {
                pbLoading.Visibility = Visibility.Visible;
                mLocation.Visibility = Visibility.Visible;

                mLocation.SetView(geoCoordinate, 16, MapAnimationKind.None);

                if (mLocation.Layers.Count > 0)
                {
                    var pinLayer = mLocation.Layers[0];
                    pinLayer[0].GeoCoordinate = geoCoordinate;
                }
                else
                {
                    var pinLayer = new MapLayer();
                    Pushpin p = new Pushpin();
                    pinLayer.Add(new MapOverlay
                    {
                        GeoCoordinate = geoCoordinate,
                        PositionOrigin = new Point(.5, .5),
                        Content = new Ellipse
                        {
                            Width = accuracy,
                            Height = accuracy,
                            //Fill = new RadialGradientBrush(((SolidColorBrush)Application.Current.Resources["HubSpotr_Black"]).Color, Colors.Transparent),
                            Fill = (SolidColorBrush)Application.Current.Resources["HubSpotr_Black"],
                            Opacity = .05
                        }
                    });

                    mLocation.Layers.Add(pinLayer);
                }

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
            IList<HubViewModel> nearHubs = (await reference.NearHubs(quantity))
                .Select(nh => new HubViewModel(nh))
                .ToList();

            App.Hubs.Clear();

            MapLayer pinLayer = null;
            if (mLocation.Layers.Count > 1)
            {
                pinLayer = mLocation.Layers[1];
                pinLayer.Clear();
            }
            else
            {
                pinLayer = new MapLayer();
                mLocation.Layers.Add(pinLayer);
            }

            foreach (var hub in nearHubs)
            {
                App.Hubs.Add(hub);

                pinLayer.Add(new MapOverlay
                {
                    GeoCoordinate = new GeoCoordinate(hub.Lat, hub.Lng),
                    PositionOrigin = new Point(.5, .5),
                    Content = new Ellipse
                    {
                        Width = (hub.Radius / 2.39) * 2,
                        Height = (hub.Radius / 2.39) * 2,
                        Fill = hub.Color,
                        Opacity = .5
                    }
                });
            }

            pbLoading.Visibility = Visibility.Collapsed;
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