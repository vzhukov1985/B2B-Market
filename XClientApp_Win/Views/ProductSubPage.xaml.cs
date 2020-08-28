﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace XClientApp_Win.Views
{
    /// <summary>
    /// Interaction logic for ProductSubPage.xaml
    /// </summary>
    public partial class ProductSubPage : Page
    {
        public ProductSubPage()
        {
            InitializeComponent();
        }
        private void Favourite_MouseDown(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
        }

        private void OrderQuantity_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = Regex.IsMatch(e.Text, "[^0-9]+");
        }

        private void Image_ImageFailed(object sender, ExceptionRoutedEventArgs e)
        {
            var image = new BitmapImage();
            using (var mem = new MemoryStream(XClientApp.Resources.Images.EmptyPicture))
            {
                mem.Position = 0;
                image.BeginInit();
                image.CreateOptions = BitmapCreateOptions.PreservePixelFormat;
                image.CacheOption = BitmapCacheOption.OnLoad;
                image.UriSource = null;
                image.StreamSource = mem;
                image.EndInit();
            }
            ((Image)sender).Source = image;
        }
    }
}