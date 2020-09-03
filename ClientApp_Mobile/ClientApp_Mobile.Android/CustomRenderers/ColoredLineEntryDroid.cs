using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.Content.Res;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.Graphics.Drawables.Shapes;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using ClientApp_Mobile.Droid;
using ClientApp_Mobile.Droid.CustomRenderers;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;

[assembly: ExportRenderer(typeof(ClientApp_Mobile.Controls.ColoredLineEntry), typeof(ColoredLineEntryDroid))]
namespace ClientApp_Mobile.Droid.CustomRenderers
{
    public class ColoredLineEntryDroid : EntryRenderer
    {
        public ColoredLineEntryDroid(Context context) : base(context)
        {

        }
        protected override void OnElementChanged(ElementChangedEventArgs<Entry> e)
        {
            base.OnElementChanged(e);

            if (Control == null || e.NewElement == null) return;

            SetLineColor();
        }

        protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            base.OnElementPropertyChanged(sender, e);

            if (e.PropertyName == ClientApp_Mobile.Controls.ColoredLineEntry.BottomLineColorProperty.PropertyName)
                SetLineColor();
        }

        private void SetLineColor()
        {
            var color = (Element as Controls.ColoredLineEntry).BottomLineColor.ToAndroid();

            if (Build.VERSION.SdkInt >= BuildVersionCodes.Lollipop)
                Control.BackgroundTintList = ColorStateList.ValueOf(color);
            else
                Control.Background.SetColorFilter(color, PorterDuff.Mode.SrcAtop);
        }
    }
}