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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ClientApp_Win.Views
{
    /// <summary>
    /// Interaction logic for OffersSubPage.xaml
    /// </summary>
    public partial class OffersSubPage : Page
    {

        public OffersSubPage()
        {
            InitializeComponent();
        }

        private void Favourite_MouseDown(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
        }
    }
}
