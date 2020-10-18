using ClientApp_Mobile.Services;
using Core.DBModels;
using Core.Models;
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

        private async void ProceedRequest()
        {
            IsBusy = true;


            List<Guid> unprocessedOrders = await ApiConnect.ProceedClientRequests(AppSettings.CurrentUser.Client.Id, Requests);

            if (unprocessedOrders == null)
            {
                IsBusy = false;
                return;
            }

            if (unprocessedOrders.Count == 0)
            {
                Device.BeginInvokeOnMainThread(() => DialogService.ShowMessageDlg("Все заявки были отправлены поставщикам", "Заявки отправлены"));
            }

            if (unprocessedOrders.Count >0)
            {
                Device.BeginInvokeOnMainThread(() => DialogService.ShowErrorDlg("Не обработано " + unprocessedOrders.Count.ToString() + " наименований. Возможно поставщик не может осуществить поставку в указанном количестве."));
            }

            IsBusy = false;
            Device.BeginInvokeOnMainThread(() => ShellPageService.GotoCurrentRequestPage());
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
}
