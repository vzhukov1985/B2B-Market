using ClientApp.Services;
using Core.Models;
using Core.Services;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Collections.ObjectModel;

namespace ClientApp.ViewModels
{
    public class ArchivedRequestSubPageVM<CommandType> : INotifyPropertyChanged where CommandType : IRelayCommand, new()
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName] string prop = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        }

        private readonly IPageService PageService;

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

        private Dictionary<string, ObservableCollection<ArchivedOrder>> _ordersGroup;
        public Dictionary<string, ObservableCollection<ArchivedOrder>> OrdersGroup
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
            using (MarketDbContext db = new MarketDbContext())
            {
                Request.Orders = new ObservableCollection<ArchivedOrder>(await db.ArchivedOrders.Where(o => o.ArchivedRequestId == Request.Id)
                    .Include(o => o.Product)
                    .ThenInclude(p => p.Category)
                    .ThenInclude(c => c.MidCategory)
                    .ThenInclude(m => m.TopCategory)
                    .Include(o => o.Product)
                    .ThenInclude(p => p.VolumeType)
                    .Include(o => o.Product)
                    .ThenInclude(p => p.VolumeUnit)
                    .OrderBy(o => o.Product.Category)
                    .ToListAsync());

                OrdersGroup = Request.Orders.GroupBy(o => o.Product.Category.MidCategory.TopCategory.Name).ToDictionary(g => g.Key, g => new ObservableCollection<ArchivedOrder>(g));
                Request.ArchivedRequestsStatuses = new ObservableCollection<ArchivedRequestsStatus>(Request.ArchivedRequestsStatuses.OrderBy(s => s.DateTime));
            }
        }

        public CommandType NavigationBackCommand { get; }

        public ArchivedRequestSubPageVM(ClientUser user, ArchivedRequest request, IPageService pageService)
        {
            User = user;
            Request = request;
            PageService = pageService;

            NavigationBackCommand = new CommandType();
            NavigationBackCommand.Create(_ => PageService.SubPageNavigationBack());

            Title = request.DateTimeSent.ToString("d") + " - " + request.ArchivedSupplier.FullName;
            
            QueryDb();
            
        }
    }
}
