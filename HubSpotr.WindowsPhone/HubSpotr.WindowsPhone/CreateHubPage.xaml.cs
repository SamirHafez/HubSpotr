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
                    Foreground = new SolidColorBrush(Colors.Red)
                }
            });

            base.OnNavigatedTo(e);
        }

        private async void Create(object sender, EventArgs e)
        {
            string name = tbName.Text;

            if (name == null || (name = name.Trim()) == string.Empty)
            {
                MessageBox.Show("Please specify a name for the hub", "error", MessageBoxButton.OK);
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

            MessageBox.Show("Hub successfully created", "Success", MessageBoxButton.OK);

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

                double radius = e.NewValue / 4.78;

                mLocation.Children.Add(new Ellipse
                {
                    Width = radius * 2,
                    Height = radius * 2,
                    Fill = new SolidColorBrush(Color.FromArgb(125, 255, 0, 0))
                });
            }
        }

        private void OnMapZoomChanged(object sender, MapZoomEventArgs e)
        {
            e.Handled = true;
        }

        private void OnMapPanChanged(object sender, MapDragEventArgs e)
        {
            e.Handled = true;
        }
    }
}