using ClientApp_Mobile.Services;
using Core.DBModels;
using Core.Services;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace ClientApp_Mobile.ViewModels.SubPages
{
    class MainSubPageVM : BaseVM
    {
        private ClientUser _user;
        public ClientUser User
        {
            get { return _user; }
            set
            {
                _user = value;
                OnPropertyChanged("User");
            }
        }

        private List<MainListItems> _mainListItems;
        public List<MainListItems> MainListItems
        {
            get { return _mainListItems; }
            set
            {
                _mainListItems = value;
                OnPropertyChanged("MainListItems");
            }
        }

        private void ShowItem(MainListItem item)
        {
            switch (item.Type)
            {
                case MainListItemType.Category:
                    ShellPageService.GotoMidCategoriesPage(new TopCategory { Id = item.Id, Name = item.Name });
                    break;
                case MainListItemType.AllContractedSuppliers:
                    ShellPageService.GotoOffersPage("Наши поставщики", null, AppSettings.CurrentUser.Client.ContractedSuppliersIDs);
                    break;
                case MainListItemType.Supplier:
                    ShellPageService.GotoOffersPage(item.Name, null, new List<Guid>() { item.Id });
                    break;
            }
        }

        private void QueryDb()
        {
            IsBusy = true;
            try
            {
                MainListItems categories, suppliers;
                using (MarketDbContext db = new MarketDbContext())
                {
                    db.Database.OpenConnection();
                    categories = new MainListItems("КАТЕГОРИИ", (db.TopCategories
                                                                   .Select(tc => new MainListItem
                                                                   {
                                                                       Id = tc.Id,
                                                                       Name = tc.Name,
                                                                       Type = MainListItemType.Category,
                                                                       IsContracted = null
                                                                   })
                                                                   .ToList())
                                                                   .OrderBy(tc => tc.Name));

                    suppliers = new MainListItems("", db.Suppliers
                                                        .Where(s => s.IsActive == true)
                                                        .Select(s => new MainListItem
                                                        {
                                                            Id = s.Id,
                                                            Name = s.ShortName,
                                                            Type = MainListItemType.Supplier
                                                        })
                                                        .ToList());
                }


                foreach (var supplier in suppliers)
                    supplier.IsContracted = AppSettings.CurrentUser.Client.ContractedSuppliersIDs.Contains(supplier.Id);

                suppliers = new MainListItems("ПОСТАВЩИКИ", suppliers.OrderByDescending(s => s.IsContracted).ThenBy(s => s.Name));
                suppliers.Insert(0, new MainListItem { Id = Guid.Empty, Name = "Наши поставщики", IsContracted = true, Type = MainListItemType.AllContractedSuppliers });

                MainListItems = new List<MainListItems> { categories, suppliers };
                IsBusy = false;
            }
            catch
            {
                Device.BeginInvokeOnMainThread(() => DialogService.ShowConnectionErrorDlg());
                IsBusy = false;
                return;
            }
        }


        public Command ShowItemCommand { get; }


        public MainSubPageVM()
        {
            User = AppSettings.CurrentUser;
            MainListItems = new List<MainListItems>();

            ShowItemCommand = new Command<MainListItem>(i => ShowItem(i));

            Task.Run(() => QueryDb());
        }
    }




    enum MainListItemType
    {
        Category,
        AllContractedSuppliers,
        Supplier
    }
    class MainListItem
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public MainListItemType Type { get; set; }
        public bool? IsContracted { get; set; }
        public Uri PictureUri
        {
            get
            {
                switch (Type)
                {
                    case MainListItemType.Category:
                        return HTTPManager.GetTopCategoryPictureUri(Id);
                    case MainListItemType.AllContractedSuppliers:
                        return HTTPManager.GetSupplierPictureUri(Guid.Empty);
                    case MainListItemType.Supplier:
                        return HTTPManager.GetSupplierPictureUri(Id);
                    default:
                        return null;
                }
            }
        }
    }

    class MainListItems : List<MainListItem>
    {
        public string SectionName { get; set; }

        public MainListItems(string sectionName, IEnumerable<MainListItem> items) : base(items)
        {
            SectionName = sectionName;
        }
    }
}
