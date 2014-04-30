using HubSpotr.Core.Extensions;
using HubSpotr.Core.Model;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace HubSpotr.WindowsPhone.ViewModels
{
    public class HubViewModel
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public double Lat { get; set; }

        public double Lng { get; set; }

        public double Radius { get; set; }

        public int Participants { get; set; }

        public ObservableCollection<PostViewModel> Posts { get; set; }

        public Hub Source { get; private set; }

        public HubViewModel(Hub hub)
        {
            this.Source = hub;

            this.Id = hub.Id;
            this.Name = hub.Name;
            this.Lat = hub.Lat;
            this.Lng = hub.Lng;
            this.Radius = hub.Radius;
            this.Participants = hub.Participants;

            this.Posts = new ObservableCollection<PostViewModel>();
        }
    }
}
