using HubSpotr.Core.Extensions;
using HubSpotr.Core.Model;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Controls.Maps;
using Microsoft.Phone.Shell;
using System;
using System.Device.Location;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace HubSpotr.WindowsPhone
{
    public partial class CreateHubPage : PhoneApplicationPage
    {
        private GeoCoordinate coordinates;

        // http://msdn.microsoft.com/en-us/library/aa940990.aspx
        private const double MAP_ZOOM = 15;
        private const double MAP_CONSTANT = 4.78;

        public CreateHubPage()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            var lat = double.Parse(NavigationContext.QueryString["lat"]);
            var lng = double.Parse(NavigationContext.QueryString["lng"]);

            this.coordinates = new GeoCoordinate(lat, lng);

            mLocation.SetView(this.coordinates, 15);
            mLocation.Children.Add(new Pushpin
            {
                Location = this.coordinates,
                Content = new TextBlock
                {
                    Text = "you",
                    Foreground = (SolidColorBrush)Application.Current.Resources["HubSpotr_Pink"]
                }
            });

            mLocation.Children.Add(new Ellipse
            {
                Width = (sRadius.Value / MAP_CONSTANT) * 2,
                Height = (sRadius.Value / MAP_CONSTANT) * 2,
                Fill = (SolidColorBrush)Application.Current.Resources["HubSpotr_Pink"],
                Opacity = .5
            });

            base.OnNavigatedTo(e);
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

            pbLoading.Visibility = Visibility.Visible;

            await new Hub
            {
                Name = tbName.Text,
                Radius = sRadius.Value,
                Lat = this.coordinates.Latitude,
                Lng = this.coordinates.Longitude
            }.Create();

            pbLoading.Visibility = Visibility.Collapsed;

            MessageBox.Show("Hub successfully created", "success", MessageBoxButton.OK);

            button.IsEnabled = true;
            NavigationService.GoBack();
        }

        private void OnRadiusChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (mLocation != null)
            {
                tbRadius.Text = string.Format("radius ({0:0}m)", e.NewValue);

                if(mLocation.Children.Count > 1)
                    mLocation.Children.RemoveAt(1);

                mLocation.Children.Add(new Ellipse
                {
                    Width = (e.NewValue / MAP_CONSTANT) * 2,
                    Height = (e.NewValue / MAP_CONSTANT) * 2,
                    Fill = (SolidColorBrush)Application.Current.Resources["HubSpotr_Pink"],
                    Opacity = .5
                });
            }
        }
    }
}