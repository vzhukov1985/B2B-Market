using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using ClientApp_Mobile.iOS.CustomRenderers;
using CoreAnimation;
using CoreGraphics;
using Foundation;
using UIKit;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;

[assembly: ExportRenderer(typeof(ClientApp_Mobile.Controls.ColoredLineEntry), typeof(ColoredLineEntryiOS))]
namespace ClientApp_Mobile.iOS.CustomRenderers
{
    public class ColoredLineEntryiOS : EntryRenderer
    {
        protected override void OnElementChanged(ElementChangedEventArgs<Entry> e)
        {
            base.OnElementChanged(e);
            if (Control != null)
            {
                // do whatever you want to the UITextField here!
                Control.BorderStyle = UITextBorderStyle.None;
            }
        }

        public override void Draw(CGRect rect)
        {
            base.Draw(rect);
            
            var view = (Element as Controls.ColoredLineEntry);
            if (view != null)
            {
                var borderLayer = new CALayer();
                borderLayer.MasksToBounds = true;
                borderLayer.Frame = new CGRect(3f, Frame.Height, Frame.Width-6f, 2f);
                borderLayer.BorderColor = view.BottomLineColor.ToCGColor();
                borderLayer.BorderWidth = 2.0f;

                Control.Layer.AddSublayer(borderLayer);
                Control.BorderStyle = UITextBorderStyle.None;

            }
        }

        protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            base.OnElementPropertyChanged(sender, e);

            if (e.PropertyName == ClientApp_Mobile.Controls.ColoredLineEntry.BottomLineColorProperty.PropertyName)
                SetNeedsDisplay();
        }
    }
}