﻿using OperatorApp.ViewModels;
using OperatorApp_Win.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace OperatorApp_Win.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            WindowsPageService.NavigationService = frmMain.NavigationService;
            WindowsPageService pageService = new WindowsPageService();
            DataContext = new MainWindowVM<RelayCommand>(pageService);
            pageService.ShowQuantityUnitsPage();

        }
    }
}
