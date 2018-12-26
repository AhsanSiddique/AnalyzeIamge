using Android.App;
using Android.Graphics;
using Android.OS;
using Android.Support.V7.App;
using Android.Views;
using Android.Widget;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace ImageAnalyze
{
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme", MainLauncher = true)]
    public class MainActivity : AppCompatActivity
    {
        const string subscriptionKey = "3407ad6140b240f58847194ebf0dc26d";
        const string uriBase = "https://westcentralus.api.cognitive.microsoft.com/vision/v2.0/analyze";
        Bitmap mBitmap;
        private ImageView imageView;
        private ProgressBar progressBar;
        ByteArrayContent content;
        private TextView textView;
        Button btnAnalyze;
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            
            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.activity_main);
            
            mBitmap = BitmapFactory.DecodeResource(Resources, Resource.Drawable.myPic);
            imageView = FindViewById<ImageView>(Resource.Id.imgView);
            imageView.SetImageBitmap(mBitmap);
            textView = FindViewById<TextView>(Resource.Id.txtDescription);
            progressBar = FindViewById<ProgressBar>(Resource.Id.progressBar);
            btnAnalyze = FindViewById<Button>(Resource.Id.btnAnalyze);
            byte[] bitmapData;
            using (var stream = new MemoryStream())
            {
                mBitmap.Compress(Bitmap.CompressFormat.Jpeg, 100, stream);
                bitmapData = stream.ToArray();
            }
            content = new ByteArrayContent(bitmapData);

            btnAnalyze.Click += async delegate
            {
                busy();
               await MakeAnalysisRequest(content);
            };           
    }
        public async Task MakeAnalysisRequest(ByteArrayContent content)
        {
            try
            {
                HttpClient client = new HttpClient();

                // Request headers.
                client.DefaultRequestHeaders.Add(
                    "Ocp-Apim-Subscription-Key", subscriptionKey);

                string requestParameters =
                    "visualFeatures=Description&details=Landmarks&language=en";

                // Assemble the URI for the REST API method.
                string uri = uriBase + "?" + requestParameters;

                content.Headers.ContentType =
                    new MediaTypeHeaderValue("application/octet-stream");
                
                // Asynchronously call the REST API method.
                var response = await client.PostAsync(uri, content);

                // Asynchronously get the JSON response.
                

                string contentString = await response.Content.ReadAsStringAsync();

                var analysesResult = JsonConvert.DeserializeObject<AnalysisModel>(contentString);
                NotBusy();
                textView.Text = analysesResult.description.captions[0].text.ToString();
            }
            catch (Exception e)
            {
                Toast.MakeText(this, "" + e.ToString(), ToastLength.Short).Show();
            }
        }

        void busy()
        {
            progressBar.Visibility = ViewStates.Visible;
            btnAnalyze.Enabled = false;
        }

        void NotBusy()
        {
            progressBar.Visibility = ViewStates.Invisible;
            btnAnalyze.Enabled = true;
        }
    }
}

