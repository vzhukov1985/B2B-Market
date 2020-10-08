using System;
using ClientApp_Mobile.Controls;
using ClientApp_Mobile.iOS.CustomRenderers;
using Foundation;
using UIKit;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;

[assembly: ExportRenderer(typeof(SwipeBackContentPage), typeof(PageRendereriOS))]
namespace ClientApp_Mobile.iOS.CustomRenderers
{
    public class PageRendereriOS : PageRenderer
    {
        public override void ViewDidAppear(bool animated)
        {
            base.ViewDidAppear(animated);

            if (ViewController.NavigationController == null)
                return;

            ViewController.NavigationController.InteractivePopGestureRecognizer.Enabled = true;

            ViewController.NavigationController.InteractivePopGestureRecognizer.Delegate = new UIGestureRecognizerDelegate();
            ViewController.NavigationController.InteractivePopGestureRecognizer
                .AddTarget(this, new ObjCRuntime.Selector(nameof(HandleBackSwipe)));
        }

        [Export(nameof(HandleBackSwipe))]
        private void HandleBackSwipe()
        {
            if (ViewController.NavigationController == null)
                return;

            ViewController.NavigationController.InteractivePopGestureRecognizer
                .RemoveTarget(this, new ObjCRuntime.Selector(nameof(HandleBackSwipe)));

           // ContentPageElement.SendBackButtonPressed();
        }
    }
}

