using HubSpotr.Core.Model;
using System;
using System.Collections.Generic;

namespace HubSpotr.WindowsPhone.ViewModels
{
    public class UserViewModel : IEquatable<UserViewModel>
    {
        public string Id { get; set; }

        public string Name { get; set; }

        public string Email { get; set; }

        public int HubId { get; set; }

        public HubViewModel Hub { get; set; }

        public UserViewModel(User user)
        {
            this.Id = user.Id;
            this.HubId = user.HubId;
            this.Name = user.Name;
            this.Email = user.Email;

            if (HubId != 0)
                this.Hub = new HubViewModel(user.Hub);
        }

        // FOR DESIGN PURPOSES ONLY
        public UserViewModel()
        { }

        public bool Equals(UserViewModel other)
        {
            return this.Id.Equals(other.Id);
        }

        public override int GetHashCode()
        {
            return this.Id.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            var uvm = obj as UserViewModel;
            return uvm != null && this.Equals(uvm);
        }
    }

    public class UserViewModelCollection : List<UserViewModel>
    {
    }
}
