using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ClientApp_Mobile.iOS.CustomRenderers;
using ClientApp_Mobile.Views;
using Foundation;
using UIKit;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;

[assembly: ExportRenderer(typeof(MainPage), typeof(ShellRendererNoDivideriOS))]
namespace ClientApp_Mobile.iOS.CustomRenderers
{
    public class ShellRendererNoDivideriOS : ShellRenderer
    {
        void OnElementSelected(Element element)
        {
            ((IShellController)this.Shell).OnFlyoutItemSelected(element);
        }

        IShellFlyoutContentRenderer shellFlyoutContentRenderer;
        protected override IShellFlyoutContentRenderer CreateShellFlyoutContentRenderer()
        {
            shellFlyoutContentRenderer = base.CreateShellFlyoutContentRenderer();

            if (shellFlyoutContentRenderer != null && shellFlyoutContentRenderer.ViewController != null)
            {
                var childs = shellFlyoutContentRenderer.ViewController.ChildViewControllers;
                if (childs != null && childs.Length > 0)
                {
                    var child = childs[0] as UITableViewController;
                    if (child != null && child.TableView != null)
                    {
                        child.TableView.ScrollEnabled = false;
                        child.TableView.RowHeight = 70;
                        child.TableView.Source = new MyCustomShellTableViewSource(this, OnElementSelected);
                    }
                }
            }

            return shellFlyoutContentRenderer;

        }
        public class MyCustomShellTableViewSource : ShellTableViewSource
        {
            public MyCustomShellTableViewSource(IShellContext context, Action<Element> onElementSelected) : base(context, onElementSelected)
            {
            }

            public override UIView GetViewForFooter(UITableView tableView, nint section)
            {
                return new SeparatorView();
            }
        }

        class SeparatorView : UIView
        {
            UIView _line;

            public SeparatorView()
            {
                _line = new UIView
                {
                    BackgroundColor = Xamarin.Forms.Color.Transparent.ToUIColor(),
                    TranslatesAutoresizingMaskIntoConstraints = true,
                    Alpha = 0.2f
                };

                Add(_line);
            }

            public override void LayoutSubviews()
            {
                _line.Frame = new CoreGraphics.CGRect(15, 0, Frame.Width - 30, 1);
                base.LayoutSubviews();
            }
        }
    }
}