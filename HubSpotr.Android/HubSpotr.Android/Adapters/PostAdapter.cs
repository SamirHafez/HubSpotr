using Android.App;
using Android.Graphics;
using Android.Views;
using Android.Widget;
using HubSpotr.Core.Extensions;
using HubSpotr.Core.Model;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Timers;

namespace HubSpotr.Android.Adapters
{
    class PostAdapter : BaseAdapter
    {
        private Activity activity;
        private List<Post> posts;
        private Hub hub;

        public Timer timer;

        public PostAdapter(Activity activity, Hub hub)
        {
            this.activity = activity;
            this.posts = new List<Post>();
            this.hub = hub;

            timer = new Timer();
            timer.Interval = 2000;
            timer.Elapsed += OnTimedEvent;

            timer.Start();
        }

        private async void OnTimedEvent(object sender, ElapsedEventArgs e)
        {
            Post latest = this.posts.OrderByDescending(p => p.At).FirstOrDefault();

            timer.Stop();

            if (latest != null)
            {
                List<Post> newPosts = await this.hub.NewPosts(posts.OrderByDescending(p => p.At).First().At);

                if (newPosts.Count > 0)
                {
                    this.posts.InsertRange(0, newPosts);
                    this.NotifyDataSetChanged();
                }

                timer.Start();
            }
            else
            {
                List<Post> postList = await this.hub.Posts();

                if (postList.Count > 0)
                {
                    this.posts.InsertRange(0, postList);
                    this.NotifyDataSetChanged();
                }

                timer.Start();
            }
        }

        public Task Add(Post post)
        {
            return post.Post();
        }

        public override int Count
        {
            get { return this.posts.Count; }
        }

        public override long GetItemId(int position)
        {
            return this.posts[position].Id;
        }

        public override Java.Lang.Object GetItem(int position)
        {
            return null;
        }

        public override View GetView(int position, View convertView, ViewGroup parent)
        {
            View view = convertView ?? this.activity.LayoutInflater.Inflate(Resource.Layout.PostItem, parent, false);

            Post post = this.posts[position];

            TextView postItemOwnerView = view.FindViewById<TextView>(Resource.Id.postItemOwner);
            TextView postItemView = view.FindViewById<TextView>(Resource.Id.postItemMessage);
            TextView postDateView = view.FindViewById<TextView>(Resource.Id.postItemDate);
            ImageView pictureView = view.FindViewById<ImageView>(Resource.Id.postItemPicture);

            postItemOwnerView.Text = post.Name;
            postItemView.Text = post.Message;
            postDateView.Text = post.At.ToShortDateString();

            SetImageBitmapFromUrl(pictureView, post.Picture);

            return view;
        }

        private async void SetImageBitmapFromUrl(ImageView imageView, string url)
        {
            byte[] imageBytes;
            using (var webClient = new WebClient())
                imageBytes = await webClient.DownloadDataTaskAsync(url);

            imageView.SetImageBitmap(await BitmapFactory.DecodeByteArrayAsync(imageBytes, 0, imageBytes.Length));
        }
    }
}