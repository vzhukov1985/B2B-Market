using ClientApp.Services;
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

namespace ClientApp.ViewModels
{
    public class ArchivedRequestsListSubPageVM<CommandType> : INotifyPropertyChanged where CommandType : IRelayCommand, new()
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

        private Dictionary<string, ObservableCollection<ArchivedRequest>> _archivedRequestsByMonth;
        public Dictionary<string, ObservableCollection<ArchivedRequest>> ArchivedRequestsByMonth
        {
            get { return _archivedRequestsByMonth; }
            set
            {
                _archivedRequestsByMonth = value;
                OnPropertyChanged("ArchivedRequestsByMonth");
            }
        }

        private async void QueryDb()
        {
            using (MarketDbContext db = new MarketDbContext())
            {
                  ArchivedRequests = new ObservableCollection<ArchivedRequest>(await db.ArchivedRequests
                      .Where(r => r.ClientId == User.ClientId)
                      .Include(r => r.ArchivedRequestsStatuses)
                      .ThenInclude(rs => rs.ArchivedRequestStatusType)
                      .Include(r => r.ArchivedSupplier)
                      .OrderByDescending(r => r.DateTimeSent)
                      .ToListAsync());

                ArchivedRequestsByMonth = ArchivedRequests.GroupBy(r => r.DateTimeSent.ToString("MMMM yyyy")).ToDictionary(g => g.Key, g => new ObservableCollection<ArchivedRequest>(g));
            }

        }

        public CommandType NavigationBackCommand { get; }
        public CommandType ShowArchivedRequestSubPageCommand { get; }

        public ArchivedRequestsListSubPageVM(ClientUser user, IPageService pageService)
        {
            User = user;
            PageService = pageService;

            NavigationBackCommand = new CommandType();
            NavigationBackCommand.Create(_ => PageService.SubPageNavigationBack());
            ShowArchivedRequestSubPageCommand = new CommandType();
            ShowArchivedRequestSubPageCommand.Create(r => PageService.ShowArchivedRequestSubPage(User, (ArchivedRequest)r));
            
            QueryDb();
        }

    }
}
