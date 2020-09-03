using ClientApp_Mobile.ViewModels;
using ClientApp_Mobile.Views;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace ClientApp_Mobile.Services
{
    public static class AppPageService
    {
        public static void GoToAuthPasswordPage()
        {
            Application.Current.MainPage = new AuthPasswordPage() { BindingContext = new AuthPasswordPageVM() };
        }

        public static void GoToAuthPINPage()
        {
            Application.Current.MainPage = new AuthPINPage() { BindingContext = new AuthPINPageVM() };
        }

        public static async void GoToFirstTimePasswordSetPage()
        {
            var page = new FirstTimePwdSetPage { BindingContext = new FirstTimePwdSetPageVM() };
            await Application.Current.MainPage.Navigation.PushModalAsync(page);
        }

        public static async void GoToFirstTimeSettingsPage(string PINCode)
        {
            var page = new FirstTimeSettingsPage { BindingContext = new FirstTimeSettingsPageVM(PINCode) };
            await Application.Current.MainPage.Navigation.PushModalAsync(page);
        }

        public static async void GoToFirstTimeReadyPage()
        {
            var page = new FirstTimeReadyPage { BindingContext = new FirstTimeReadyPageVM() };
            await Application.Current.MainPage.Navigation.PushModalAsync(page);
        }

        public static async void GoToNewLocalUserBiometricSettingsPage()
        {
            var page = new NewLocalUserBiometricSettingsPage { BindingContext = new NewLocalUserBiometricSettingsPageVM() };
            await Application.Current.MainPage.Navigation.PushModalAsync(page);
        }

        public static void GoToMainMage()
        {
            Application.Current.MainPage = new AppShell();
        }

        public static async Task<string> ShowSetPinPage(string title)
        {
            var source = new TaskCompletionSource<string>();
            var page = new PINSetPage { BindingContext = new PINSetPageVM(title) }; ;
            page.PageDisapearing += (pinCode) =>
            {
                source.SetResult(pinCode);
            };
            await Application.Current.MainPage.Navigation.PushModalAsync(page);
            return await source.Task;
        }

        public static async Task<bool> ShowBiometricTestPage()
        {
            var source = new TaskCompletionSource<bool>();
            var bc = new BiometricTestPageVM();
            var page = new BiometricTestPage() { BindingContext = bc };
            page.PageDisapearing += (Result) =>
            {
                source.SetResult(Result);
            };
            await Application.Current.MainPage.Navigation.PushModalAsync(page);
            return await source.Task;
        }

    }
}
