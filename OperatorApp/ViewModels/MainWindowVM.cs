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
        public CommandType ProductCategoriesPageCommand { get; }
        public CommandType MidCategoriesPageCommand { get; }
        public CommandType TopCategoriesPageCommand { get; }
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
            ProductCategoriesPageCommand = new CommandType();
            ProductCategoriesPageCommand.Create(_ => pageService.ShowProductCategoriesPage());
            MidCategoriesPageCommand = new CommandType();
            MidCategoriesPageCommand.Create(_ => pageService.ShowMidCategoriesPage());
            TopCategoriesPageCommand = new CommandType();
            TopCategoriesPageCommand.Create(_ => pageService.ShowTopCategoriesPage());
            OffersPageCommand = new CommandType();
            OffersPageCommand.Create(_ => pageService.ShowOffersPage());
            PicturesPageCommand = new CommandType();
            PicturesPageCommand.Create(_ => pageService.ShowPicturesPage());
            DescriptionsPageCommand = new CommandType();
            DescriptionsPageCommand.Create(_ => pageService.ShowDescriptionsPage());
        }

    }
}
