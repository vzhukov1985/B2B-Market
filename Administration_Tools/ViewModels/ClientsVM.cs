using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Core.DBModels;
using System.Linq;
using System.Collections.ObjectModel;
using Core.Services;
using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Administration_Tools.Services;

namespace Administration_Tools.ViewModels
{
    public class ClientsVM<CommandType> : INotifyPropertyChanged where CommandType: IRelayCommand, new()
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName]string prop = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        }

        private readonly IDialogService DialogService;
        private readonly IPageService PageService;

        private ObservableCollection<Client> _clients;
        public ObservableCollection<Client> Clients
        {
            get { return _clients; }
            set
            {
                _clients = value;
                OnPropertyChanged("Clients");
            }
        }

        private Client _selectedClient;
        public Client SelectedClient
        {
            get { return _selectedClient; }
            set
            {
                _selectedClient = value;
                OnPropertyChanged("SelectedClient");
            }
        }

        private Contract _selectedContract;
        public Contract SelectedContract
        {
            get { return _selectedContract; }
            set
            {
                _selectedContract = value;
                OnPropertyChanged("SelectedContract");
            }
        }

        public void UpdateClientsList()
        {
            using (MarketDbContext db = new MarketDbContext())
            {
                Clients = new ObservableCollection<Client>(db.Clients
                    .Include(_ => _.Contracts)
                    .ThenInclude(_ => _.Supplier)
                    .Include(c => c.Users)
                    .ToList());
            }
        }

        public void SaveClientChanges()
        {
            using (MarketDbContext db = new MarketDbContext())
            {
                if (SelectedClient != null)
                {
                    db.Clients.Update(SelectedClient);
                }
                db.SaveChanges();
            }
        }

        public void AddClient()
        {
            Client NewClient = new Client
            {
                Id = Guid.NewGuid(),
                ShortName = "Новый Клиент",
                FullName = "Новый Клиент",
                BIN = "0",
                Address = "Не указан",
                Phone = "Нет",
                Email = "Нет"
            };

            using (MarketDbContext db = new MarketDbContext())
            {
                db.Clients.Add(NewClient);
                db.SaveChanges();
            }
            Clients.Add(NewClient);
            SelectedClient = NewClient;
        }

        public void RemoveClient()
        {
            using (MarketDbContext db = new MarketDbContext())
            {
                db.Clients.Remove(SelectedClient);
                db.SaveChanges();
            }
            Clients.Remove(SelectedClient);
        }

        public void AddContract()
        {
            using (MarketDbContext db = new MarketDbContext())
            {
                if (SelectedClient == null)
                    return;

                ObservableCollection<Supplier> AvailableSuppliers = new ObservableCollection<Supplier>(db.Suppliers.Include(cc => cc.Contracts).Where(dd => !dd.Contracts.Any(ee => ee.ClientId == SelectedClient.Id)).ToList());

                Supplier SupplierToAdd = DialogService.AddContractWithSupplierDlg(AvailableSuppliers);
                if (SupplierToAdd != null)
                {
                    Contract NewContract = new Contract() { SupplierId = SupplierToAdd.Id, ClientId = SelectedClient.Id };
                    db.Contracts.Add(NewContract);
                    db.SaveChanges();
                    SelectedClient.Contracts.Add(NewContract);
                }
            }
        }

        public void RemoveContract()
        {
            if (SelectedContract != null)
            {
                using (MarketDbContext db = new MarketDbContext())
                {
                    db.Contracts.Remove(SelectedContract);
                    db.SaveChanges();
                    SelectedClient.Contracts.Remove(SelectedContract);
                }
            }
        }
       
        public CommandType UpdateClientsListCommand { get; }
        public CommandType SaveClientChangesCommand { get; }
        public CommandType AddClientCommand { get; }
        public CommandType RemoveClientCommand { get; set; }

        public CommandType AddContractCommand { get; }
        public CommandType RemoveContractCommand { get; }

        public CommandType ShowClientUsersPageCommand { get; }

        public ClientsVM(IPageService pageService, IDialogService dialogService)
        {
            DialogService = dialogService;
            PageService = pageService;

            UpdateClientsListCommand = new CommandType();
            UpdateClientsListCommand.Create(_ => { UpdateClientsList(); });
            SaveClientChangesCommand = new CommandType();
            SaveClientChangesCommand.Create(_ => { SaveClientChanges(); }, _ => { return SelectedClient != null; });
            AddClientCommand = new CommandType();
            AddClientCommand.Create(_ => { AddClient(); });
            RemoveClientCommand = new CommandType();
            RemoveClientCommand.Create(_ => { RemoveClient(); }, _ => SelectedClient != null);

            AddContractCommand = new CommandType();
            AddContractCommand.Create(_ => { AddContract(); }, _ => SelectedClient != null);
            RemoveContractCommand = new CommandType();
            RemoveContractCommand.Create(_ => { RemoveContract(); }, _ => (SelectedClient != null) && (SelectedContract != null));

            ShowClientUsersPageCommand = new CommandType();
            ShowClientUsersPageCommand.Create(_ => { PageService.ShowClientUsersPage(SelectedClient, dialogService); }, _ => SelectedClient != null);
            
            UpdateClientsListCommand.Execute(null);
        }
    }
}