using ClientApp.Services;
using Core.Models;
using Core.Services;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;

namespace ClientApp.ViewModels
{
    public class CurrentRequestConfirmSubPageVM<CommandType> : INotifyPropertyChanged where CommandType : IRelayCommand, new()
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName] string prop = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        }

        private readonly IPageService PageService;
        private readonly IDialogService DialogService;

        private ObservableCollection<ArchivedRequest> _requests;
        public ObservableCollection<ArchivedRequest> Requests
        {
            get { return _requests; }
            set
            {
                _requests = value;
                OnPropertyChanged("Requests");
            }
        }

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

        public int ItemsCount
        {
            get
            {
                return Requests.Sum(r => r.ItemsQuantity);
            }
        }

        public int ProductsCount
        {
            get
            {
                return Requests.SelectMany(r => r.Orders.Select(o => o.ProductId)).Distinct().Count();
            }
        }

        public int SuppliersCount
        {
            get
            {
                return Requests.Count;
            }
        }

        public decimal TotalSum
        {
            get
            {
                return Requests.Sum(r => r.TotalPrice);
            }
        }

        private async void ProceedRequest()
        {
            int UnProcessedRequestsCount = 0;
            using (MarketDbContext db = new MarketDbContext())
            {
                var AvailableArchivedSuppliersIds = db.ArchivedSuppliers.Select(s => s.Id);
                var Suppliers = db.Suppliers.AsNoTracking().Where(s => Requests.Select(r => r.ArchivedSupplierId).Contains(s.Id));

                foreach (ArchivedRequest request in Requests)
                {
                    if (FTPManager.UploadRequestToSupplierFTP(Suppliers.Where(s => s.Id == request.ArchivedSupplierId).FirstOrDefault().FTPAccess, request))
                    {
                        if (AvailableArchivedSuppliersIds.Contains(request.ArchivedSupplierId))
                            db.Entry(request.ArchivedSupplier).State = EntityState.Unchanged;

                        foreach (ArchivedOrder order in request.Orders)
                        {
                            order.ArchivedRequestId = request.Id;
                            order.Product = null;
                        }
                        request.Client = null;

                        await db.ArchivedRequests.AddAsync(request);
                        db.CurrentOrders.RemoveRange(User.Client.CurrentOrders.Where(o => o.ClientId == User.ClientId && request.Orders.Select(oo => oo.OfferId).Contains(o.OfferId)));
                    }
                    else
                    {
                        UnProcessedRequestsCount++;
                    }
                }

                await db.SaveChangesAsync();
                
                User.Client.CurrentOrders = new ObservableCollection<CurrentOrder>(db.CurrentOrders
                    .Where(o => o.ClientId == User.ClientId)
                    .Include(o => o.Offer)
                    .ThenInclude(o => o.QuantityUnit)
                    .Include(o => o.Offer)
                    .ThenInclude(o => o.Supplier));
            }

            if (UnProcessedRequestsCount > 0)
            {
                if (UnProcessedRequestsCount == Requests.Count)
                    DialogService.ShowErrorDlg("Проблемы с соединением. Заявки не обработаны. Попробуйте позже.");
                else
                    DialogService.ShowErrorDlg("Проблемы с соединением. Не обработано " + UnProcessedRequestsCount.ToString() + "заявок. Попробуйте позже.");
            }

            PageService.ShowCurrentRequestSubPage(User);
        }

        public CommandType NavigationBackCommand { get; }
        public CommandType ProceedRequestCommand { get; }

        public CurrentRequestConfirmSubPageVM(ClientUser user, List<ArchivedRequest> requests, IPageService pageService, IDialogService dialogService)
        {
            User = user;
            Requests = new ObservableCollection<ArchivedRequest>(requests);
            PageService = pageService;
            DialogService = dialogService;

            NavigationBackCommand = new CommandType();
            NavigationBackCommand.Create(_ => PageService.SubPageNavigationBack());
            ProceedRequestCommand = new CommandType();
            ProceedRequestCommand.Create(_ => ProceedRequest());


        }

    }
}
