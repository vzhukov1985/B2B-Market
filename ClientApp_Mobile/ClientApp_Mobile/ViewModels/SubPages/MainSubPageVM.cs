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
                HeightSuppliers = ((_suppliers.Count+1) * 65) + ((_suppliers.Count+1) * 1);
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


        private List<Guid> ContractedSuppliersIds;

        private void ShowSupplierProducts(Supplier selectedSupplier)
        {
            if (selectedSupplier.FullName == "Наши поставщики")
            {
                ShellPageService.GotoOffersPage("Наши поставщики", null, ContractedSuppliersIds);
            }
            else
            {
                ShellPageService.GotoOffersPage(selectedSupplier.FullName, null, new List<Guid>() { selectedSupplier.Id });
            }
        }

        private async void QueryDb()
        {
            IsBusy = true;
            try
            {
                ContractedSuppliersIds = new List<Guid>(User.Client.Contracts.Select(c => c.Supplier.Id));
                List<Supplier> unsortedSuppliersList;
                using (MarketDbContext db = new MarketDbContext())
                {
                    TopCategories = await db.TopCategories.AsNoTracking().ToListAsync();

                    unsortedSuppliersList = await db.Suppliers
                        .AsNoTracking()
                        .Where(s => s.IsActive == true)
                        .ToListAsync();
                }
                foreach (Supplier supplier in unsortedSuppliersList)
                    supplier.IsContractedWithClient = ContractedSuppliersIds.Contains(supplier.Id);
                Suppliers = unsortedSuppliersList.OrderByDescending(s => s.IsContractedWithClient).ThenBy(s => s.FullName).ToList(); ;
                Suppliers.Insert(0, new Supplier { IsContractedWithClient = true, FullName = "Наши поставщики", Id = Guid.Empty });
                IsBusy = false;
            }
            catch
            {
                Device.BeginInvokeOnMainThread(() => ShellDialogService.ShowConnectionErrorDlg());
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
