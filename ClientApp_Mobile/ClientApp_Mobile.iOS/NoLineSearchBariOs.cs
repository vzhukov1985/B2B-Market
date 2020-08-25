using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ClientApp_Mobile.iOS;
using Foundation;
using UIKit;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;

[assembly: ExportRenderer(typeof(ClientApp_Mobile.Renderers.NoLineSearchBar), typeof(NoLineSearchBariOs))]
namespace ClientApp_Mobile.iOS
{
    public class NoLineSearchBariOs: SearchBarRenderer
    {
        protected override void OnElementChanged(ElementChangedEventArgs<SearchBar> e)
        {
            base.OnElementChanged(e);

            if (UIDevice.CurrentDevice.CheckSystemVersion(13, 0) && Control != null)
            {
                Control.SearchTextField.BackgroundColor = UIColor.FromRGB(255, 255, 255);
            }
        }
    }
}