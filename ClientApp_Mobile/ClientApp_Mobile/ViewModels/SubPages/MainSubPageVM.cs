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
    public class MainSubPageVM : BaseVM
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

        private List<TopCategory> _topCategories;
        public List<TopCategory> TopCategories
        {
            get { return _topCategories; }
            set
            {
                _topCategories = value;
                if (_topCategories != null)
                    HeightCategories = (_topCategories.Count * 65) + (_topCategories.Count * 1);
                OnPropertyChanged("TopCategories");
            }
        }

        private List<Supplier> _suppliers;
        public List<Supplier> Suppliers
        {
            get { return _suppliers; }
            set
            {
                _suppliers = value;
                if (_suppliers != null)
                    HeightSuppliers = ((_suppliers.Count + 1) * 65) + ((_suppliers.Count + 1) * 1);
                OnPropertyChanged("Suppliers");
            }
        }

        private double _heightCategories;
        public double HeightCategories
        {
            get { return _heightCategories; }
            set
            {
                _heightCategories = value;
                OnPropertyChanged("HeightCategories");
            }
        }

        private double _heightSuppliers;
        public double HeightSuppliers
        {
            get { return _heightSuppliers; }
            set
            {
                _heightSuppliers = value;
                OnPropertyChanged("HeightSuppliers");
            }
        }


        private void ShowSupplierProducts(Supplier selectedSupplier)
        {
            if (selectedSupplier.ShortName == "Наши поставщики")
            {
                ShellPageService.GotoOffersPage("Наши поставщики", null, UserService.CurrentUser.Client.ContractedSuppliersIDs);
            }
            else
            {
                ShellPageService.GotoOffersPage(selectedSupplier.ShortName, null, new List<Guid>() { selectedSupplier.Id });
            }
        }

        private async void QueryDb()
        {
            IsBusy = true;
            try
            {
                List<Supplier> unsortedSuppliersList;

                using (MarketDbContext db = new MarketDbContext())
                {
                    TopCategories = await db.TopCategories.AsNoTracking().ToListAsync();

                    unsortedSuppliersList = await db.Suppliers
                        .AsNoTracking()
                        .Select(s => new Supplier { Id = s.Id, ShortName = s.ShortName, IsActive = s.IsActive })
                        .Where(s => s.IsActive == true)
                        .ToListAsync();
                }

                foreach (Supplier supplier in unsortedSuppliersList)
                    supplier.IsContractedWithClient = UserService.CurrentUser.Client.ContractedSuppliersIDs.Contains(supplier.Id);

                Suppliers = unsortedSuppliersList.OrderByDescending(s => s.IsContractedWithClient).ThenBy(s => s.ShortName).ToList(); ;
                Suppliers.Insert(0, new Supplier { IsContractedWithClient = true, ShortName = "Наши поставщики", Id = Guid.Empty });
                IsBusy = false;
            }
            catch
            {
                Device.BeginInvokeOnMainThread(() => DialogService.ShowConnectionErrorDlg());
                IsBusy = false;
                return;
            }
        }


        public Command ShowMidCategoriesCommand { get; }
        public Command ShowSupplierProductsCommand { get; }


        public MainSubPageVM()
        {
            User = UserService.CurrentUser;

            ShowMidCategoriesCommand = new Command<TopCategory>(c => ShellPageService.GotoMidCategoriesPage(c));
            ShowSupplierProductsCommand = new Command(s => ShowSupplierProducts((Supplier)s));

            Task.Run(() => QueryDb());
        }

    }
}
