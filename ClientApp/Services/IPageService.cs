using Core.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace ClientApp.Services
{
    public interface IPageService
    {
        void ShowAuthorizationPage();
        void ShowFirstLoginPage(ClientUser user);
        void ShowMainPage(ClientUser user);
        void ShowOffersSubPage(ClientUser user);
    }
}
