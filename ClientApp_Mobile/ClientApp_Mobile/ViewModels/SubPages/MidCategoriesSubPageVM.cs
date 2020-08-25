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
using Xamarin.Forms;

namespace ClientApp_Mobile.ViewModels.SubPages
{
    public class MidCategoriesSubPageVM : BaseVM
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

        private ObservableCollection<MidCategory> _subCategories;
        public ObservableCollection<MidCategory> SubCategories
        {
            get { return _subCategories; }
            set
            {
                _subCategories = value;
                OnPropertyChanged("SubCategories");
            }
        }

        private async void ShowAllOffers()
        {
            IsBusy = true;
            try
            {
                List<Guid> filterGuids;
                var AllMidCategoriesGuids = SubCategories.Select(sc => sc.Id);
                using (MarketDbContext db = new MarketDbContext())
                {
                    filterGuids = await db.ProductCategories.AsNoTracking().Where(c => AllMidCategoriesGuids.Contains((Guid)c.MidCategoryId)).Select(c => c.Id).ToListAsync();
                }
                ShellPageService.GotoOffersPage(CategoryName, string.Join(",", filterGuids.Select(g => g.ToString()).ToArray()));
                IsBusy = false;
            }
            catch
            {
                ShellDialogService.ShowConnectionErrorDlg();
                IsBusy = false;
                return;
            }
        }

        private async void CategorySelected(MidCategory selectedCategory)
        {
            if (selectedCategory.Name == "Все товары")
            {
                ShowAllOffers();
            }
            else
            {
                using (MarketDbContext db = new MarketDbContext())
                {
                    var productCategories = await db.ProductCategories.Where(c => c.MidCategoryId == selectedCategory.Id).ToListAsync();
                    if ( productCategories.Count > 1)
                    {
                        ShellPageService.GotoProductCategoriesPage(selectedCategory.Id.ToString(), selectedCategory.Name);
                    }
                    else
                    {
                        if (productCategories != null)
                        {
                            ShellPageService.GotoOffersPage(productCategories[0].Name, productCategories[0].Id.ToString());
                        }
                        else
                        {
                            ShowAllOffers();
                        }
                    }
                }
                
            }
        }

        private async void QueryDb()
        {
            IsBusy = true;
            try
            {
                using (MarketDbContext db = new MarketDbContext())
                    SubCategories = new ObservableCollection<MidCategory>(await db.MidCategories.AsNoTracking().Where(c => c.TopCategoryId == CategoryId).OrderBy(c => c.Name).ToListAsync());

                SubCategories.Insert(0, new MidCategory { Name = "Все товары" });
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

        public MidCategoriesSubPageVM(string topCategoryId, string topCategoryName)
        {
            CategoryName = Uri.UnescapeDataString(topCategoryName ?? string.Empty);
            CategoryId = new Guid(topCategoryId);

            CategorySelectedCommand = new Command(c => CategorySelected((MidCategory)c));

            QueryDb();
        }

        public MidCategoriesSubPageVM()
        {

        }
    }
}
