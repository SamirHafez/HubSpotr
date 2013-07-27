using Android.App;
using Android.Views;
using Android.Widget;
using HubSpotr.Core.Extensions;
using HubSpotr.Core.Model;
using System.Collections.Generic;

namespace HubSpotr.Android.Adapters
{
    public class HubAdapter : BaseAdapter
    {
        private Activity activity;
        private List<Hub> hubs;
        private ProgressBar progress;

        public Hub this[int position]
        {
            get { return this.hubs[position]; }
        }

        public HubAdapter(Activity activity, ProgressBar progress)
        {
            this.activity = activity;
            this.progress = progress;
            this.hubs = new List<Hub>();
        }

        public async void RefreshHubs(Hub reference, int quantity)
        {
            progress.Visibility = ViewStates.Visible;

            List<Hub> nearHubs = await reference.NearHubs(quantity);

            this.hubs.Clear();
            this.hubs.AddRange(nearHubs);

            progress.Visibility = ViewStates.Gone;

            this.NotifyDataSetChanged();
        }

        public override int Count
        {
            get { return this.hubs.Count; }
        }

        public override long GetItemId(int position)
        {
            return this.hubs[position].Id;
        }

        public override Java.Lang.Object GetItem(int position)
        {
            return null;
        }

        public override View GetView(int position, View convertView, ViewGroup parent)
        {
            View view = convertView ?? this.activity.LayoutInflater.Inflate(Resource.Layout.HubItem, parent, false);

            Hub hub = this.hubs[position];

            TextView hubNameView = view.FindViewById<TextView>(Resource.Id.hubItemName);
            TextView hubParticipantsView = view.FindViewById<TextView>(Resource.Id.hubItemParticipants);

            hubNameView.Text = hub.Name;
            hubParticipantsView.Text = string.Format("{0} Participant(s)", hub.Participants);

            return view;
        }
    }
}