using System;
using System.Runtime.Serialization;

namespace HubSpotr.Core.Model
{
    public sealed class Post
    {
        public int Id { get; set; }

        [DataMember(Name = "hubId")]
        public int HubId { get; set; }

        [DataMember(Name = "name")]
        public string Name { get; set; }

        [DataMember(Name = "message")]
        public string Message { get; set; }

        [DataMember(Name = "at")]
        public DateTime At { get; set; }

        [DataMember(Name = "picture")]
        public string Picture { get; set; }
    }
}