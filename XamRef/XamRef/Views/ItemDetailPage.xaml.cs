using System.ComponentModel;
using Xamarin.Forms;
using XamRef.ViewModels;

namespace XamRef.Views
{
    public partial class ItemDetailPage : ContentPage
    {
        public ItemDetailPage()
        {
            InitializeComponent();
            BindingContext = new ItemDetailViewModel();
        }
    }
}