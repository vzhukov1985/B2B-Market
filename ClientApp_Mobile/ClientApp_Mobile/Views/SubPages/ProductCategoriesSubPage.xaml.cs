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
    [QueryProperty("CategoryIdParam", "categoryId")]
    [QueryProperty("CategoryNameParam", "categoryName")]
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ProductCategoriesSubPage : ContentPage
    {
        public string CategoryIdParam { get; set; }

        private string _categoryNameParam;
        public string CategoryNameParam
        {
            get => _categoryNameParam;
            set
            {
                BindingContext = new ProductCategoriesSubPageVM(CategoryIdParam, value);
                _categoryNameParam = value;
            }
        }
        public ProductCategoriesSubPage()
        {
            InitializeComponent();
        }
    }
}