using System.Runtime.Serialization;

namespace HubSpotr.Core.Model
{
    public sealed class Hub
    {
        public int Id { get; set; }

        [DataMember(Name = "name")]
        public string Name { get; set; }

        [DataMember(Name = "lat")]
        public double Lat { get; set; }

        [DataMember(Name = "lng")]
        public double Lng { get; set; }

        [DataMember(Name = "radius")]
        public double Radius { get; set; }

        [DataMember(Name = "participants")]
        public int Participants { get; set; }

        public string Filter { get; set; }
    }
}