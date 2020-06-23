using ClientApp.ViewModels;
using ClientApp_Win.Services;
using System;
using System.Collections.Generic;
using System.Reflection;
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
    /// Interaction logic for FirstLoginPage.xaml
    /// </summary>
    public partial class FirstLoginPage : Page
    {
        public FirstLoginPage()
        {
            InitializeComponent();
        }

        private void ContinueButton_Click(object sender, RoutedEventArgs e)
        {
            ((FirstLoginPageVM<RelayCommand>)DataContext).SetNewPasswordCommand.Execute(new DoublePassword { Password1 = pwdBox1.Password, Password2 = pwdBox2.Password });
        }
    }
}
