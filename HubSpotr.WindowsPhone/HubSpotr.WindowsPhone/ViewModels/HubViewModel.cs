using HubSpotr.Core.Model;
using System;
using System.Collections.ObjectModel;
using System.Windows.Media;

namespace HubSpotr.WindowsPhone.ViewModels
{
    public class HubViewModel : IEquatable<HubViewModel>
    {
        private static readonly Random randomizer = new Random();

        public int Id { get; set; }

        public string Name { get; set; }

        public double Lat { get; set; }

        public double Lng { get; set; }

        public double Radius { get; set; }

        public int Participants { get; set; }

        public ObservableCollection<PostViewModel> Posts { get; set; }

        public Hub Source { get; private set; }

        public SolidColorBrush Color { get; set; }

        public HubViewModel(Hub hub)
        {
            this.Source = hub;

            this.Id = hub.Id;
            this.Name = hub.Name;
            this.Lat = hub.Lat;
            this.Lng = hub.Lng;
            this.Radius = hub.Radius;
            this.Participants = hub.Participants;

            this.Color = new SolidColorBrush(System.Windows.Media.Color.FromArgb(255, (byte)randomizer.Next(256), (byte)randomizer.Next(256), (byte)randomizer.Next(256)));

            this.Posts = new ObservableCollection<PostViewModel>();
        }

        public bool Equals(HubViewModel other)
        {
            return this.Id.Equals(other.Id);
        }

        public override int GetHashCode()
        {
            return this.Id.GetHashCode();
        }
    }
}
