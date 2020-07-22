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
    public class SuppliersVM<CommandType> : INotifyPropertyChanged where CommandType : IRelayCommand, new()
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName]string prop = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        }

        private readonly IDialogService DialogService;

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

        private Supplier _selectedSupplier;
        public Supplier SelectedSupplier
        {
            get { return _selectedSupplier; }
            set
            {
                _selectedSupplier = value;
                OnPropertyChanged("SelectedSupplier");
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

        public void UpdateSuppliersList()
        {
            using (MarketDbContext db = new MarketDbContext())
            {
                Suppliers = new ObservableCollection<Supplier>(db.Suppliers.Include(_ => _.Contracts).ThenInclude(_ => _.Client).ToList());
            }
        }

        public void SaveSupplierChanges()
        {
            using (MarketDbContext db = new MarketDbContext())
            {
                if (SelectedSupplier != null)
                {
                    db.Suppliers.Update(SelectedSupplier);
                }
                db.SaveChanges();
            }
        }

        public void AddSupplier()
        {
            Supplier NewSupplier = new Supplier
            {
                Id = Guid.NewGuid(),
                ShortName = "Новый Поставщик",
                FullName = "Новый Поставщик",
                BIN = "0",
                Address = "Не указан",
                Phone = "Нет",
                Email = "Нет",
                FTPAccess = ""
            };

            using (MarketDbContext db = new MarketDbContext())
            {
                db.Suppliers.Add(NewSupplier);
                db.SaveChanges();
            }
            Suppliers.Add(NewSupplier);
            SelectedSupplier = NewSupplier;
        }

        public void RemoveSupplier()
        {
            using (MarketDbContext db = new MarketDbContext())
            {
                db.Suppliers.Remove(SelectedSupplier);
                db.SaveChanges();
            }
            Suppliers.Remove(SelectedSupplier);
        }

        public void AddContract()
        {
            using (MarketDbContext db = new MarketDbContext())
            {
                if (SelectedSupplier == null)
                    return;

                ObservableCollection<Client> AvailableClients = new ObservableCollection<Client>(db.Clients.Include(cc => cc.Contracts).Where(dd => !dd.Contracts.Any(ee => ee.SupplierId == SelectedSupplier.Id)).ToList());

                Client ClientToAdd = DialogService.AddContractWithClientDlg(AvailableClients);
                if (ClientToAdd != null)
                {
                    Contract NewContract = new Contract() { ClientId = ClientToAdd.Id, SupplierId = SelectedSupplier.Id };
                    db.Contracts.Add(NewContract);
                    db.SaveChanges();
                    SelectedSupplier.Contracts.Add(NewContract);
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
                    SelectedSupplier.Contracts.Remove(SelectedContract);
                }
            }
        }

        public CommandType UpdateSuppliersListCommand { get; }
        public CommandType SaveSupplierChangesCommand { get; }
        public CommandType AddSupplierCommand { get; }
        public CommandType RemoveSupplierCommand { get; }

        public CommandType AddContractCommand { get; }
        public CommandType RemoveContractCommand { get; }

        public SuppliersVM(IDialogService dialogService)
        {
            DialogService = dialogService;

            UpdateSuppliersListCommand = new CommandType();
            UpdateSuppliersListCommand.Create(_ => { UpdateSuppliersList(); });
            SaveSupplierChangesCommand = new CommandType();
            SaveSupplierChangesCommand.Create(_ => { SaveSupplierChanges(); }, _ => { return SelectedSupplier != null; });
            AddSupplierCommand = new CommandType();
            AddSupplierCommand.Create(_ => { AddSupplier(); });
            RemoveSupplierCommand = new CommandType();
            RemoveSupplierCommand.Create(_ => { RemoveSupplier(); }, _ => SelectedSupplier != null);

            AddContractCommand = new CommandType();
            AddContractCommand.Create(_ => { AddContract(); }, _ => SelectedSupplier != null);
            RemoveContractCommand = new CommandType();
            RemoveContractCommand.Create(_ => { RemoveContract(); }, _ => (SelectedSupplier != null) && (SelectedContract != null));

            UpdateSuppliersListCommand.Execute(null);
        }
    }
}