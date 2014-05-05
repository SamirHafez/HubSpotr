using HubSpotr.Core.Extensions;
using HubSpotr.Core.Model;
using HubSpotr.WindowsPhone.ViewModels;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Maps.Controls;
using Microsoft.Phone.Shell;
using System;
using System.Device.Location;
using System.Windows;
using System.Windows.Media;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Windows.Devices.Geolocation;

namespace HubSpotr.WindowsPhone
{
    public partial class CreateHubPage : PhoneApplicationPage
    {
        public CreateHubPage()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            App.Geolocator.PositionChanged += OnPositionChanged;
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            App.Geolocator.PositionChanged -= OnPositionChanged;
        }

        private void mLocation_Loaded(object sender, RoutedEventArgs e)
        {
        }

        private void OnPositionChanged(Geolocator sender, PositionChangedEventArgs e)
        {
            var geoCoordinate = new GeoCoordinate(e.Position.Coordinate.Latitude, e.Position.Coordinate.Longitude);
            double accuracy = e.Position.Coordinate.Accuracy;

            Dispatcher.BeginInvoke(() =>
            {
                pbLoading.Visibility = Visibility.Collapsed;
                spFields.Visibility = Visibility.Visible;

                mLocation.SetView(geoCoordinate, App.MAP_ZOOM, MapAnimationKind.None);

                mLocation.Visibility = Visibility.Visible;

                if (mLocation.Layers.Count > 0)
                {
                    var layer = mLocation.Layers[0];

                    var point = layer[0];
                    var radius = layer[1];

                    point.GeoCoordinate = geoCoordinate;

                    var pointEllipse = (Ellipse)point.Content;
                    pointEllipse.Width = accuracy;
                    pointEllipse.Height = accuracy;

                    radius.GeoCoordinate = geoCoordinate;
                }
                else
                {
                    MapLayer pinLayer = CreateMapLayer(geoCoordinate, accuracy);
                    mLocation.Layers.Add(pinLayer);
                }

                sRadius.Minimum = accuracy;
            });
        }

        private MapLayer CreateMapLayer(GeoCoordinate geoCoordinate, double accuracy)
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
                    Width = (sRadius.Value / App.MAP_CONSTANT) * 2,
                    Height = (sRadius.Value / App.MAP_CONSTANT) * 2,
                    Fill = (SolidColorBrush)Application.Current.Resources["HubSpotr_Pink"],
                    Opacity = .5
                }
            });
            return pinLayer;
        }

        private async void Create(object sender, EventArgs e)
        {
            string name = tbName.Text;

            if (name == null || (name = name.Trim()) == string.Empty)
            {
                MessageBox.Show("Please specify a name for the hub", "missing field", MessageBoxButton.OK);
                return;
            }

            var button = (ApplicationBarIconButton)sender;
            button.IsEnabled = false;

            var geoCoordinate = mLocation.Layers[0][0].GeoCoordinate;

            App.Hub = new HubViewModel(await new Hub
            {
                Name = tbName.Text,
                Radius = sRadius.Value,
                Lat = geoCoordinate.Latitude,
                Lng = geoCoordinate.Longitude
            }.Create());

            button.IsEnabled = true;
            NavigationService.Navigate(new Uri("/HubPage.xaml", UriKind.Relative));
        }

        private void OnRadiusChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (mLocation != null)
            {
                tbRadius.Text = string.Format("radius ({0:0}m)", e.NewValue);

                var layer = mLocation.Layers[0];
                var ellipse = (Ellipse)layer[1].Content;

                ellipse.Width = (e.NewValue / App.MAP_CONSTANT) * 2;
                ellipse.Height = (e.NewValue / App.MAP_CONSTANT) * 2;
            }
        }
    }
}