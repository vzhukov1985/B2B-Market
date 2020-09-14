using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using ClientApp_Mobile.Controls;
using ClientApp_Mobile.iOS.CustomRenderers;
using Foundation;
using UIKit;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;

[assembly: ExportRenderer(typeof(DecimalEntry), typeof(DecimalEntryiOS))]
namespace ClientApp_Mobile.iOS.CustomRenderers
{
    public class DecimalEntryiOS: EntryRenderer
    {
        protected override void OnElementChanged(ElementChangedEventArgs<Entry> e)
        {
            base.OnElementChanged(e);

            DecimalEntry myAppEntry = Element as DecimalEntry;
            UITextField nativeTextField = Control;

            // Control is the native control, whereas Element is the Xamarin.Forms view

            if (myAppEntry != null)
            {
                if (((myAppEntry.Keyboard == Keyboard.Numeric)
                    || (myAppEntry.Keyboard == Keyboard.Telephone))
                    && (Device.Idiom == TargetIdiom.Phone))
                {
                    UIToolbar toolbar =
                        new UIToolbar(new RectangleF(0.0f, 0.0f, (float)Control.Frame.Size.Width, 44.0f));

                    var doneButton = new UIBarButtonItem(UIBarButtonSystemItem.Done, delegate
                    {
                        this.Control.ResignFirstResponder();
                        IEntryController controller = (IEntryController)Element;
                        controller?.SendCompleted();
                    });

                    toolbar.Items = new UIBarButtonItem[]
                    {
                    new UIBarButtonItem(UIBarButtonSystemItem.FlexibleSpace), doneButton
                    };
                    this.Control.InputAccessoryView = toolbar;
                }
            }
        }
    }
}