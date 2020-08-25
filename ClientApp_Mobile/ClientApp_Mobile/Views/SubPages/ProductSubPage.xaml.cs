using ClientApp_Mobile.ViewModels.SubPages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace ClientApp_Mobile.Views.SubPages
{
    [QueryProperty("ProductIdParam", "id")]
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ProductSubPage : ContentPage
    {
        private string _productIdParam;
        public string ProductIdParam
        {
            get { return _productIdParam; }
            set
            {
                _productIdParam = string.IsNullOrEmpty(value) ? "" : Uri.UnescapeDataString(value);
            }
        }


        public ProductSubPage()
        {
            InitializeComponent();
            BindingContext = new ProductSubPageVM();
            ((ProductSubPageVM)BindingContext).QueryDB(new Guid("7c8bad17-7350-4f56-b753-320a422cfd9b"));//TODO:DeleteAfterDesign
        }

        private void ContentPage_Appearing(object sender, EventArgs e)
        {
            //TODO:RemoveContentsAfterDesign
           /* ProductSubPageVM bc = (ProductSubPageVM)BindingContext;
            bc.QueryDB(new Guid(ProductIdParam));*/ 
        }
    }
}