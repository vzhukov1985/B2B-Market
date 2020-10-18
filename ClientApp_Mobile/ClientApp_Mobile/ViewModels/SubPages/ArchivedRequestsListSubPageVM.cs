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
using System.Text;
using Xamarin.Forms;

namespace ClientApp_Mobile.ViewModels.SubPages
{
    public class ArchivedRequestsListSubPageVM : BaseVM
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


        private List<ArchivedRequestsByMonth> _archivedRequestsByMonth;
        public List<ArchivedRequestsByMonth> ArchivedRequestsByMonth
        {
            get { return _archivedRequestsByMonth; }
            set
            {
                _archivedRequestsByMonth = value;
                OnPropertyChanged("ArchivedRequestsByMonth");
            }
        }

        public Command ShowArchivedRequestSubPageCommand { get; }

        public ArchivedRequestsListSubPageVM()
        {
            User = AppSettings.CurrentUser;

            ShowArchivedRequestSubPageCommand = new Command<ArchivedRequestForClientDbView>(ar => ShellPageService.GotoArchivedRequestPage(ar));
        }

        public async void QueryDb()
        {
            IsBusy = true;
            try
            {
                var archivedRequestsFromDb = await ApiConnect.GetArchivedRequestsByClient(AppSettings.CurrentUser.Client.Id, CTS.Token);

                var groupedlist = archivedRequestsFromDb
                                        .GroupBy(r => r.DateTimeSent.ToString("MMMM yyyy"))
                                        .Select(g =>
                                            new ArchivedRequestsByMonth(g.Key,
                                                                        archivedRequestsFromDb.Where(ar => ar.DateTimeSent.ToString("MMMM yyyy") == g.Key)
                                                                            .OrderByDescending(ar => ar.Code))).ToList();
                foreach (var month in groupedlist)
                {
                    if (CTS.IsCancellationRequested) { IsBusy = false; return; }
                    month.Month = char.ToUpper(month.Month[0]) + month.Month.Substring(1);
                }

                Device.BeginInvokeOnMainThread(() => ArchivedRequestsByMonth = groupedlist);
                IsBusy = false;
            }
            catch (OperationCanceledException)
            {
                IsBusy = false;
                return;
            }
        }
    }



    public class ArchivedRequestsByMonth : List<ArchivedRequestForClientDbView>, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName] string prop = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        }

        private string _month;
        public string Month
        {
            get { return _month; }
            set
            {
                _month = value;
                OnPropertyChanged("Month");
            }
        }

        public ArchivedRequestsByMonth(string month, IEnumerable<ArchivedRequestForClientDbView> archivedRequests) : base(archivedRequests)
        {
            Month = month;
        }
    }
}
