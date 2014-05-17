using System;
using System.Runtime.Serialization;

namespace HubSpotr.Core.Model
{
    [DataContract]
    public sealed class Post
    {
        [DataMember(Name = "id")]
        public int Id { get; set; }

        [DataMember(Name = "hubId")]
        public int HubId { get; set; }

        [DataMember(Name = "userId")]
        public string UserId { get; set; }

        [DataMember(Name = "userName")]
        public string UserName { get; set; }

        [DataMember(Name = "message")]
        public string Message { get; set; }

        [DataMember(Name = "at")]
        public DateTime At { get; set; }
    }
}