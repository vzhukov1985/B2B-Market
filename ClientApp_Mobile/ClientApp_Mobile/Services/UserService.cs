using ClientApp_Mobile.Models;
using Core.DBModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace ClientApp_Mobile.Services
{
    public class UserService
    {
        public static ClientUser CurrentUser;

        public static AppLocalUsers AppLocalUsers = new AppLocalUsers();
        public static int CurrentUserAppLocalUsersIndex;

        public static void GetUserInfoFromDb(Guid id)
        {
            try
            {
                using (MarketDbContext db = new MarketDbContext())
                {
                    CurrentUser = db.ClientsUsers
                                    .Where(u => u.Id == id)
                                    .Include(u => u.Favorites)
                                    .ThenInclude(f => f.Product)
                                    .Include(u => u.Client)
                                    .ThenInclude(c => c.Contracts)
                                    .ThenInclude(ct => ct.Supplier)
                                    .Include(u => u.Client)
                                    .ThenInclude(c => c.CurrentOrders)
                                    .ThenInclude(o => o.Offer)
                                    .ThenInclude(of => of.QuantityUnit)
                                    .Include(u => u.Client)
                                    .ThenInclude(c => c.CurrentOrders)
                                    .ThenInclude(o => o.Offer)
                                    .ThenInclude(of => of.Supplier)
                                    .FirstOrDefault();
                }

                CurrentUser.Client.ContractedSuppliersIDs = new List<Guid>(CurrentUser.Client.Contracts.Select(c => c.Supplier.Id));

            }
            catch
            {
                Device.BeginInvokeOnMainThread(() =>
                {
                    DialogService.ShowConnectionErrorDlg();
                });
                return;
            }
        }
    }
}

