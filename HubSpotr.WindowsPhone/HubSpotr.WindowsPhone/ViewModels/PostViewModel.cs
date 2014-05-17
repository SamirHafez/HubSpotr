using HubSpotr.Core.Model;
using System;
using System.Linq;

namespace HubSpotr.WindowsPhone.ViewModels
{
    public class PostViewModel
    {
        public int Id { get; set; }

        public string UserId { get; set; }

        public string UserName { get; set; }

        public string Message { get; set; }

        public DateTime At { get; set; }

        public int HubId { get; set; }

        public HubViewModel Hub
        {
            get
            {
                return App.Hubs.FirstOrDefault(h => h.Id == HubId);
            }
        }

        public Post Source { get; private set; }

        public PostViewModel(Post post)
        {
            this.Source = post;

            this.Id = post.Id;
            this.UserId = post.UserId;
            this.UserName = post.UserName;
            this.Message = post.Message;
            this.At = post.At;
            this.HubId = post.HubId;
        }

        // FOR DESIGN PURPOSES ONLY
        public PostViewModel()
        { }
    }
}
