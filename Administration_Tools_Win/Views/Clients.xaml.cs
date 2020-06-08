using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Administration_Tools.ViewModels;
using Administration_Tools_Win.Services;

namespace Administration_Tools_Win.Views
{
    /// <summary>
    /// Interaction logic for Clients.xaml
    /// </summary>
    public partial class Clients : Page
    {
        public Clients()
        {
            InitializeComponent();
            DataContext = new ClientsVM<RelayCommand>(new WindowsPageService(), new WindowsDialogService());
        }
    }
}
