using ClientApp.Services;
using Core.Models;
using Core.Services;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

namespace ClientApp.ViewModels
{
    public class MainPageVM<CommandType> : INotifyPropertyChanged where CommandType : IRelayCommand, new()
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

        private void GatherUserInfo(ClientUser user)
        {
            using (MarketDbContext db = new MarketDbContext())
            {
                User = db.ClientsUsers.Where(u => u.Login == user.Login)
                    .Include(u => u.FavoriteProducts)
                    .ThenInclude(f => f.Product)
                    .Include(u => u.Client)
                    .ThenInclude(c => c.Contracts)
                    .ThenInclude(ct => ct.Supplier)
                    .Include(u => u.Client)
                    .ThenInclude(c => c.CurrentOrders)
                    .ThenInclude(o => o.Offer)
                    .ThenInclude(of => of.QuantityUnit)
                    .Include(u => u.Client)
                    .ThenInclude(c => c.CurrentOrders)
                    .ThenInclude(o => o.Offer)
                    .ThenInclude(of => of.Supplier)
                    .FirstOrDefault();
            }
        }

        public CommandType ShowMainSubPageCommand { get; }
        public CommandType ShowFavoritesSubPageCommand { get; }
        public CommandType ShowSearchSubPageCommand { get; }
        public CommandType ShowCurrentRequestSubPageCommand { get; }
        public CommandType ShowArchivedRequestsSubPageCommand { get; }


        public MainPageVM(ClientUser user, IPageService pageService)
        {
            PageService = pageService;

            ShowMainSubPageCommand = new CommandType();
            ShowMainSubPageCommand.Create(_ => PageService.ShowMainSubPage(User));
            ShowSearchSubPageCommand = new CommandType();
            ShowSearchSubPageCommand.Create(_ => PageService.ShowSearchSubPage(User));
            ShowFavoritesSubPageCommand = new CommandType();
            ShowFavoritesSubPageCommand.Create(_ => PageService.ShowFavoritesSubPage(User));
            ShowCurrentRequestSubPageCommand = new CommandType();
            ShowCurrentRequestSubPageCommand.Create(_ => PageService.ShowCurrentRequestSubPage(User));
            ShowArchivedRequestsSubPageCommand = new CommandType();
            ShowArchivedRequestsSubPageCommand.Create(_ => PageService.ShowArchivedRequestsListSubPage(User));

            GatherUserInfo(user);
            PageService.ShowMainSubPage(User);
        }
    }
}
