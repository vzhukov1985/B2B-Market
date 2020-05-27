using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Core.Models;
using System.Collections.ObjectModel;
using System.Linq;

namespace Administration_Tools.ViewModels
{
    public class AddContractWithClientVM : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName]string prop = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(prop));
        }

        private List<Client> _availableClients;
        public List<Client> AvailableClients
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



        public AddContractWithClientVM(List<Client> AvailableClients)
        {
            this.AvailableClients = AvailableClients;
        }


    }
}
