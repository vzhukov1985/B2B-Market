using System;

using Android.App;
using Android.Content.PM;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using Android;

namespace ClientApp_Mobile.Droid
{
    [Activity(Label = "B2B Market HoReCa", Icon = "@mipmap/icon", Theme = "@style/MainTheme", MainLauncher = false, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize, ScreenOrientation = ScreenOrientation.Portrait )]
    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity
    {
        public static Activity FormsContext { get; set; }
        protected override void OnCreate(Bundle savedInstanceState)
        {
            TabLayoutResource = Resource.Layout.Tabbar;
            ToolbarResource = Resource.Layout.Toolbar;

            base.OnCreate(savedInstanceState);

            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            global::Xamarin.Forms.Forms.Init(this, savedInstanceState);
            LoadApplication(new App());
        }
        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
        {
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
            
            if (requestCode == 202)
            {

                if ((grantResults.Length == 1) && (grantResults[0] == Permission.Granted))
                {

                }
                else
                {
                    if (Build.VERSION.SdkInt >= BuildVersionCodes.P)
                    {
                        string[] reruiredPermission = new string[] { Manifest.Permission.UseBiometric };
                    }
                    else
                    {
#pragma warning disable CS0618 // Type or member is obsolete
                        string[] reruiredPermission = new string[] { Manifest.Permission.UseFingerprint };
#pragma warning restore CS0618 // Type or member is obsolete
                    }
                }
            }
        }
    }
}