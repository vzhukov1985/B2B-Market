using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Text.Method;
using Android.Views;
using Android.Widget;
using ClientApp_Mobile.Droid.CustomRenderers;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;

[assembly: ExportRenderer(typeof(ClientApp_Mobile.Controls.DecimalEntry), typeof(DecimalEntryDroid))]
namespace ClientApp_Mobile.Droid.CustomRenderers
{
    class DecimalEntryDroid: EntryRenderer
    {
        public DecimalEntryDroid(Context context) : base(context)
        {

        }
        protected override void OnElementChanged(ElementChangedEventArgs<Entry> e)
        {
            base.OnElementChanged(e);

            if (Control != null)
            {
                this.Control.KeyListener = DigitsKeyListener.GetInstance("1234567890,");
                // this.Control.KeyListener = DigitsKeyListener.GetInstance(true, true); // I know this is deprecated, but haven't had time to test the code without this line, I assume it will work without
                this.Control.InputType = Android.Text.InputTypes.ClassNumber | Android.Text.InputTypes.NumberFlagDecimal;
            }
        }
    }
}