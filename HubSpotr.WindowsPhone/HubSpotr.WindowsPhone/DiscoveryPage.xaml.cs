using HubSpotr.Core.Extensions;
using HubSpotr.Core.Model;
using HubSpotr.WindowsPhone.Controls;
using HubSpotr.WindowsPhone.ViewModels;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Maps.Controls;
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
            mLocation.Visibility = Visibility.Visible;
        }

        private void OnPositionChanged(Geolocator sender, PositionChangedEventArgs e)
        {
            var geoCoordinate = new GeoCoordinate(e.Position.Coordinate.Latitude, e.Position.Coordinate.Longitude);
            var accuracy = e.Position.Coordinate.Accuracy;

            Dispatcher.BeginInvoke(async () =>
            {
                pbLoading.Visibility = Visibility.Visible;

                mLocation.SetView(geoCoordinate, App.MAP_ZOOM);

                RefreshPin(geoCoordinate, accuracy);

                var mockHub = new Hub
                {
                    Lat = geoCoordinate.Latitude,
                    Lng = geoCoordinate.Longitude
                };

                IList<HubViewModel> nearHubs = Enumerable.Empty<HubViewModel>().ToList();
                try
                {
                    nearHubs = (await mockHub.NearHubs(App.HUBS_IN_PROXIMITY))
                        .Select(nh => new HubViewModel(nh))
                        .ToList();
                }
                catch
                {
                    MessageBox.Show("There was an error downloading the list of hubs. Please try again.", "error", MessageBoxButton.OK);
                }

                RefreshHubs(nearHubs);

                pbLoading.Visibility = Visibility.Collapsed;
            });
        }

        private void RefreshPin(GeoCoordinate geoCoordinate, double accuracy)
        {
            if (mLocation.Layers.Count > 0)
            {
                var pinLayer = mLocation.Layers[0];
                pinLayer[0].GeoCoordinate = geoCoordinate;
                pinLayer[1].GeoCoordinate = geoCoordinate;

                var ellipse = (Ellipse)pinLayer[0].Content;
                ellipse.Width = accuracy;
                ellipse.Height = accuracy;
            }
            else
            {
                var pinLayer = new MapLayer();
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
                pinLayer.Add(new MapOverlay
                {
                    GeoCoordinate = geoCoordinate,
                    PositionOrigin = new Point(.5, .5),
                    Content = new Ellipse
                    {
                        Width = 5,
                        Height = 5,
                        Fill = (SolidColorBrush)Application.Current.Resources["HubSpotr_Black"]
                    }
                });

                mLocation.Layers.Add(pinLayer);
            }
        }

        private void RefreshHubs(IList<HubViewModel> nearHubs)
        {
            MapLayer hubLayer = null;
            if (mLocation.Layers.Count > 1)
                hubLayer = mLocation.Layers[1];
            else
            {
                hubLayer = new MapLayer();
                mLocation.Layers.Add(hubLayer);
            }

            var deletedHubs = App.Hubs.Except(nearHubs).ToList();
            foreach (var deletedHub in deletedHubs)
            {
                App.Hubs.Remove(deletedHub);

                if (hubLayer.Count > 0)
                {
                    var hubFromLayer = hubLayer.First(hl => ((HubViewModel)((Ellipse)hl.Content).DataContext).Equals(deletedHub));
                    hubLayer.Remove(hubFromLayer);
                }
            }

            if (hubLayer.Count == 0)
                foreach (var hub in App.Hubs)
                {
                    hubLayer.Add(new MapOverlay
                    {
                        GeoCoordinate = new GeoCoordinate(hub.Lat, hub.Lng),
                        PositionOrigin = new Point(.5, .5),
                        Content = new Ellipse
                        {
                            DataContext = hub,
                            Width = (hub.Radius / App.MAP_CONSTANT) * 2,
                            Height = (hub.Radius / App.MAP_CONSTANT) * 2,
                            Fill = hub.Color,
                            Opacity = .5
                        }
                    });
                }

            var newHubs = nearHubs.Except(App.Hubs).ToList();
            foreach (var hub in newHubs)
            {
                App.Hubs.Add(hub);

                hubLayer.Add(new MapOverlay
                {
                    GeoCoordinate = new GeoCoordinate(hub.Lat, hub.Lng),
                    PositionOrigin = new Point(.5, .5),
                    Content = new Ellipse
                    {
                        DataContext = hub,
                        Width = (hub.Radius / App.MAP_CONSTANT) * 2,
                        Height = (hub.Radius / App.MAP_CONSTANT) * 2,
                        Fill = hub.Color,
                        Opacity = .5
                    }
                });
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