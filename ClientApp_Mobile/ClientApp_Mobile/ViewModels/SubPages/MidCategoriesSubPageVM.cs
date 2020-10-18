using ClientApp_Mobile.Services;
using Core.DBModels;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace ClientApp_Mobile.ViewModels.SubPages
{
    class MidCategoriesSubPageVM : BaseVM
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


        private TopCategory _selectedTopCategory;
        public TopCategory SelectedTopCategory
        {
            get { return _selectedTopCategory; }
            set
            {
                _selectedTopCategory = value;
                OnPropertyChanged("SelectedTopCategory");
            }
        }


        private List<MidCategory> _subCategories;
        public List<MidCategory> SubCategories
        {
            get { return _subCategories; }
            set
            {
                _subCategories = value;
                OnPropertyChanged("SubCategories");
            }
        }


        public Command CategorySelectedCommand { get; }

        public MidCategoriesSubPageVM(TopCategory selectedTopCategory)
        {
            SelectedTopCategory = selectedTopCategory;
            Title = selectedTopCategory.Name;
            CategorySelectedCommand = new Command(c => Task.Run(() => MidCategorySelected((MidCategory)c)));

            Task.Run(() => QueryDb());
        }

        private async void QueryDb()
        {
            IsBusy = true;
            try
            {
                SubCategories = (await ApiConnect.GetMidCategories(SelectedTopCategory.Id.ToString(), CTS.Token))
                                                 .OrderBy(c => c.Name).ToList();
                if (CTS.Token.IsCancellationRequested) { IsBusy = false; return; }
                SubCategories.Insert(0, new MidCategory { Name = "Все товары" });
                if (CTS.Token.IsCancellationRequested) { IsBusy = false; return; }
                IsBusy = false;
            }
            catch (OperationCanceledException)
            {
                IsBusy = false;
                return;
            }
        }

        private async void MidCategorySelected(MidCategory selectedMidCategory)
        {
            IsBusy = true;
            if (selectedMidCategory.Name == "Все товары")
            {
                ShowAllOffers();
            }
            else
            {
                if (CTS.IsCancellationRequested) { IsBusy = false; return; }
                List<ProductCategory> productCategories;
                try
                {
                    productCategories = await ApiConnect.GetProductCategoriesBasedOnMidCats(new List<Guid> { selectedMidCategory.Id }, CTS.Token);
                }
                catch (OperationCanceledException)
                {
                    IsBusy = false;
                    return;
                }

                if (CTS.IsCancellationRequested) { IsBusy = false; return; }

                IsBusy = false;

                if (productCategories.Count > 1)
                {
                    Device.BeginInvokeOnMainThread(() => ShellPageService.GotoProductCategoriesPage(selectedMidCategory, productCategories));
                }
                else
                {
                    if (productCategories != null)
                    {
                        Device.BeginInvokeOnMainThread(() => ShellPageService.GotoOffersPage(productCategories[0].Name, new List<Guid>() { productCategories[0].Id }));
                    }
                    else
                    {
                        ShowAllOffers();
                    }
                }
            }
        }

        private async void ShowAllOffers()
        {
            IsBusy = true;
            List<Guid> filterGuids;
            var allMidCategoriesGuids = SubCategories.Select(sc => sc.Id).ToList();

            try
            {
                filterGuids = await ApiConnect.GetProductCategoriesIdsBasedOnMidCats(allMidCategoriesGuids, CTS.Token);
            }
            catch (OperationCanceledException)
            {
                IsBusy = false;
                return;
            }

            Device.BeginInvokeOnMainThread(() => ShellPageService.GotoOffersPage(SelectedTopCategory.Name, filterGuids));
            IsBusy = false;
        }
    }
}
