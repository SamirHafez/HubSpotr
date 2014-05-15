using HubSpotr.WindowsPhone.Core.Model;
using System;

namespace HubSpotr.WindowsPhone.ViewModels
{
    public class UserViewModel : IEquatable<UserViewModel>
    {
        public string Id { get; set; }

        public UserViewModel(User user)
        {
            this.Id = user.Id;
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
}
