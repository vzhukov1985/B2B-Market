using ClientApp_Mobile.ViewModels;
using ClientApp_Mobile.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace ClientApp_Mobile.Services
{
    public static class AppPageService
    {
        public static void GoToAuthorizationPage()
        {
            if (UserService.AppLocalUsers.Any(lu => lu.UsePINAccess == true))
            {
                GoToAuthPINPage();
            }
            else
            {
                GoToAuthPasswordPage();
            }
        }

        public static void GoToAuthPasswordPage()
        {
            var page = new AuthPasswordPage { BindingContext = new AuthPasswordPageVM() };
            Application.Current.MainPage = page;
        }

        public static void GoToAuthPINPage()
        {
            var page = new AuthPINPage { BindingContext = new AuthPINPageVM() };
            Application.Current.MainPage = page;
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
            var page = new MainPage { BindingContext = new MainPageVM() };
            if (UserService.AppLocalUsers.Any(lu => lu.UsePINAccess == true))
            {
                page.Items[2].Items[0].Items[0].ContentTemplate = new DataTemplate(typeof(AuthPINPage));
            }
            else
            {
                page.Items[2].Items[0].Items[0].ContentTemplate = new DataTemplate(typeof(AuthPasswordPage));
            }
            Application.Current.MainPage = page;
        }

        public static async Task<string> ShowSetPinPage(string title)
        {
            var source = new TaskCompletionSource<string>();
            var page = new PINSetPage { BindingContext = new PINSetPageVM(title, () => Application.Current.MainPage.Navigation.PopModalAsync()) }; ;
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
            var bc = new BiometricTestPageVM(() => Application.Current.MainPage.Navigation.PopModalAsync());
            var page = new BiometricTestPage() { BindingContext = bc };
            bc.ProceedCompleted += (bool result) =>
            {
                source.SetResult(result);
            };
            await Application.Current.MainPage.Navigation.PushModalAsync(page);
            return await source.Task;
        }

    }
}
