﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Administration_Tools.ViewModels;

namespace Administration_Tools_Win.Views
{
    /// <summary>
    /// Interaction logic for AddContractWithClientDlg.xaml
    /// </summary>
    public partial class AddContractWithClientDlg : Window
    {
        public AddContractWithClientDlg()
        {
            InitializeComponent();
        }

        private void AddContract_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }
    }
}
