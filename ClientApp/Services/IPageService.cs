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

        void SubPageNavigationBack();

        void ShowMainSubPage(ClientUser user);
        void ShowMidCategoriesSubPage(ClientUser user, TopCategory topCategory);
        void ShowProductCategoriesSubPage(ClientUser user, MidCategory midCategory);
        void ShowOffersSubPage(ClientUser user, string title, List<Guid> categoryFilter, List <Guid> supplierFilter, string searchText = "");
        void ShowProductSubPage(ClientUser user, Product product);

        void ShowSearchSubPage(ClientUser user);

        void ShowFavoritesSubPage(ClientUser user);

        void ShowCurrentRequestSubPage(ClientUser user);
        void ShowArchivedRequestsListSubPage(ClientUser user);
        void ShowArchivedRequestSubPage(ClientUser user, ArchivedRequest request);
    }
}
