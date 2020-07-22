using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Core.DBModels;
using System.Collections.ObjectModel;


namespace Administration_Tools.ViewModels
{
    public class AddContractWithClientVM : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName]string prop = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        }

        private ObservableCollection<Client> _availableClients;
        public ObservableCollection<Client> AvailableClients
        {
            get { return _availableClients; }
            set
            {
                _availableClients = value;
                OnPropertyChanged("AvailableClients");
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


        public AddContractWithClientVM(ObservableCollection<Client> AvailableClients)
        {
            this.AvailableClients = AvailableClients;
        }
    }
}
