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
using ClientApp_Mobile.Models;

namespace ClientApp_Mobile.ViewModels.SubPages
{
    public class ArchivedRequestSubPageVM : BaseVM
    {
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

        private ArchivedRequest _request;
        public ArchivedRequest Request
        {
            get { return _request; }
            set
            {
                _request = value;
                OnPropertyChanged("Request");
            }
        }

        private ObservableCollection<ArchivedOrdersByCategories> _ordersGroup;
        public ObservableCollection<ArchivedOrdersByCategories> OrdersGroup
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
            try
            {
                using (MarketDbContext db = new MarketDbContext())
                {
                    Request.ArchivedOrders = new ObservableCollection<ArchivedOrder>(await db.ArchivedOrders.AsNoTracking().Where(o => o.ArchivedRequestId == Request.Id).ToListAsync());
                }
                OrdersGroup = new ObservableCollection<ArchivedOrdersByCategories>(Request.ArchivedOrders.GroupBy(o => o.ProductCategory).Select(g => new ArchivedOrdersByCategories(g.Key, new ObservableCollection<ArchivedOrder>(g))));
                Request.ArchivedRequestsStatuses = new ObservableCollection<ArchivedRequestsStatus>(Request.ArchivedRequestsStatuses.OrderBy(s => s.DateTime));
                IsBusy = false;
            }
            catch
            {
                ShellDialogService.ShowConnectionErrorDlg();
                IsBusy = false;
                return;
            }
        }


        public ArchivedRequestSubPageVM(ArchivedRequest request)
        {
            User = UserService.CurrentUser;
            Request = request;

            Title = request.DateTimeSent.ToString("d") + " - " + request.ArchivedSupplier.FullName;

            QueryDb();
        }
    }
}
