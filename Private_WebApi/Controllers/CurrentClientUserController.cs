using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Core.DBModels;
using Core.Models;
using Core.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace Private_WebApi.Controllers
{
    [ApiController]
    [Authorize(Roles = "ClientUser")]
    [Route("api/clientsusers/current")]
    public class CurrentClientUserController : Controller
    {
        private MarketDbContext db;

        public CurrentClientUserController(MarketDbContext dbContext)
        {
            db = dbContext;
        }

        // Get initial user info after clientapp login
        [HttpGet("info")]
        public async Task<IActionResult> GetInitialUserInfo()
        {

            Guid userId;
            if (!Guid.TryParse(User.Identity.Name, out userId))
                return StatusCode(500);

            try
            {
                var res = await db.ClientsUsers.Where(cu => cu.Id == userId).Select(cu => new CurrentUserInfo
                {
                    Id = cu.Id,
                    Login = cu.Login,
                    Name = cu.Name,
                    Surname = cu.Surname,
                    IsAdmin = cu.IsAdmin,
                    Client = new CurrentUserClientInfo
                    {
                        Id = cu.ClientId,
                        ShortName = cu.Client.ShortName,
                        FullName = cu.Client.FullName,
                        ContractedSuppliersIDs = cu.Client.Contracts.Select(c => c.SupplierId).ToList(),
                        Address = cu.Client.Address,
                        Bin = cu.Client.Bin,
                        City = cu.Client.City,
                        Country = cu.Client.Country,
                        Email = cu.Client.Email,
                        Phone = cu.Client.Phone
                    },
                    InitialPassword = cu.InitialPassword,
                    PasswordHash = cu.PasswordHash,
                    PinHash = cu.PinHash
                }).FirstOrDefaultAsync();

                res.FavoriteProductsIds = await db.Favorites.Where(f => f.ClientUserId == userId).Select(f => f.ProductId).ToListAsync();

                return Ok(res);
            }
            catch
            {
                return StatusCode(500);
            }
        }

        [HttpPut("updnamesurname")]
        public async Task<IActionResult> UpdateNameAndSurname(UserNameSurname userNameSurname)
        {
            try
            {
                ClientUser userToUpdate = new ClientUser { Id = new Guid(User.Identity.Name), Name = userNameSurname.Name, Surname = userNameSurname.Surname };
                db.ClientsUsers.Attach(userToUpdate);
                db.Entry(userToUpdate).Property(e => e.Name).IsModified = true;
                db.Entry(userToUpdate).Property(e => e.Surname).IsModified = true;
                await db.SaveChangesAsync();
                return Ok();
            }
            catch
            {
                return StatusCode(500);
            }
        }
        public class UserNameSurname
        {
            public string Name { get; set; }
            public string Surname { get; set; }
        }

        [HttpPut("updlogin")]
        public async Task<IActionResult> UpdateLogin([FromBody] string login)
        {
            try
            {
                ClientUser userToUpdate = new ClientUser { Id = new Guid(User.Identity.Name), Login = login };
                db.ClientsUsers.Attach(userToUpdate);
                db.Entry(userToUpdate).Property(e => e.Login).IsModified = true;
                await db.SaveChangesAsync();
                return Ok();
            }
            catch
            {
                return StatusCode(500);
            }
        }

        [HttpPut("updpwdpin")]
        public async Task<IActionResult> UpdatePasswordAndPin(PasswordPinInfo passwordPinInfo)
        {
            try
            {
                ClientUser userToUpdate = new ClientUser { Id = new Guid(User.Identity.Name), PasswordHash = passwordPinInfo.PasswordHash, PinHash = passwordPinInfo.PINHash };
                db.ClientsUsers.Attach(userToUpdate);
                db.Entry(userToUpdate).Property(e => e.PasswordHash).IsModified = true;
                db.Entry(userToUpdate).Property(e => e.PinHash).IsModified = true;
                await db.SaveChangesAsync();
                return Ok();
            }
            catch
            {
                return StatusCode(500);
            }
        }
        public class PasswordPinInfo
        {
            public string PasswordHash { get; set; }
            public string PINHash { get; set; }
        }

        [HttpGet("favorites")]
        public async Task<IActionResult> GetUserFavoritesIds()
        {
            try
            {
                return Ok(await db.Favorites.Where(f => f.ClientUserId == new Guid(User.Identity.Name)).Select(f => f.ProductId).ToListAsync());
            }
            catch
            {
                return StatusCode(500);
            }
        }

        [HttpPut("favorites")]
        public async Task<IActionResult> AddRemoveProductFavorites([FromBody] string productId)
        {
            string userId = User.Identity.Name;
            Guid userIdGuid, productIdGuid;
            try
            {
                userIdGuid = Guid.Parse(userId);
                productIdGuid = Guid.Parse(productId);

                var fav = db.Favorites.Where(f => f.ClientUserId == userIdGuid && f.ProductId == productIdGuid).FirstOrDefault();

                if (fav == null)
                {
                    db.Favorites.Add(new Favorite { ClientUserId = userIdGuid, ProductId = productIdGuid });
                }
                else
                {
                    db.Favorites.Remove(fav);
                }
                await db.SaveChangesAsync();

                return Ok();
            }
            catch
            {
                return StatusCode(500);
            }
        }

        [HttpPost("currentorders/getbyproduct")]
        public async Task<IActionResult> GetCurrentOrdersByProduct([FromBody] GetCurrentOrdersByProductParams productOrdersParams)
        {
            try
            {
                return Ok(
                    await db.CurrentOrders.Where(co => co.ClientId == productOrdersParams.ClientId && co.Offer.ProductId == productOrdersParams.ProductId).ToListAsync());
            }
            catch
            {
                return StatusCode(500);
            }
        }
        public class GetCurrentOrdersByProductParams
        {
            public Guid ClientId { get; set; }
            public Guid ProductId { get; set; }
        }

        [HttpPost("currentorders")]
        public async Task<IActionResult> GetClientCurrentOrders([FromBody] GetCurrentOrdersParams currentOrderParams)
        {
            try
            {
                using (MarketDbContext db = new MarketDbContext())
                {
                    return Ok(await db.CurrentOrders
                                         .Where(co => co.ClientId == currentOrderParams.ClientId)
                                         .Select(co => new OrderFromDbView
                                         {
                                             ProductId = co.Offer.Product.Id,
                                             ProductName = co.Offer.Product.Name,
                                             ProductPictureUri = co.Offer.Product.PictureUri,
                                             ProductTopCategoryId = co.Offer.Product.Category.MidCategory.TopCategoryId,
                                             ProductTopCategoryName = co.Offer.Product.Category.MidCategory.TopCategory.Name,
                                             ProductCategoryName = co.Offer.Product.Category.Name,
                                             VolumeType = co.Offer.Product.VolumeType.Name,
                                             Volume = co.Offer.Product.Volume,
                                             VolumeUnit = co.Offer.Product.VolumeUnit.ShortName,
                                             OfferId = co.OfferId,
                                             IsActive = co.Offer.IsActive,
                                             IsSupplierActive = co.Offer.Supplier.IsActive,
                                             OrderQuantity = co.Quantity,
                                             PriceForClient = currentOrderParams.ContractedSuppliersIds.Contains(co.Offer.SupplierId) ? co.Offer.DiscountPrice : co.Offer.RetailPrice,
                                             QuantityUnit = co.Offer.QuantityUnit.ShortName,
                                             Remains = co.Offer.Remains,
                                             SupplierId = co.Offer.SupplierId,
                                             SupplierShortName = co.Offer.Supplier.ShortName,
                                             SupplierCountry = co.Offer.Supplier.Country,
                                             SupplierCity = co.Offer.Supplier.City,
                                             SupplierAddress = co.Offer.Supplier.Address,
                                             SupplierBin = co.Offer.Supplier.Bin,
                                             SupplierEmail = co.Offer.Supplier.Email,
                                             SupplierFullName = co.Offer.Supplier.FullName,
                                             SupplierPhone = co.Offer.Supplier.Phone,
                                             ProductCode = co.Offer.Product.Code,
                                             SupplierProductCode = co.Offer.SupplierProductCode,
                                             SupplierFTPUser = co.Offer.Supplier.FTPUser,
                                             SupplierContactPersonName = co.Offer.Supplier.ContactPersonName,
                                             SupplierContactPersonPhone = co.Offer.Supplier.ContactPersonPhone
                                         }).ToListAsync());
                }
            }

            catch
            {
                return StatusCode(500);
            }
        }
        public class GetCurrentOrdersParams
        {
            public Guid ClientId { get; set; }
            public List<Guid> ContractedSuppliersIds { get; set; }
        }

        [HttpPut("currentorders")]
        public async Task<IActionResult> UpdateCurrentOrder([FromBody] CurrentOrder order)
        {
            try
            {
                var curOrder = db.CurrentOrders.Where(co => co.ClientId == order.ClientId && co.OfferId == order.OfferId).FirstOrDefault();
                CurrentOrder res = null;
                if (curOrder == null && order.Quantity > 0)
                {
                    db.CurrentOrders.Add(order);
                    res = order;
                }
                if (curOrder != null && order.Quantity == 0)
                {
                    db.CurrentOrders.Remove(curOrder);
                    res = order;
                }
                if (curOrder != null && order.Quantity > 0)
                {
                    curOrder.Quantity = order.Quantity;
                    db.CurrentOrders.Update(curOrder);
                    res = curOrder;
                }
                await db.SaveChangesAsync();
                return Ok(res);
            }
            catch
            {
                return StatusCode(500);
            }
        }

        [HttpPost("currentorders/remove")]
        public async Task<IActionResult> RemoveClientCurrentOrders([FromBody] RemoveCurrentOrdersParams removeCurrentOrdersParams)
        {
            try
            {
                var ordersToRemove = removeCurrentOrdersParams.offerIds.Select(oid => new CurrentOrder
                {
                    ClientId = removeCurrentOrdersParams.ClientId,
                    OfferId = oid
                });

                db.RemoveRange(ordersToRemove);
                await db.SaveChangesAsync();
                return Ok();
            }
            catch
            {
                return StatusCode(500);
            }
        }
        public class RemoveCurrentOrdersParams
        {
            public Guid ClientId { get; set; }
            public List<Guid> offerIds { get; set; }
        }

        [HttpPost("currentorders/proceed")]
        public async Task<IActionResult> ProceedCurrentOrders([FromBody] string content)
        {
            ProceedRequestParams requestsParams = JsonConvert.DeserializeObject<ProceedRequestParams>(content);

            List<Guid> unprocessedOrdersIds = new List<Guid>();



            try
            {
                var archivedClient = await db.ArchivedClients.Where(c => c.Id == requestsParams.Requests.FirstOrDefault().ClientId).FirstOrDefaultAsync();
                if (archivedClient == null)
                {
                    await db.ArchivedClients.AddAsync(ArchivedClient.CloneForDb(requestsParams.Requests.FirstOrDefault().ArchivedClient));
                }
                else
                {
                    db.Entry(archivedClient).State = EntityState.Detached;
                    archivedClient = requestsParams.Requests.FirstOrDefault().ArchivedClient;
                    db.Entry(archivedClient).State = EntityState.Modified;
                    db.ArchivedClients.Update(archivedClient);
                }

                await db.SaveChangesAsync();
                foreach (var request in requestsParams.Requests)
                {

                    int processedOrders = 0;
                    foreach (var order in request.OrdersToConfirm)
                    {
                        var offerToUpdateRemains = db.Offers.Where(o => o.Id == order.OfferId).FirstOrDefault();
                        if (offerToUpdateRemains.Remains >= order.Quantity)
                        {
                            offerToUpdateRemains.Remains -= order.Quantity;

                            db.Offers.Attach(offerToUpdateRemains);
                            db.Entry(offerToUpdateRemains).Property(o => o.Remains).IsModified = true;
                            db.Offers.Update(offerToUpdateRemains);
                            await db.SaveChangesAsync();
                            processedOrders++;
                        }
                        else
                        {
                            unprocessedOrdersIds.Add(order.OfferId);
                        }
                    }


                    request.OrdersToConfirm.RemoveAll(oc => unprocessedOrdersIds.Contains(oc.OfferId));

                    if (db.ArchivedRequests.Count() > 0)
                    {
                        request.Code = db.ArchivedRequests.Select(ar => ar.Code).Max() + 1;
                    }
                    else
                    {
                        request.Code = 1;
                    }

                    if (RequestFilesProcessor.CreateNewRequestFileForSupplierLocally(request))
                    {
                        var archivedSupplier = await db.ArchivedSuppliers.Where(s => s.Id == request.SupplierId).FirstOrDefaultAsync();
                        if (archivedSupplier == null)
                        {
                            await db.ArchivedSuppliers.AddAsync(ArchivedSupplier.CloneForDB(request.ArchivedSupplier));
                        }
                        else
                        {
                            db.Entry(archivedSupplier).State = EntityState.Detached;
                            archivedSupplier = request.ArchivedSupplier;
                            db.Entry(archivedSupplier).State = EntityState.Modified;
                            db.ArchivedSuppliers.Update(archivedSupplier);
                        }


                        await db.ArchivedRequests.AddAsync(ArchivedRequest.CloneForDb(request));


                        foreach (ArchivedRequestsStatus status in request.ArchivedRequestsStatuses)
                        {
                            status.ArchivedRequestStatusType = null;
                            await db.ArchivedRequestsStatuses.AddAsync(ArchivedRequestsStatus.CloneForDb(status));
                        }

                        await db.ArchivedOrders.AddRangeAsync(request.OrdersToConfirm.Select(ao => ArchivedOrder.CloneForDB(ao)));

                        db.CurrentOrders.RemoveRange(request.OrdersToConfirm.Select(oc => new CurrentOrder { ClientId = requestsParams.ClientId, OfferId = oc.OfferId }));
                        await db.SaveChangesAsync();
                    }
                    else
                    {
                        unprocessedOrdersIds.AddRange(request.OrdersToConfirm.Select(oc => oc.OfferId));
                    }
                }

                return Ok(unprocessedOrdersIds);
            }
            catch
            {
                return StatusCode(500);
            }
        }

        public class ProceedRequestParams
        {
            public Guid ClientId { get; set; }
            public List<RequestForConfirmation> Requests { get; set; }
        }

        [HttpPost("archivedrequests")]
        public async Task<IActionResult> GetClientArchivedRequests([FromBody] Guid clientId)
        {
            try
            {
                return Ok(await db.ArchivedRequests.Where(ar => ar.ClientId == clientId).Select(ar => new ArchivedRequestForClientDbView
                {
                    ArchivedSupplierName = ar.ArchivedSupplier.ShortName,
                    Code = ar.Code,
                    DateTimeSent = ar.DateTimeSent,
                    ItemsQuantity = ar.ItemsQuantity,
                    ProductsQuantity = ar.ProductsQuantity,
                    SenderName = ar.SenderName,
                    SenderSurname = ar.SenderSurname,
                    Id = ar.Id,
                    TotalPrice = ar.TotalPrice,
                    StatusName = ar.ArchivedRequestsStatuses == null ? "NONE" : ar.ArchivedRequestsStatuses.OrderBy(st => st.DateTime).Last().ArchivedRequestStatusType.Name,
                    StatusDescription = ar.ArchivedRequestsStatuses == null ? "Нет данных" : ar.ArchivedRequestsStatuses.OrderBy(st => st.DateTime).Last().ArchivedRequestStatusType.Description
                }).ToListAsync());
            }
            catch
            {
                return StatusCode(500);
            }
        }

        [HttpPost("archivedrequests/details")]
        public async Task<IActionResult> GetClientArchivedOrdersByRequest([FromBody] Guid requestId)
        {
            try
            {
                var res = await db.ArchivedRequests.Where(ar => ar.Id == requestId).Select(ar => new ArchivedRequestDetails
                {
                    ArchivedOrders = ar.ArchivedOrders,
                    DeliveryDateTime = ar.DeliveryDateTime,
                    Comments = ar.Comments
                }).FirstOrDefaultAsync();
                return Ok(res);
            }
            catch
            {
                return StatusCode(500);
            }
        }

        [HttpPost("archivedrequests/statuses")]
        public async Task<IActionResult> GetClientArchivedRequestStatuses([FromBody] Guid requestId)
        {
            try
            {
                var res = await db.ArchivedRequests.Where(ar => ar.Id == requestId)
                                                   .Include(ar => ar.ArchivedRequestsStatuses)
                                                   .ThenInclude(arst => arst.ArchivedRequestStatusType)
                                                   .Select(ar => ar.ArchivedRequestsStatuses)
                                                   .FirstOrDefaultAsync();
                return Ok(res);
            }
            catch
            {
                return StatusCode(500);
            }
        }
    }
}
