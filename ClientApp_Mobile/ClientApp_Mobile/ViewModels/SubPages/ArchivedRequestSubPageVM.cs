using Core.DBModels;
using Core.Services;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Collections.ObjectModel;
using ClientApp_Mobile.Services;
using System.Threading.Tasks;
using Xamarin.Forms;
using Core.Models;

namespace ClientApp_Mobile.ViewModels.SubPages
{
    public class ArchivedRequestSubPageVM : BaseVM
    {
        private CurrentUserInfo _user;
        public CurrentUserInfo User
        {
            get { return _user; }
            set
            {
                _user = value;
                OnPropertyChanged("User");
            }
        }

        private ArchivedRequestForClientDbView _request;
        public ArchivedRequestForClientDbView Request
        {
            get { return _request; }
            set
            {
                _request = value;
                OnPropertyChanged("Request");
            }
        }

        private List<ArchivedOrdersByCategories> _ordersGroup;
        public List<ArchivedOrdersByCategories> OrdersGroup
        {
            get { return _ordersGroup; }
            set
            {
                _ordersGroup = value;
                OnPropertyChanged("OrdersGroup");
            }
        }

        private string _title;
        public string Title
        {
            get { return _title; }
            set
            {
                _title = value;
                OnPropertyChanged("Title");
            }
        }

        public async void QueryDb()
        {
            IsBusy = true;
            Request.ArchivedOrders =await ApiConnect.GetArchivedOrdersByRequest(Request.Id, CTS.Token);

            if (CTS.IsCancellationRequested) { IsBusy = false; return; }
            Request.ArchivedRequestsStatuses = new ObservableCollection<ArchivedRequestsStatus>((await ApiConnect.GetArchivedRequestStatuses(Request.Id, CTS.Token)).OrderBy(s => s.DateTime));

            if (CTS.IsCancellationRequested) { IsBusy = false; return; }
            OrdersGroup = Request.ArchivedOrders.GroupBy(o => o.ProductCategory).Select(g => new ArchivedOrdersByCategories(g.Key, new List<ArchivedOrder>(g))).ToList();

            IsBusy = false;
        }


        public ArchivedRequestSubPageVM(ArchivedRequestForClientDbView request)
        {
            User = AppSettings.CurrentUser;
            Request = request;

            Title = request.DateTimeSent.ToString("d") + " - " + request.ArchivedSupplierName;

            Task.Run(() => QueryDb());
        }
    }



    public class ArchivedOrdersByCategories : List<ArchivedOrder>, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName] string prop = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        }

        private string _categoryName;
        public string CategoryName
        {
            get { return _categoryName; }
            set
            {
                _categoryName = value;
                OnPropertyChanged("CategoryName");
            }
        }

        public ArchivedOrdersByCategories(string categoryName, IEnumerable<ArchivedOrder> archivedOrders) : base(archivedOrders)
        {
            CategoryName = categoryName;
        }
    }
}
