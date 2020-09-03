using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace ClientApp_Mobile.Services
{
    public static class DialogService
    {
        public static void ShowConnectionErrorDlg()
        {
            ShowErrorDlg("Нет соединения с сервером. Попробуйте позднее.");
        }
        public static async void ShowErrorDlg(string Text)
        {
            await Application.Current.MainPage.DisplayAlert("Ошибка", Text, "OK");
        }

        public static async void ShowMessageDlg(string Text, string caption)
        {
            await Application.Current.MainPage.DisplayAlert(caption, Text, "OK");
        }

        public static async Task<bool> ShowOkCancelDialog(string text, string caption)
        {
            return await Application.Current.MainPage.DisplayAlert(caption, text, "OK", "Отмена");
        }

        public static async Task<string> ShowInputDialog(string prompt, string caption, Keyboard keyboardType, string initialText = "", string placehoder = "", int maxLength = -1)
        {
            return await Application.Current.MainPage.DisplayPromptAsync(caption, prompt, "OK", "Отмена", placehoder, maxLength, keyboardType, initialText);
        }
    }
}
