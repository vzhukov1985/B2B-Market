using Administration_Tools.Services;
using Core.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;

namespace Administration_Tools.ViewModels
{
    public class MainVM<CommandType>:INotifyPropertyChanged where CommandType: IRelayCommand, new()
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName]string prop = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        }

		private readonly IPageService PageService;

        public CommandType ShowSuppliersPageCommand { get; }
        public CommandType ShowClientsPageCommand { get; }

        public MainVM(IPageService pageService)
        {
            PageService = pageService;

            ShowSuppliersPageCommand = new CommandType();
            ShowSuppliersPageCommand.Create(_ => { PageService.ShowSuppliersPage(); });
            ShowClientsPageCommand = new CommandType();
            ShowClientsPageCommand.Create(_ => { PageService.ShowClientsPage(); });

        }
    }
}
