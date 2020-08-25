using ClientApp_Mobile.ViewModels;
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
    [QueryProperty("TitleParam", "title")]
    [QueryProperty("CategoryFilterParam", "catFilter")]
    [QueryProperty("SuppliersFilterParam", "supFilter")]
    [QueryProperty("SearchTextParam", "search")]
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class OffersSubPage : ContentPage
    {
        private string _titleParam;
        public string TitleParam
        {
            get { return _titleParam; }
            set
            {
                _titleParam = string.IsNullOrEmpty(value) ? "" : Uri.UnescapeDataString(value);
            }
        }

        private string _supplierFilterParam;
        public string SuppliersFilterParam
        {
            get { return _supplierFilterParam; }
            set
            {
                _supplierFilterParam = string.IsNullOrEmpty(value) ? "" : Uri.UnescapeDataString(value);
            }
        }

        private string _categoryFilterParam;
        public string CategoryFilterParam
        {
            get { return _categoryFilterParam; }
            set
            {
                _categoryFilterParam = string.IsNullOrEmpty(value) ? "" : Uri.UnescapeDataString(value);
            }
        }

        private string _searchTextParam;
        public string SearchTextParam
        {
            get { return _searchTextParam; }
            set
            {
                _searchTextParam = string.IsNullOrEmpty(value) ? "" : Uri.UnescapeDataString(value); ;
            }
        }

        public static readonly BindableProperty ShowFavoritesOnlyProp = BindableProperty.Create(nameof(ShowFavoritesOnly), typeof(bool), typeof(OffersSubPage), false);
        public bool ShowFavoritesOnly
        {
            get { return (bool)GetValue(ShowFavoritesOnlyProp); }
            set
            {
                TitleParam = "Избранное";
                SetValue(ShowFavoritesOnlyProp, value);
            }
        }

        public OffersSubPage()
        {
            InitializeComponent();
            BindingContext = new OffersSubPageVM();
        }

        private void ContentPage_Appearing(object sender, EventArgs e)
        {
            if (ShowFavoritesOnly)
            {
                List<Guid> parsedCategoryFilter = string.IsNullOrEmpty(CategoryFilterParam) ? null : CategoryFilterParam.Split(',').Select(s => new Guid(s)).ToList();
                List<Guid> parsedSupplierFilter = string.IsNullOrEmpty(SuppliersFilterParam) ? null : SuppliersFilterParam.Split(',').Select(s => new Guid(s)).ToList();

                OffersSubPageVM bc = (OffersSubPageVM)BindingContext;

                bc.Title = TitleParam;
                bc.QueryDb(ShowFavoritesOnly, parsedCategoryFilter, parsedSupplierFilter, SearchTextParam);
            }
        }
    }
}