using System.Runtime.Serialization;

namespace HubSpotr.WindowsPhone.Core.Model
{
    [DataContract]
    public sealed class User
    {
        [DataMember(Name = "id")]
        public string Id { get; set; }
    }
}
