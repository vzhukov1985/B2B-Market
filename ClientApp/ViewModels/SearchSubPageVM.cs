using ClientApp.Services;
using Core.DBModels;
using Core.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;

namespace ClientApp.ViewModels
{
    public class SearchSubPageVM<CommandType> : INotifyPropertyChanged where CommandType : IRelayCommand, new()
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

        public CommandType NavigationBackCommand { get; }
        public CommandType SearchCommand { get; }


        public SearchSubPageVM(ClientUser user, IPageService pageService)
        {
            User = user;
            PageService = pageService;
            NavigationBackCommand = new CommandType();
            NavigationBackCommand.Create(_ => PageService.SubPageNavigationBack());
            SearchCommand = new CommandType();
            SearchCommand.Create(txt => PageService.ShowOffersSubPage(
                User,
                "Результаты поиска  '"+ (string)txt+"'",
                null, 
                null, 
                (string)txt));
        }

    }
}
