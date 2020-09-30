using ClientApp_Mobile.Models;
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
using System.Text;
using Xamarin.Forms;

namespace ClientApp_Mobile.ViewModels.SubPages
{
    public class ArchivedRequestsListSubPageVM : BaseVM
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

        public async void QueryDb()
        {
            IsBusy = true;
            try
            {
                if (UserService.CurrentUser.Client.ArchivedRequests == null)
                {
                    using (MarketDbContext db = new MarketDbContext())
                    {

                        UserService.CurrentUser.Client.ArchivedRequests = await db.ArchivedRequests
                            .Where(r => r.ClientId == User.ClientId)
                            .Include(r => r.ArchivedRequestsStatuses)
                            .ThenInclude(rs => rs.ArchivedRequestStatusType)
                            .Include(r => r.ArchivedSupplier)
                            .ToListAsync(CTS.Token);

                    }
                }
                    var groupedlist = UserService.CurrentUser
                                                    .Client
                                                    .ArchivedRequests
                                                        .GroupBy(r => r.DateTimeSent.ToString("MMMM yyyy"))
                                                        .Select(g => 
                                                            new ArchivedRequestsByMonth(g.Key,
                                                                                        UserService.CurrentUser.Client.ArchivedRequests.Where(ar => ar.DateTimeSent.ToString("MMMM yyyy") == g.Key)
                                                                                            .OrderByDescending(ar => ar.DateTimeSent))).ToList();
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
            catch
            {
                Device.BeginInvokeOnMainThread(() => DialogService.ShowConnectionErrorDlg());
                IsBusy = false;
                return;
            }
        }

        public Command ShowArchivedRequestSubPageCommand { get; }

        public ArchivedRequestsListSubPageVM()
        {
            User = UserService.CurrentUser;

            ShowArchivedRequestSubPageCommand = new Command<ArchivedRequest>(ar => ShellPageService.GotoArchivedRequestPage(ar));
        }

    }
}
