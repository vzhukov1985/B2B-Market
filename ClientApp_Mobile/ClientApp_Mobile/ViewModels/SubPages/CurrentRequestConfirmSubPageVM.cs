using ClientApp_Mobile.Services;
using Core.DBModels;
using Core.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace ClientApp_Mobile.ViewModels.SubPages
{
    class CurrentRequestConfirmSubPageVM : BaseVM
    {
        private List<RequestForConfirmation> _requests;
        public List<RequestForConfirmation> Requests
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
                return Requests.Sum(r => r.ProductsQuantity);
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

        public Command ProceedRequestCommand { get; }
        public Command ChangeCommentsCommand { get; }
        public Command CommentsAvailableChangeCommand { get; }

        public CurrentRequestConfirmSubPageVM(List<RequestForConfirmation> requests)
        {
            User = AppSettings.CurrentUser;
            Requests = requests;

            ProceedRequestCommand = new Command(_ => Task.Run(() => ProceedRequest()));
            ChangeCommentsCommand = new Command<ArchivedRequest>(ar => ChangeComments(ar));
            CommentsAvailableChangeCommand = new Command<ArchivedRequest>(ar => CommentsAvailableChange(ar));
        }

        public CurrentRequestConfirmSubPageVM()
        {

        }

        private async void ProceedRequest()
        {
            IsBusy = true;
            int UnprocessedRequestsCount = 0;
            try
            {

                using (MarketDbContext db = new MarketDbContext())
                {                  
                    foreach (var request in Requests)
                    {
                        if (/*FTPManager.UploadRequestToSupplierFTP(request, request.FTPSupplierFolder)*/true)
                        {
                            if (!db.ArchivedSuppliers.Any(s => s.Id == request.SupplierId))
                            {
                                await db.ArchivedSuppliers.AddAsync(ArchivedSupplier.CloneForDB(request.ArchivedSupplier));
                            }

                            await db.ArchivedRequests.AddAsync(ArchivedRequest.CloneForDb(request));

                            request.ArchivedOrders = new List<ArchivedOrder>();

                            foreach (var order in request.OrdersToConfirm)
                            {
                                request.ArchivedOrders.Add(order);

                                await db.ArchivedOrders.AddAsync(ArchivedOrder.CloneForDB(order));

                                Offer ofRemainsToUpdate = new Offer() { Id = order.OfferId, Remains = order.Remains - order.Quantity };
                                db.Offers.Attach(ofRemainsToUpdate);
                                db.Entry(ofRemainsToUpdate).Property(o => o.Remains).IsModified = true;

                                db.CurrentOrders.Remove(new CurrentOrder { ClientId = AppSettings.CurrentUser.ClientId, OfferId = order.OfferId });
                                AppSettings.CurrentUser.Client.CurrentOrders.Remove(AppSettings.CurrentUser.Client.CurrentOrders.Where(co => co.OfferId == order.OfferId).FirstOrDefault());
                            }
                            foreach (ArchivedRequestsStatus status in request.ArchivedRequestsStatuses)
                            {
                                await db.ArchivedRequestsStatuses.AddAsync(ArchivedRequestsStatus.CloneForDb(status));
                            }
                        }
                        else
                        {
                            UnprocessedRequestsCount++;
                        }
                        await db.SaveChangesAsync();
                    }
                    if (!db.ArchivedClients.Any(c => c.Id == Requests.FirstOrDefault().ClientId))
                    {
                        await db.ArchivedClients.AddAsync(ArchivedClient.CloneForDb(Requests.FirstOrDefault().ArchivedClient));
                    }
                    await db.SaveChangesAsync();

                    if (UnprocessedRequestsCount > 0)
                    {
                        if (UnprocessedRequestsCount == Requests.Count)
                            Device.BeginInvokeOnMainThread(() => DialogService.ShowErrorDlg("Проблемы с соединением. Заявки не обработаны. Попробуйте позже."));
                        else
                            Device.BeginInvokeOnMainThread(() => DialogService.ShowErrorDlg("Проблемы с соединением. Не обработано " + UnprocessedRequestsCount.ToString() + "заявок. Попробуйте позже."));
                    }
                    else
                    {
                        Device.BeginInvokeOnMainThread(() => DialogService.ShowMessageDlg("Все заявки были отправлены поставщикам", "Заявки отправлены"));
                    }

                    Device.BeginInvokeOnMainThread(() => ShellPageService.GotoCurrentRequestPage());
                    IsBusy = false;

                }
            }
            catch
            {
                Device.BeginInvokeOnMainThread(() => DialogService.ShowConnectionErrorDlg());
                IsBusy = false;
                return;
            }
            IsBusy = false;
        }

        public async void ChangeComments(ArchivedRequest request)
        {
            string res = await DialogService.ShowInputDialog("Введите комментарии к заказу:", "Комментарии", Keyboard.Text, request.Comments, "", 300);
            if (res != null)
                request.Comments = res;
        }
        public async void CommentsAvailableChange(ArchivedRequest request)
        {
            if (string.IsNullOrEmpty(request.Comments))
            {
                ChangeComments(request);
            }
            else
            {
                if (await DialogService.ShowOkCancelDialog("Вы действительно хотите удалить коментарии к заказу?", "Удаление комментариев"))
                    request.Comments = "";
            }
        }

    }




    class RequestForConfirmation : ArchivedRequest
    {
        public string FTPSupplierFolder { get; set; }

        public DateTime DeliveryDate
        {
            get { return DeliveryDateTime.Date; }
            set
            {
                DeliveryDateTime = new DateTime(value.Year, value.Month, value.Day, DeliveryDateTime.Hour, DeliveryDateTime.Minute, DeliveryDateTime.Second);
                OnPropertyChanged("DeliveryDate");
            }
        }

        public TimeSpan DeliveryTime
        {
            get { return DeliveryDateTime.TimeOfDay; }
            set
            {
                DeliveryDateTime = new DateTime(DeliveryDateTime.Year, DeliveryDateTime.Month, DeliveryDateTime.Day, value.Hours, value.Minutes, value.Seconds);
                OnPropertyChanged("DeliveryTime");
            }
        }

        public List<OrderOfRequestForConfirmation> OrdersToConfirm { get; set; }
    }

    class ProductForConfirmRequestView
    {
        public string Name { get; set; }
        public Uri PictureUri { get; set; }
        public string CategoryName { get; set; }
        public string VolumeType { get; set; }
        public decimal Volume { get; set; }
        public string VolumeUnit { get; set; }
    }

    class OrderOfRequestForConfirmation : ArchivedOrder
    {
        public Guid OfferId { get; set; }
        public decimal Remains { get; set; }
        public ProductForConfirmRequestView Product { get; set; }
    }
}
