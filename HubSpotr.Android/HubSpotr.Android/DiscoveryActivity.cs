using Android.App;
using Android.Content;
using Android.Locations;
using Android.OS;
using Android.Views;
using Android.Widget;
using HubSpotr.Android.Adapters;
using HubSpotr.Core.Model;
using System.Linq;

namespace HubSpotr.Android
{
    [Activity(Label = "HubSpotr", Icon = "@drawable/ic_launcher")]
    public class DiscoveryActivity : Activity, ILocationListener
    {
        private HubAdapter hubAdapter;

        private LocationManager locationManager;
        private string locationProvider;

        private ProgressBar progress;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            SetContentView(Resource.Layout.HubDiscovery);

            this.progress = FindViewById<ProgressBar>(Resource.Id.discoveryProgress);
            this.progress.Visibility = ViewStates.Visible;

            InitializeAdapters();
        }

        protected override void OnResume()
        {
            base.OnResume();
            InitializeLocation();
        }

        protected override void OnPause()
        {
            base.OnPause();
            this.locationManager.RemoveUpdates(this);
        }

        private void InitializeLocation()
        {
            if(this.locationManager == null)
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

        private void InitializeAdapters()
        {
            ListView hubView = FindViewById<ListView>(Resource.Id.hubsView);

            this.hubAdapter = new HubAdapter(this, this.progress);
            hubView.Adapter = this.hubAdapter;

            hubView.ItemClick += HubSelected;
        }

        private void HubSelected(object sender, AdapterView.ItemClickEventArgs e)
        {
            CommonData.Hub = this.hubAdapter[e.Position];

            var hubActivity = new Intent(this, typeof(HubActivity));

            StartActivity(hubActivity);
        }

        #region LOCATION
        
        public void OnLocationChanged(Location location)
        {
            var referenceHub = new Hub 
            {
                Lat = location.Latitude,
                Lng = location.Longitude,
                Radius = 1 * 1000
            };

            this.hubAdapter.RefreshHubs(referenceHub, 5);
        }

        public void OnProviderDisabled(string provider)
        {
        }

        public void OnProviderEnabled(string provider)
        {
        }

        public void OnStatusChanged(string provider, Availability status, Bundle extras)
        {
        }

        #endregion
    }
}