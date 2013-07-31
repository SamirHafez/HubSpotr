using Android.App;
using Android.Locations;
using Android.OS;
using Android.Widget;
using HubSpotr.Android.Adapters;
using HubSpotr.Core.Extensions;
using HubSpotr.Core.Model;
using System.Linq;
using System.Timers;

namespace HubSpotr.Android
{
    [Activity(Label = "HubSpotr", Icon = "@drawable/ic_launcher")]
    public class HubActivity : Activity, ILocationListener
    {
        private PostAdapter postAdapter;

        private LocationManager locationManager;
        private string locationProvider;

        protected async override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            SetContentView(Resource.Layout.Hub);

            try
            {
                await CommonData.Hub.Join();
                JoinSucceded();
            }
            catch
            {
                JoinFailed();
            }
        }

        protected override void OnResume()
        {
            base.OnResume();

            InitializeLocation();
            //this.postAdapter.timer.Start();
        }

        protected override void OnPause()
        {
            base.OnPause();

            this.locationManager.RemoveUpdates(this);
            //this.postAdapter.timer.Stop();
        }

        public override void OnBackPressed()
        {
            new AlertDialog.Builder(this)
                           .SetTitle("Leaving")
                           .SetMessage("You are about to leave this hub")
                           .SetPositiveButton("OK", async (o, e) => 
                           {
                               await CommonData.Hub.Leave();
                               this.Finish(); 
                           })
                           .SetNegativeButton("Cancel", (o, e) => { })
                           .Show();
        }

        private void JoinSucceded()
        {
            InitializeHubView();
            InitializeAdapters();
            InitializeInput();
        }

        private void JoinFailed()
        {
            new AlertDialog.Builder(this)
                               .SetTitle("Failed to join")
                               .SetMessage("Please try again")
                               .SetCancelable(false)
                               .SetPositiveButton("Leave", (o, e) => this.Finish())
                               .Show();
        }

        #region INITIALIZATION

        private void InitializeAdapters()
        {
            ListView postsView = FindViewById<ListView>(Resource.Id.postsView);

            this.postAdapter = new PostAdapter(this, CommonData.Hub);
            postsView.Adapter = this.postAdapter;
        }

        private void InitializeInput()
        {
            EditText messageBox = FindViewById<EditText>(Resource.Id.postText);
            Button postButton = FindViewById<Button>(Resource.Id.postButton);

            postButton.Click += delegate
            {
                var post = new Post
                {
                    Message = messageBox.Text.Trim(),
                    HubId = CommonData.Hub.Id
                };

                if (post.Message == string.Empty)
                    return;

                this.postAdapter.Add(post);

                messageBox.Text = string.Empty;
            };
        }

        private void InitializeLocation()
        {
            if (this.locationManager == null)
                this.locationManager = (LocationManager)GetSystemService(LocationService);
            var criteriaForLocationService = new Criteria
            {
                CostAllowed = true,
                SpeedRequired = false,
                Accuracy = Accuracy.NoRequirement
            };
            var acceptableLocationProviders = this.locationManager.GetProviders(criteriaForLocationService, true);

            this.locationProvider = acceptableLocationProviders.FirstOrDefault() ?? string.Empty;

            this.locationManager.RequestLocationUpdates(this.locationProvider, 0, 0, this);
        }

        private void InitializeHubView()
        {
            TextView titleView = FindViewById<TextView>(Resource.Id.hubNameView);

            titleView.Text = CommonData.Hub.Name;
        }

        #endregion

        #region LOCATION

        public void OnLocationChanged(Location currentLocation)
        {
            float distanceToHubCenter = currentLocation.DistanceTo(new Location(this.locationProvider)
            {
                Latitude = CommonData.Hub.Lat,
                Longitude = CommonData.Hub.Lng
            });

            if (distanceToHubCenter > CommonData.Hub.Radius)
                new AlertDialog.Builder(this)
                               .SetTitle("Sorry")
                               .SetMessage("You fell out of this hubs range")
                               .SetCancelable(false)
                               .SetPositiveButton("Leave", async (o, e) =>
                                   {
                                       await CommonData.Hub.Leave();
                                       this.Finish();
                                   })
                               .Show();
        }

        public void OnProviderDisabled(string provider)
        {
        }

        public void OnProviderEnabled(string provider)
        {
        }

        public void OnStatusChanged(string provider, Availability status, Bundle extras)
        { }

        #endregion
    }
}

