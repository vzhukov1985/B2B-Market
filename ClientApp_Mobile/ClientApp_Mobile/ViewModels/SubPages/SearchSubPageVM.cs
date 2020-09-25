using ClientApp_Mobile.Services;
using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;

namespace ClientApp_Mobile.ViewModels.SubPages
{
    class SearchSubPageVM : BaseVM
    {
        private string _searchText;
        public string SearchText
        {
            get { return _searchText; }
            set
            {
                _searchText = value;
                OnPropertyChanged("SearchText");
            }
        }

        public Command SearchCommand { get; }

        public SearchSubPageVM()
        {
            SearchCommand = new Command(_ => ShellPageService.GotoOffersPage($"Результаты поиска '{SearchText}'", null, null, SearchText));
        }

    }
}
