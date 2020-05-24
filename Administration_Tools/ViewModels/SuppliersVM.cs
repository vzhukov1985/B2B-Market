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


        public void UpdateSuppliersList()
        {
            using (SuppliersDbContext db = new SuppliersDbContext())
            {
                Suppliers = db.Suppliers.OrderBy(s=>s.ShortName).ToList();
            }
        }

        public void SaveSupplierChanges()
        {
            using (SuppliersDbContext db = new SuppliersDbContext())
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
            using (SuppliersDbContext db = new SuppliersDbContext())
            {
                Supplier NewSupplier = new Supplier
                {

                    ShortName = "Новый Поставщик",
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
            using (SuppliersDbContext db = new SuppliersDbContext())
            {
                db.Suppliers.Remove(SelectedSupplier);
                db.SaveChanges();
                UpdateSuppliersListCommand.Execute(null);
                SelectedSupplier = null;
            }

        }


        public RelayCommand UpdateSuppliersListCommand { get; }
        public RelayCommand SaveSupplierChangesCommand { get; }

        public RelayCommand AddSupplierCommand { get; }
        public RelayCommand RemoveSupplierCommand { get; set; }

        public SuppliersVM()
        {
            UpdateSuppliersListCommand = new RelayCommand(_ => { UpdateSuppliersList(); });
            SaveSupplierChangesCommand = new RelayCommand(_ => { SaveSupplierChanges(); });
            AddSupplierCommand = new RelayCommand(_ => { AddSupplier(); });
            RemoveSupplierCommand = new RelayCommand(_ => { RemoveSupplier(); });

            UpdateSuppliersListCommand.Execute(null);

        }

    }
}
