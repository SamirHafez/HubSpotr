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
using System.Windows.Shapes;
using Windows.Devices.Geolocation;

namespace HubSpotr.WindowsPhone
{
    public partial class CreateHubPage : PhoneApplicationPage
    {
        private Geolocator geolocator;

        // http://msdn.microsoft.com/en-us/library/aa940990.aspx
        private const double MAP_ZOOM = 16;
        private const double MAP_CONSTANT = 2.39;

        public CreateHubPage()
        {
            InitializeComponent();
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
                spFields.Visibility = Visibility.Visible;

                mLocation.SetView(geoCoordinate, MAP_ZOOM, MapAnimationKind.None);

                mLocation.Visibility = Visibility.Visible;

                if (mLocation.Layers.Count > 0)
                {
                    var layer = mLocation.Layers[0];

                    var point = layer[0];
                    var radius = layer[0];

                    point.GeoCoordinate = geoCoordinate;
                    radius.GeoCoordinate = geoCoordinate;
                }
                else
                {
                    MapLayer pinLayer = CreateMapLayer(geoCoordinate);
                    mLocation.Layers.Add(pinLayer);
                }
            });
        }

        private MapLayer CreateMapLayer(GeoCoordinate geoCoordinate)
        {
            var pinLayer = new MapLayer();
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

            pinLayer.Add(new MapOverlay
            {
                GeoCoordinate = geoCoordinate,
                PositionOrigin = new Point(.5, .5),
                Content = new Ellipse
                {
                    Width = (sRadius.Value / MAP_CONSTANT) * 2,
                    Height = (sRadius.Value / MAP_CONSTANT) * 2,
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

                ellipse.Width = (e.NewValue / MAP_CONSTANT) * 2;
                ellipse.Height = (e.NewValue / MAP_CONSTANT) * 2;
            }
        }
    }
}