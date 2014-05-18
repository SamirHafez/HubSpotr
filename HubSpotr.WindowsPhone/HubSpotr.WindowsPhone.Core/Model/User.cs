using System.Runtime.Serialization;

namespace HubSpotr.Core.Model
{
    [DataContract]
    public sealed class User
    {
        [DataMember(Name = "id")]
        public string Id { get; set; }

        [DataMember(Name = "name")]
        public string Name { get; set; }

        [DataMember(Name = "email")]
        public string Email { get; set; }

        [DataMember(Name = "hubId")]
        public int HubId { get; set; }

        [DataMember(Name = "hub")]
        public Hub Hub { get; set; }
    }
}
