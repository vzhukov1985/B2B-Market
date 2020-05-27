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
    public class ClientsVM : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName]string prop = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(prop));
        }

        private List<Client> _clients;
        public List<Client> Clients
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


        public void UpdateClientsList()
        {
            using (ClientsDbContext db = new ClientsDbContext())
            {
                Clients = db.Clients.OrderBy(s => s.ShortName).ToList();
            }
        }

        public void SaveClientChanges()
        {
            using (ClientsDbContext db = new ClientsDbContext())
            {
                if (SelectedClient != null)
                {
                    db.Clients.Update(SelectedClient);
                }
                db.SaveChanges();
                UpdateClientsListCommand.Execute(null);
            }

        }

        public void AddClient()
        {
            using (ClientsDbContext db = new ClientsDbContext())
            {
                Client NewClient = new Client
                {

                    ShortName = "Новый Клиент",
                    FullName = "Новый Клиент",
                    BIN = "0",
                    Address = "Не указан",
                    Phone = "Нет",
                    Email = "Нет"
                };
                db.Clients.Add(NewClient);
                db.SaveChanges();
                UpdateClientsListCommand.Execute(null);
                SelectedClient = NewClient;
            }
        }

        public void RemoveClient()
        {
            using (ClientsDbContext db = new ClientsDbContext())
            {
                db.Clients.Remove(SelectedClient);
                db.SaveChanges();
                UpdateClientsListCommand.Execute(null);
                SelectedClient = null;
            }

        }


        public RelayCommand UpdateClientsListCommand { get; }
        public RelayCommand SaveClientChangesCommand { get; }

        public RelayCommand AddClientCommand { get; }
        public RelayCommand RemoveClientCommand { get; set; }

        public ClientsVM()
        {
            UpdateClientsListCommand = new RelayCommand(_ => { UpdateClientsList(); });
            SaveClientChangesCommand = new RelayCommand(_ => { SaveClientChanges(); });
            AddClientCommand = new RelayCommand(_ => { AddClient(); });
            RemoveClientCommand = new RelayCommand(_ => { RemoveClient(); });

            UpdateClientsListCommand.Execute(null);
        }
    }
}
