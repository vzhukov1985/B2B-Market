using ClientApp_Mobile.Services;
using Core.DBModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace ClientApp_Mobile.ViewModels.SubPages
{
    class ProductCategoriesSubPageVM : BaseVM
    {
        private string _title;
        public string Title
        {
            get { return _title; }
            set
            {
                _title = value;
                OnPropertyChanged("Title");
            }
        }

        private MidCategory _selectedMidCategory;
        public MidCategory SelectedMidCategory
        {
            get { return _selectedMidCategory; }
            set
            {
                _selectedMidCategory = value;
                OnPropertyChanged("SelectedMidCategory");
            }
        }

        private List<ProductCategory> _subCategories;
        public List<ProductCategory> SubCategories
        {
            get { return _subCategories; }
            set
            {
                _subCategories = value;
                OnPropertyChanged("SubCategories");
            }
        }

        private void ProductCategorySelected(ProductCategory selectedProductCategory)
        {
            List<Guid> filterGuids;
            
            if (selectedProductCategory.Name == "Все товары")
            {
                filterGuids = SubCategories.Select(sc => sc.Id).ToList();
                Device.BeginInvokeOnMainThread(() => ShellPageService.GotoOffersPage(SelectedMidCategory.Name, filterGuids));
            }
            else
            {
                filterGuids = new List<Guid> { selectedProductCategory.Id };
                Device.BeginInvokeOnMainThread(() => ShellPageService.GotoOffersPage(selectedProductCategory.Name, filterGuids));
            }
        }

        public Command CategorySelectedCommand { get; }

        public ProductCategoriesSubPageVM(MidCategory selectedMidCategory, List<ProductCategory> subCategories)
        {
            Task.Run(() =>
            {
                SelectedMidCategory = selectedMidCategory;
                Title = selectedMidCategory.Name;
                subCategories.Insert(0, new ProductCategory { Name = "Все товары" });
                SubCategories = subCategories;
            });

            CategorySelectedCommand = new Command(c => Task.Run(() => ProductCategorySelected((ProductCategory)c)));

        }

        public ProductCategoriesSubPageVM()
        {

        }
    }
}
