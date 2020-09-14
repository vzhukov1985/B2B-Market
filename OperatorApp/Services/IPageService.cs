using System;
using System.Collections.Generic;
using System.Text;

namespace OperatorApp.Services
{
    public interface IPageService
    {
        void ShowQuantityUnitsPage();
        void ShowVolumeTypesPage();
        void ShowVolumeUnitsPage();
        void ShowProductExtraPropertyTypesPage();
        void ShowCategoriesPage();
        void ShowOffersPage();
        void ShowPicturesPage();
        void ShowDescriptionsPage();
    }
}
