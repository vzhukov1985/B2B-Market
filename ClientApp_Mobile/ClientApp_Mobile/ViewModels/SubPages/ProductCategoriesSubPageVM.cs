using ClientApp_Mobile.Services;
using Core.DBModels;
using Core.Services;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace ClientApp_Mobile.ViewModels.SubPages
{
    public class ProductCategoriesSubPageVM : BaseVM
    {
        private Guid _categoryId;
        public Guid CategoryId
        {
            get { return _categoryId; }
            set
            {
                _categoryId = value;
                OnPropertyChanged("CategoryId");
            }
        }

        private string _categoryName;
        public string CategoryName
        {
            get { return _categoryName; }
            set
            {
                _categoryName = value;
                OnPropertyChanged("CategoryName");
            }
        }

        private Product _selectedCategory;
        public Product SelectedCategory
        {
            get { return _selectedCategory; }
            set
            {
                _selectedCategory = value;
                if (value != null)
                    CategorySelectedCommand.Execute(value);
                OnPropertyChanged("SelectedCategory");
            }
        }

        private ObservableCollection<ProductCategory> _subCategories;
        public ObservableCollection<ProductCategory> SubCategories
        {
            get { return _subCategories; }
            set
            {
                _subCategories = value;
                OnPropertyChanged("SubCategories");
            }
        }

        private void CategorySelected(ProductCategory selectedCategory)
        {
            List<Guid> filterGuids;
            
            if (selectedCategory.Name == "Все товары")
            {
                filterGuids = SubCategories.Select(sc => sc.Id).ToList();
                ShellPageService.GotoOffersPage(CategoryName, string.Join(",", filterGuids.Select(g => g.ToString()).ToArray()));
            }
            else
            {
                filterGuids = new List<Guid> { selectedCategory.Id };
                ShellPageService.GotoOffersPage(selectedCategory.Name, string.Join(",", filterGuids.Select(g => g.ToString()).ToArray()));
            }
        }

        private async void QueryDb()
        {
            IsBusy = true;
            try
            {
                using (MarketDbContext db = new MarketDbContext())
                    SubCategories = new ObservableCollection<ProductCategory>(await db.ProductCategories.Where(c => c.MidCategoryId == CategoryId).OrderBy(c => c.Name).ToListAsync());

                SubCategories.Insert(0, new ProductCategory { Name = "Все товары" });
                IsBusy = false;
            }
            catch
            {
                ShellDialogService.ShowConnectionErrorDlg();
                IsBusy = false;
                return;
            }
        }

        public Command CategorySelectedCommand { get; }

        public ProductCategoriesSubPageVM(string midCategoryId, string midCategoryName)
        {
            CategoryName = Uri.UnescapeDataString(midCategoryName ?? string.Empty);
            CategoryId = new Guid(midCategoryId);

            CategorySelectedCommand = new Command(c => CategorySelected((ProductCategory)c));

            QueryDb();
        }

        public ProductCategoriesSubPageVM()
        {

        }
    }
}
