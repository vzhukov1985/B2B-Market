using Core.Services;
using OperatorApp.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;

namespace OperatorApp.ViewModels
{
    public class MainWindowVM<CommandType>: INotifyPropertyChanged where CommandType : IRelayCommand, new()
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName] string prop = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        }


        public CommandType QuantityUnitsPageCommand { get; }
        public CommandType VolumeTypesPageCommand { get; }
        public CommandType VolumeUnitsPageCommand { get; }
        public CommandType ProductExtraPropertyTypesPageCommand { get; }
        public CommandType CategoriesPageCommand { get; }
        public CommandType OffersPageCommand { get;}
        public CommandType PicturesPageCommand { get; }
        public CommandType DescriptionsPageCommand { get; }

        public MainWindowVM(IPageService pageService)
        {
            QuantityUnitsPageCommand = new CommandType();
            QuantityUnitsPageCommand.Create(_ => pageService.ShowQuantityUnitsPage());
            VolumeTypesPageCommand = new CommandType();
            VolumeTypesPageCommand.Create(_ => pageService.ShowVolumeTypesPage());
            VolumeUnitsPageCommand = new CommandType();
            VolumeUnitsPageCommand.Create(_ => pageService.ShowVolumeUnitsPage());
            ProductExtraPropertyTypesPageCommand = new CommandType();
            ProductExtraPropertyTypesPageCommand.Create(_ => pageService.ShowProductExtraPropertyTypesPage());
            CategoriesPageCommand = new CommandType();
            CategoriesPageCommand.Create(_ => pageService.ShowCategoriesPage());
            OffersPageCommand = new CommandType();
            OffersPageCommand.Create(_ => pageService.ShowOffersPage());
            PicturesPageCommand = new CommandType();
            PicturesPageCommand.Create(_ => pageService.ShowPicturesPage());
            DescriptionsPageCommand = new CommandType();
            DescriptionsPageCommand.Create(_ => pageService.ShowDescriptionsPage());
        }

    }
}
