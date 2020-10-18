using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Core.DBModels;
using Core.Models;
using Core.Services;
using Newtonsoft.Json;
using Xamarin.Forms;

namespace ClientApp_Mobile.Services
{
    static class ApiConnect
    {
        private static HttpClient httpClient = new HttpClient();
        private static string token;

        public enum LoginResult
        {
            Invalid,
            Ok,
            PinAccessDenied
        }

        public static LoginResult Login(UserAuthParams loginParams)
        {
            var response = InvokeHTTPRequest(() =>
            {
                return httpClient.PostAsJsonAsync(CoreSettings.PrivateAPIUrl + "/api/auth", loginParams);
            }).Result;
            
            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                token = response.Content.ReadAsStringAsync().Result;
                httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                return LoginResult.Ok;
            }

            if (response.StatusCode == System.Net.HttpStatusCode.NoContent)
                return LoginResult.PinAccessDenied;

            return LoginResult.Invalid;
        }

        public static async Task<CurrentUserInfo> GetUserInfo()
        {
            var response = await InvokeHTTPRequest(() =>
            {
                return httpClient.GetAsync($"{CoreSettings.PrivateAPIUrl}/api/clientsusers/current/info");
            });

            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                return await response.Content.ReadAsAsync<CurrentUserInfo>();
            }
            else
            {
                return new CurrentUserInfo();
            }
        }

        public static async Task<bool> UpdateUserNameAndSurname(string Name, string Surname)
        {
            var response = await InvokeHTTPRequest(() =>
            {
                return httpClient.PutAsJsonAsync($"{CoreSettings.PrivateAPIUrl}/api/clientsusers/current/updnamesurname", new { Name, Surname });
            });

            if (response.StatusCode == System.Net.HttpStatusCode.OK)
                return true;

            return false;
        }

        public static async Task<bool> UpdateUserLogin(string login)
        {
            var response = await InvokeHTTPRequest(() =>
            {
                return httpClient.PutAsJsonAsync($"{CoreSettings.PrivateAPIUrl}/api/clientsusers/current/updlogin", login);
            });

            if (response.StatusCode == System.Net.HttpStatusCode.OK)
                return true;

            return false;
        }

        public static async Task<bool> UpdateUserPinAndPassword(string passwordHash, string pinHash)
        {
            var response = await InvokeHTTPRequest(() =>
            {
                return httpClient.PutAsJsonAsync($"{CoreSettings.PrivateAPIUrl}/api/clientsusers/current/updpwdpin", new { PasswordHash = passwordHash, PINHash = pinHash });
            });

            if (response.StatusCode == System.Net.HttpStatusCode.OK)
                return true;

            return false;
        }

        public static async Task<List<Guid>> GetUserFavoriteIds()
        {
            var response = await InvokeHTTPRequest(() =>
            {
                return httpClient.GetAsync($"{CoreSettings.PrivateAPIUrl}/api/favorites");
            });

            if (response.StatusCode == System.Net.HttpStatusCode.OK)
                return await response.Content.ReadAsAsync<List<Guid>>();

            return new List<Guid>();
        }

        public static async void AddRemoveProductToFavorites(string productId)
        {
            await InvokeHTTPRequest(() =>
            {               
                return httpClient.PutAsJsonAsync($"{CoreSettings.PrivateAPIUrl}/api/clientsusers/current/favorites", productId);
            });
        }

        public static async Task<List<TopCategory>> GetTopCategories()
        {
            var response = await InvokeHTTPRequest(() =>
            {
                return httpClient.GetAsync($"{CoreSettings.PrivateAPIUrl}/api/topcategories");
            });

            if (response.StatusCode == System.Net.HttpStatusCode.OK)
                return await response.Content.ReadAsAsync<List<TopCategory>>();

            return new List<TopCategory>();
        }

        public static async Task<List<MidCategory>> GetMidCategories(string topCategoryId, CancellationToken cancellationToken)
        {
            var response = await InvokeHTTPRequest(() =>
            {
                return httpClient.GetAsync($"{CoreSettings.PrivateAPIUrl}/api/midcategories?topcategoryid={topCategoryId}");
            });

            if (response.StatusCode == System.Net.HttpStatusCode.OK)
                return await response.Content.ReadAsAsync<List<MidCategory>>();

            return new List<MidCategory>();
        }

        public static async Task<List<ProductCategory>> GetProductCategoriesBasedOnMidCats(List<Guid> midcatguids, CancellationToken cancellationToken)
        {
            var response = await InvokeHTTPRequest(() =>
            {
                return httpClient.PostAsJsonAsync($"{CoreSettings.PrivateAPIUrl}/api/productcategories/getbasedonmidcats?onlyguids=false", midcatguids, cancellationToken);
            });

            if (response.StatusCode == System.Net.HttpStatusCode.OK)
                return await response.Content.ReadAsAsync<List<ProductCategory>>(cancellationToken);

            return new List<ProductCategory>();
        }

        public static async Task<List<Guid>> GetProductCategoriesIdsBasedOnMidCats(List<Guid> midcatguids, CancellationToken cancellationToken)
        {
            var response = await InvokeHTTPRequest(() =>
            {
                return httpClient.PostAsJsonAsync($"{CoreSettings.PrivateAPIUrl}/api/productcategories/getbasedonmidcats?onlyguids=true", midcatguids, cancellationToken);
            });

            if (response.StatusCode == System.Net.HttpStatusCode.OK)
                return await response.Content.ReadAsAsync<List<Guid>>(cancellationToken);

            return new List<Guid>();
        }

        public static async Task<List<SupplierIdName>> GetActiveSuppliersIdNames()
        {
            var response = await InvokeHTTPRequest(() =>
            {
                return httpClient.GetAsync($"{CoreSettings.PrivateAPIUrl}/api/suppliers?status=active&onlyidname=true");
            });

            if (response.StatusCode == System.Net.HttpStatusCode.OK)
                return await response.Content.ReadAsAsync<List<SupplierIdName>>();

            return new List<SupplierIdName>();
        }

        public static async Task<List<string>> GetAllClientsUsersLogins()
        {
            var response = await InvokeHTTPRequest(() =>
            {
                return httpClient.GetAsync($"{CoreSettings.PrivateAPIUrl}/api/clientsusers?onlylogins=true");
            });

            if (response.StatusCode == System.Net.HttpStatusCode.OK)
                return await response.Content.ReadAsAsync<List<string>>();

            return new List<string>();
        }

        public static async Task<List<ProductWithOffersDbView>> GetOffers(List<Guid> categoryFilter, List<Guid> supplierFilter, string searchText, bool queryFavoritesOnly, List<Guid> contractedSuppliersIds, CancellationToken cancellationToken)
        {
            var response = await InvokeHTTPRequest(() =>
            {
                return httpClient.PostAsJsonAsync($"{CoreSettings.PrivateAPIUrl}/api/offers/getbyparams", new { categoryFilter, supplierFilter, searchText, queryFavoritesOnly, contractedSuppliersIds }, cancellationToken);
            });

            if (response.StatusCode == System.Net.HttpStatusCode.OK)
                return await response.Content.ReadAsAsync<List<ProductWithOffersDbView>>(cancellationToken);

            return new List<ProductWithOffersDbView>();
        }

        public static async Task<List<ProductExtraProperty>> GetExtraPropertiesByProduct(Guid productId)
        {
            var response = await InvokeHTTPRequest(() =>
            {
                return httpClient.PostAsJsonAsync($"{CoreSettings.PrivateAPIUrl}/api/peps/getbyproduct", productId);
            });

            if (response.StatusCode == System.Net.HttpStatusCode.OK)
                return await response.Content.ReadAsAsync<List<ProductExtraProperty>>();

            return new List<ProductExtraProperty>();
        }

        public static async Task<string> GetProductDescription(Guid productId)
        {
            var response = await InvokeHTTPRequest(() =>
            {
                return httpClient.PostAsJsonAsync($"{CoreSettings.PrivateAPIUrl}/api/proddesc/getbyproduct", productId);
            });

            if (response.StatusCode == System.Net.HttpStatusCode.OK)
                return await response.Content.ReadAsStringAsync();

            return null;
        }

        public static async Task<List<Offer>> GetOffersByProduct(Guid productId)
        {
            var response = await InvokeHTTPRequest(() =>
            {
                return httpClient.PostAsJsonAsync($"{CoreSettings.PrivateAPIUrl}/api/offers/getbyproduct", productId);
            });

            if (response.StatusCode == System.Net.HttpStatusCode.OK)
                return await response.Content.ReadAsAsync<List<Offer>>();

            return new List<Offer>();
        }

        public static async Task<List<CurrentOrder>> GetCurrentOrdersByProduct(Guid clientId, Guid productId)
        {
            var response = await InvokeHTTPRequest(() =>
            {
                return httpClient.PostAsJsonAsync($"{CoreSettings.PrivateAPIUrl}/api/clientsusers/current/currentorders/getbyproduct", new { clientId, productId });
            });

            if (response.StatusCode == System.Net.HttpStatusCode.OK)
                return await response.Content.ReadAsAsync<List<CurrentOrder>>();

            return new List<CurrentOrder>();
        }

        public static async Task<CurrentOrder> UpdateCurrentOrder(CurrentOrder order)
        {
            var response = await InvokeHTTPRequest(() =>
            {
                return httpClient.PutAsJsonAsync($"{CoreSettings.PrivateAPIUrl}/api/clientsusers/current/currentorders", order);
            });

            if (response.StatusCode == System.Net.HttpStatusCode.OK)
                return await response.Content.ReadAsAsync<CurrentOrder>();

            return null;
        }

        public static async Task<List<OrderFromDbView>> GetClientCurrentOrders(Guid clientId, List<Guid> contractedSuppliersIds, CancellationToken cancellationToken)
        {
            var response = await InvokeHTTPRequest(() =>
            {
                return httpClient.PostAsJsonAsync($"{CoreSettings.PrivateAPIUrl}/api/clientsusers/current/currentorders", new { ClientId = clientId, ContractedSuppliersIds = contractedSuppliersIds }, cancellationToken);
            });

            if (response.StatusCode == System.Net.HttpStatusCode.OK)
                return await response.Content.ReadAsAsync<List<OrderFromDbView>>();

            return null;
        }

        public static async Task<bool> RemoveClientCurrentOrders(Guid clientId, List<Guid> offerIds)
        {
            var response = await InvokeHTTPRequest(() =>
            {
                return httpClient.PostAsJsonAsync($"{CoreSettings.PrivateAPIUrl}/api/clientsusers/current/currentorders/remove", new { clientId, offerIds });
            });

            if (response.StatusCode == System.Net.HttpStatusCode.OK)
                return true;

            return false;
        }

        public static async Task<List<Guid>> ProceedClientRequests(Guid clientId, List<RequestForConfirmation> requests)
        {
            string a = JsonConvert.SerializeObject(new { clientId, requests = requests });
           // string b = JsonConvert.SerializeObject(new { clientId, requests = new List<RequestForConfirmation>() });
            var response = await InvokeHTTPRequest(() =>
            {
                return httpClient.PostAsJsonAsync($"{CoreSettings.PrivateAPIUrl}/api/clientsusers/current/currentorders/proceed", a);
            });

            if (response.StatusCode == System.Net.HttpStatusCode.OK)
                return await response.Content.ReadAsAsync<List<Guid>>();

            return null;
        }

        public static async Task<List<ArchivedRequestStatusType>> GetArchivedOrderStatuses()
        {
            var response = await InvokeHTTPRequest(() =>
            {
                return httpClient.GetAsync($"{CoreSettings.PrivateAPIUrl}/api/archivedorders/statuses");
            });

            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                return await response.Content.ReadAsAsync<List<ArchivedRequestStatusType>>();
            }
            else
            {
                Device.BeginInvokeOnMainThread(() =>
                {
                    AppPageService.GoToAuthorizationPage();
                });
                return null;
            }
        }

        public static async Task<List<ArchivedRequestForClientDbView>> GetArchivedRequestsByClient(Guid clientId, CancellationToken cancellationToken)
        {
            var response = await InvokeHTTPRequest(() =>
            {
                return httpClient.PostAsJsonAsync($"{CoreSettings.PrivateAPIUrl}/api/clientsusers/current/archivedrequests", clientId, cancellationToken);
            });

            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                return await response.Content.ReadAsAsync<List<ArchivedRequestForClientDbView>>();
            }

            return new List<ArchivedRequestForClientDbView>();
        }

        public static async Task<ArchivedRequestDetails> GetArchivedRequestDetails(Guid requestId, CancellationToken cancellationToken)
        {
            var response = await InvokeHTTPRequest(() =>
            {
                return httpClient.PostAsJsonAsync($"{CoreSettings.PrivateAPIUrl}/api/clientsusers/current/archivedrequests/details", requestId, cancellationToken);
            });

            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                return await response.Content.ReadAsAsync<ArchivedRequestDetails>();
            }

            return new ArchivedRequestDetails();
        }


        public static async Task<List<ArchivedRequestsStatus>> GetArchivedRequestStatuses(Guid requestId, CancellationToken cancellationToken)
        {
            var response = await InvokeHTTPRequest(() =>
            {
                return httpClient.PostAsJsonAsync($"{CoreSettings.PrivateAPIUrl}/api/clientsusers/current/archivedrequests/statuses", requestId, cancellationToken);
            });

            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                return await response.Content.ReadAsAsync<List<ArchivedRequestsStatus>>();
            }

            return new List<ArchivedRequestsStatus>();
        }

        private static async Task<HttpResponseMessage> InvokeHTTPRequest(Func<Task<HttpResponseMessage>> request)
        {
            HttpResponseMessage response;
            try
            {
                response = await request();
            }
            catch
            {
                Device.BeginInvokeOnMainThread(() => DialogService.ShowConnectionErrorDlg());
                return new HttpResponseMessage(System.Net.HttpStatusCode.RequestTimeout);
            }

            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                
                Device.BeginInvokeOnMainThread(() =>
                {
                    AppPageService.GoToAuthorizationPage();
                    DialogService.ShowErrorDlg("Время сессии истекло. Необходима авторизация");
                });
            }

            if (response.StatusCode == System.Net.HttpStatusCode.InternalServerError)
            {
                Device.BeginInvokeOnMainThread(() => DialogService.ShowConnectionErrorDlg());
            }
            return response;
        }
    }
}
