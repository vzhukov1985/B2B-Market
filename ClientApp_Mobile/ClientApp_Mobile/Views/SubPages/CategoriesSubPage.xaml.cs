using ClientApp_Mobile.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace ClientApp_Mobile.Views.SubPages
{
    [QueryProperty("Category", "categoriesType")]
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class CategoriesSubPage : ContentPage
    {
        private string _category;
        public string Category
        {
            get => _category;
            set
            {
                if (value == "MidCategories")
                {
                    BindingContext = new MidCategoriesSubPageVM();
                }
                _category = value;
            }
        }

        public CategoriesSubPage()
        {
            InitializeComponent();
        }
    }
}