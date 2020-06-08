using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Core.Models;
using System.Collections.ObjectModel;

namespace Administration_Tools.ViewModels
{
    public class AddContractWithSupplierVM : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName]string prop = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        }

        private ObservableCollection<Supplier> _availableSuppliers;
        public ObservableCollection<Supplier> AvailableSuppliers
        {
            get { return _availableSuppliers; }
            set
            {
                _availableSuppliers = value;
                OnPropertyChanged("AvailableSuppliers");
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


        public AddContractWithSupplierVM(ObservableCollection<Supplier> AvailableSuppliers)
        {
            this.AvailableSuppliers = AvailableSuppliers;
        }
    }
}
