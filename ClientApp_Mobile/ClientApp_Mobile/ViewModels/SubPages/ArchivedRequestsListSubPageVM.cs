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

        private ObservableCollection<ArchivedRequest> _archivedRequests;
        public ObservableCollection<ArchivedRequest> ArchivedRequests
        {
            get { return _archivedRequests; }
            set
            {
                _archivedRequests = value;
                OnPropertyChanged("ArchivedRequests");
            }
        }

        private ObservableCollection<ArchivedRequestsByMonth> _archivedRequestsByMonth;
        public ObservableCollection<ArchivedRequestsByMonth> ArchivedRequestsByMonth
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
                using (MarketDbContext db = new MarketDbContext())
                {
                    ArchivedRequests = new ObservableCollection<ArchivedRequest>(await db.ArchivedRequests
                        .AsNoTracking()
                        .Where(r => r.ClientId == User.ClientId)
                        .Include(r => r.ArchivedRequestsStatuses)
                        .ThenInclude(rs => rs.ArchivedRequestStatusType)
                        .Include(r => r.ArchivedSupplier)
                        .OrderByDescending(r => r.DateTimeSent)
                        .ToListAsync());

                }
                ArchivedRequestsByMonth = new ObservableCollection<ArchivedRequestsByMonth>(ArchivedRequests.GroupBy(r => r.DateTimeSent.ToString("MMMM yyyy"))
                                                                                                            .Select(g => new ArchivedRequestsByMonth(g.Key, ArchivedRequests.Where(ar => ar.DateTimeSent.ToString("MMMM yyyy") == g.Key))));
                IsBusy = false;
            }
            catch
            {
                ShellDialogService.ShowConnectionErrorDlg();
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
