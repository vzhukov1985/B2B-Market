using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Core.Models;
using Administration_Tools.Models;
using System.Linq;
using System.Collections.ObjectModel;
using Core.Helpers;
using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Administration_Tools.Helpers;

namespace Administration_Tools.ViewModels
{
    public class SuppliersVM : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName]string prop = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(prop));
        }

        private IDialogService DialogService;

        private List<Supplier> _suppliers;
        public List<Supplier> Suppliers
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
                Suppliers = db.Suppliers.Include(_ => _.Contracts).ThenInclude(_ => _.Client).OrderBy(_ => _.ShortName).ToList();
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
                UpdateSuppliersListCommand.Execute(null);
            }

        }

        public void AddSupplier()
        {
            using (MarketDbContext db = new MarketDbContext())
            {
                Supplier NewSupplier = new Supplier
                {

                    ShortName = "Новый Поставщик",
                    FullName = "Новый Поставщик",
                    BIN = "0",
                    Address = "Не указан",
                    Phone = "Нет",
                    Email = "Нет"
                };
                db.Suppliers.Add(NewSupplier);
                db.SaveChanges();
                UpdateSuppliersListCommand.Execute(null);
                SelectedSupplier = NewSupplier;
            }
        }

        public void RemoveSupplier()
        {
            using (MarketDbContext db = new MarketDbContext())
            {
                db.Suppliers.Remove(SelectedSupplier);
                db.SaveChanges();
                UpdateSuppliersListCommand.Execute(null);
                SelectedSupplier = null;
            }

        }
        public void AddContract()
        {
            using (MarketDbContext db = new MarketDbContext())
            {
                if (SelectedSupplier == null)
                    return;
                List<Client> AvailableClients = db.Clients.Include(cc => cc.Contracts).Where(dd => !dd.Contracts.Any(ee => ee.SupplierId == SelectedSupplier.Id)).ToList();
                
                
                Client ClientToAdd = DialogService.AddContractWithClientDlg(AvailableClients);
                if (ClientToAdd != null)
                {
                    db.Contracts.Add(new Contract() { ClientId = ClientToAdd.Id, SupplierId = SelectedSupplier.Id });
                    db.SaveChanges();
                }
                UpdateSuppliersListCommand.Execute(null);
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
                }
            }
            UpdateSuppliersListCommand.Execute(null);
        }

        public RelayCommand UpdateSuppliersListCommand { get; }
        public RelayCommand SaveSupplierChangesCommand { get; }

        public RelayCommand AddSupplierCommand { get; }
        public RelayCommand RemoveSupplierCommand { get; }

        public RelayCommand AddContractCommand { get; }

        public RelayCommand RemoveContractCommand { get; }

        public SuppliersVM(IDialogService dialogService)
        {
            DialogService = dialogService;

            UpdateSuppliersListCommand = new RelayCommand(_ => { UpdateSuppliersList(); });
            SaveSupplierChangesCommand = new RelayCommand(_ => { SaveSupplierChanges(); }, _ => { return SelectedSupplier != null; });
            AddSupplierCommand = new RelayCommand(_ => { AddSupplier(); });
            RemoveSupplierCommand = new RelayCommand(_ => { RemoveSupplier(); }, _ => SelectedSupplier != null);

            AddContractCommand = new RelayCommand(_ => { AddContract(); }, _ => SelectedSupplier != null);
            RemoveContractCommand = new RelayCommand(_ => { RemoveContract(); }, _ => (SelectedSupplier != null) && (SelectedContract != null));

            UpdateSuppliersListCommand.Execute(null);
        }
    }
}
