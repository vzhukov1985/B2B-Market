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
    public partial class MidCategoriesSubPage : ContentPage
    {
        public string CategoryIdParam { 
            get => categoryIdParam;
            set { 

                categoryIdParam = value; 
            }
        }

        private string _categoryNameParam;
        private string categoryIdParam;

        public string CategoryNameParam
        {
            get => _categoryNameParam;
            set
            {
                BindingContext = new MidCategoriesSubPageVM(CategoryIdParam, value);
                _categoryNameParam = value;
            }
        }

        public MidCategoriesSubPage()
        {
            InitializeComponent();
        }
    }
}