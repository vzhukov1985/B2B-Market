using ClientApp.Services;
using Core.Models;
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

namespace ClientApp.ViewModels
{
    public class MainSubPageVM<CommandType> : INotifyPropertyChanged where CommandType : IRelayCommand, new()
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName] string prop = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        }

        private readonly IPageService PageService;

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

        private ObservableCollection<TopCategory> _topCategories;
        public ObservableCollection<TopCategory> TopCategories
        {
            get { return _topCategories; }
            set
            {
                _topCategories = value;
                OnPropertyChanged("TopCategories");
            }
        }

        private ObservableCollection<Supplier> _suppliers;
        public ObservableCollection<Supplier> Suppliers
        {
            get { return _suppliers; }
            set
            {
                _suppliers = value;
                OnPropertyChanged("Suppliers");
            }
        }

        private void ShowSupplierProducts(Supplier selectedSupplier)
        {
            List<Guid> supplierFilter = new List<Guid>();
            supplierFilter.Add(selectedSupplier.Id);
            PageService.ShowOffersSubPage(User, selectedSupplier.FullName, null, supplierFilter);
        }

        private void ShowContractedSuppliersProducts()
        {
            PageService.ShowOffersSubPage(User, ClientAppResourceManager.GetString("UI_MainSubPage_AllContractedSuppliers"), null, ContractedSuppliersIds);
        }

        private async void LoadTopCategoriesAndSuppliersAsync()
        {
            using (MarketDbContext db = new MarketDbContext())
            {
                TopCategories = new ObservableCollection<TopCategory>(await db.TopCategories.ToListAsync());

                Suppliers = new ObservableCollection<Supplier>(await db.Suppliers
                    .OrderByDescending(s => ContractedSuppliersIds.Count == 0 || ContractedSuppliersIds.Contains(s.Id))
                    .ThenBy(s => s.FullName)
                    .ToListAsync()); ;
            }
            foreach (Supplier supplier in Suppliers)
                supplier.IsContractedWithClient = ContractedSuppliersIds.Contains(supplier.Id);
        }

        public CommandType ShowSearchSubPageCommand { get; }
        public CommandType ShowMidCategoriesCommand { get; }
        public CommandType ShowSupplierProductsCommand { get; }
        public CommandType ShowContractedSuppliersProductsCommand { get; }

        private List<Guid> ContractedSuppliersIds;

        public MainSubPageVM(ClientUser user, IPageService pageService)
        {
            PageService = pageService;
            User = user;

            ShowSearchSubPageCommand = new CommandType();
            ShowSearchSubPageCommand.Create(_ => PageService.ShowSearchSubPage(User));
            ShowMidCategoriesCommand = new CommandType();
            ShowMidCategoriesCommand.Create(c => PageService.ShowMidCategoriesSubPage(User, (TopCategory)c));
            ShowSupplierProductsCommand = new CommandType();
            ShowSupplierProductsCommand.Create(s => ShowSupplierProducts((Supplier)s));
            ShowContractedSuppliersProductsCommand = new CommandType();
            ShowContractedSuppliersProductsCommand.Create(_ => ShowContractedSuppliersProducts());

            ContractedSuppliersIds = new List<Guid>(User.Client.Contracts.Select(c => c.Supplier.Id));

            LoadTopCategoriesAndSuppliersAsync();
        }

    }
}
