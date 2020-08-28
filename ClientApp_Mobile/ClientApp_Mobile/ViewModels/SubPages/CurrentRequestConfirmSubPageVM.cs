using ClientApp_Mobile.Services;
using Core.DBModels;
using Core.Services;
using Microsoft.EntityFrameworkCore;
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
    public class CurrentRequestConfirmSubPageVM : BaseVM
    {
        private List<ArchivedRequest> _requests;
        public List<ArchivedRequest> Requests
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
                return Requests.SelectMany(r => r.ArchivedOrders.Select(o => o.ProductId)).Distinct().Count();
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
            IsBusy = true;
            try
            {
                int UnProcessedRequestsCount = 0;
                using (MarketDbContext db = new MarketDbContext())
                {
                    var AvailableArchivedSuppliersIds = db.ArchivedSuppliers.AsNoTracking().Select(s => s.Id);
                    var Suppliers = db.Suppliers.AsNoTracking().Where(s => Requests.Select(r => r.ArchivedSupplierId).Contains(s.Id));

                    foreach (ArchivedRequest request in Requests)
                    {
                        Supplier sup = Suppliers.Where(s => s.Id == request.ArchivedSupplierId).FirstOrDefault();
                        if (FTPManager.UploadRequestToSupplierFTP(sup.FTPUser, sup.FTPPassword, request))
                        {
                            if (!AvailableArchivedSuppliersIds.Contains(request.ArchivedSupplierId))
                            {
                                await db.ArchivedSuppliers.AddAsync(ArchivedSupplier.CloneForDB(request.ArchivedSupplier));
                            }

                            await db.ArchivedRequests.AddAsync(ArchivedRequest.CloneForDb(request));

                            foreach (ArchivedOrder order in request.ArchivedOrders)
                            {
                                await db.ArchivedOrders.AddAsync(ArchivedOrder.CloneForDB(order));

                                Offer ofRemainsToUpdate = new Offer() { Id = order.OfferId, Remains = order.Remains - order.Quantity };
                                db.Offers.Attach(ofRemainsToUpdate);
                                db.Entry(ofRemainsToUpdate).Property(o => o.Remains).IsModified = true;
                            }

                            foreach (ArchivedRequestsStatus status in request.ArchivedRequestsStatuses)
                            {
                                await db.ArchivedRequestsStatuses.AddAsync(ArchivedRequestsStatus.CloneForDb(status));
                            }

                            db.CurrentOrders.RemoveRange(User.Client.CurrentOrders.Where(o => o.ClientId == User.ClientId && request.ArchivedOrders.Select(oo => oo.OfferId).Contains(o.OfferId)).Select(co => CurrentOrder.CloneForDB(co)));
                            UserService.CurrentUser.Client.ArchivedRequests?.Add(request);
                           
                        }
                        else
                        {
                            UnProcessedRequestsCount++;
                        }
                    }

                    await db.SaveChangesAsync();

                    User.Client.CurrentOrders = new List<CurrentOrder>(db.CurrentOrders
                        .Where(o => o.ClientId == User.ClientId)
                        .Include(o => o.Offer)
                        .ThenInclude(o => o.QuantityUnit)
                        .Include(o => o.Offer)
                        .ThenInclude(o => o.Supplier));
                }

                if (UnProcessedRequestsCount > 0)
                {
                    if (UnProcessedRequestsCount == Requests.Count)
                        Device.BeginInvokeOnMainThread(() => ShellDialogService.ShowErrorDlg("Проблемы с соединением. Заявки не обработаны. Попробуйте позже."));
                    else
                        Device.BeginInvokeOnMainThread(() => ShellDialogService.ShowErrorDlg("Проблемы с соединением. Не обработано " + UnProcessedRequestsCount.ToString() + "заявок. Попробуйте позже."));
                }
                else
                {
                    Device.BeginInvokeOnMainThread(() => ShellDialogService.ShowMessageDlg("Все заявки были отправлены поставщикам", "Заявки отправлены"));
                }

                Device.BeginInvokeOnMainThread(() => ShellPageService.GotoCurrentRequestPage());
                IsBusy = false;
            }
            catch
            {
                Device.BeginInvokeOnMainThread(() => ShellDialogService.ShowConnectionErrorDlg());
                IsBusy = false;
                return;
            }
        }

        public async void ChangeComments(ArchivedRequest request)
        {
            string res = await ShellDialogService.ShowInputDialog("Введите комментарии к заказу:", "Комментарии", Keyboard.Text, request.Comments, "", 300);
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
                if (await ShellDialogService.ShowOkCancelDialog("Вы действительно хотите удалить коментарии к заказу?", "Удаление комментариев"))
                    request.Comments = "";
            }
        }

        public Command ProceedRequestCommand { get; }
        public Command ChangeCommentsCommand { get; }
        public Command CommentsAvailableChangeCommand { get; }

        public CurrentRequestConfirmSubPageVM(List<ArchivedRequest> requests)
        {
            User = UserService.CurrentUser;
            Requests = requests;

            ProceedRequestCommand = new Command(_ => Task.Run(() => ProceedRequest()));
            ChangeCommentsCommand = new Command<ArchivedRequest>(ar => ChangeComments(ar));
            CommentsAvailableChangeCommand = new Command<ArchivedRequest>(ar => CommentsAvailableChange(ar));
        }

        public CurrentRequestConfirmSubPageVM()
        {

        }
    }
}
