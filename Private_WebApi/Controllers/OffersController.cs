using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Core.DBModels;
using Core.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Private_WebApi.Controllers
{
    [ApiController]
    [Authorize(Roles = "ClientUser")]
    [Route("api/offers")]
    public class OffersController : Controller
    {
        MarketDbContext db;
        public OffersController(MarketDbContext dbContext)
        {
            db = dbContext;
        }

        [HttpPost("getbyparams")]
        public async Task<IActionResult> GetOffersByParams(OffersRequestParams requestParams)
        {
            var categoryFilter = requestParams.CategoryFilter;
            var supplierFilter = requestParams.SupplierFilter;
            var searchText = requestParams.SearchText;
            var queryFavoritesOnly = requestParams.QueryFavoritesOnly;
            var contractedSuppliersIds = requestParams.ContractedSuppliersIds;

            try
            {
                using (MarketDbContext db = new MarketDbContext())
                {
                    IQueryable<Product> queryPart;

                    if (queryFavoritesOnly)
                    {
                        queryPart = db.Favorites
                                      .Where(f => f.ClientUserId == new Guid(User.Identity.Name))
                                      .Select(f => f.Product);
                    }
                    else
                    {
                        queryPart = db.Products
                                      .Where(p => p.Offers.Any(of => of.Supplier.IsActive == true && of.Remains > 0 && of.IsActive == true))
                                      .Where(p => categoryFilter == null ? true : categoryFilter.Contains(p.CategoryId))
                                      .Where(p => supplierFilter == null ? true : p.Offers.Select(of => new { of.SupplierId, of.Remains }).Any(of => supplierFilter.Contains(of.SupplierId) && of.Remains > 0))
                                      .Where(p => string.IsNullOrEmpty(searchText) ? true : EF.Functions.Like(p.Name, $"%{searchText}%") || EF.Functions.Like(p.Category.Name, $"%{searchText}%"));
                    }

                    return Ok(await queryPart
                                       .OrderBy(p => p.Category.Name)
                                       .ThenBy(p => p.Name)
                                       .Select(p => new ProductWithOffersDbView
                                       {
                                           Id = p.Id,
                                           Name = p.Name,
                                           CategoryName = p.Category.Name,
                                           PictureUri = p.PictureUri,
                                           VolumeType = p.VolumeType.Name,
                                           Volume = p.Volume,
                                           VolumeUnit = p.VolumeUnit.ShortName,
                                           IsOfContractedSupplier = p.Offers.Any(o => contractedSuppliersIds.Contains(o.Supplier.Id)),
                                           BestRetailOffer = db.Offers
                                                               .Where(o => o.ProductId == p.Id && o.IsActive == true && o.Supplier.IsActive == true)
                                                               .OrderByDescending(o => contractedSuppliersIds.Contains(o.SupplierId))
                                                               .ThenBy(o => o.RetailPrice)
                                                               .Select(o => new ProductWithOffersDbView.BestOffer
                                                               {
                                                                   Price = o.RetailPrice,
                                                                   QuantityUnit = o.QuantityUnit.ShortName,
                                                                   SupplierName = o.Supplier.ShortName
                                                               }).FirstOrDefault(),
                                           BestDiscountOffer = db.Offers
                                                               .Where(o => o.ProductId == p.Id && o.IsActive == true && o.Supplier.IsActive == true)
                                                               .OrderByDescending(o => contractedSuppliersIds.Contains(o.SupplierId))
                                                               .ThenBy(o => o.DiscountPrice)
                                                               .Select(o => new ProductWithOffersDbView.BestOffer
                                                               {
                                                                   Price = o.DiscountPrice,
                                                                   QuantityUnit = o.QuantityUnit.ShortName,
                                                                   SupplierName = o.Supplier.ShortName
                                                               }).FirstOrDefault(),
                                       })
                                       .ToListAsync());
                }
            }
            catch
            {
                return StatusCode(500);
            }
        }
        public class OffersRequestParams
        {
            public List<Guid> CategoryFilter { get; set; }
            public List<Guid> SupplierFilter { get; set; }
            public string SearchText { get; set; }
            public bool QueryFavoritesOnly { get; set; }
            public List<Guid> ContractedSuppliersIds { get; set; }
        }

        [HttpPost("getbyproduct")]
        public async Task<IActionResult> GetOffersByProduct([FromBody] Guid productId)
        {
            try
            {
                return Ok(await db.Offers
                    .Where(o => o.ProductId == productId)
                    .Select(o => new Offer
                    {
                        Id = o.Id,
                        DiscountPrice = o.DiscountPrice,
                        QuantityUnit = new QuantityUnit { ShortName = o.QuantityUnit.ShortName },
                        Remains = o.Remains,
                        RetailPrice = o.RetailPrice,
                        Supplier = new Supplier { Id = o.Supplier.Id, ShortName = o.Supplier.ShortName, IsActive = o.Supplier.IsActive },
                        IsActive = o.IsActive
                    })
                    .ToListAsync());
            }
            catch
            {
                return StatusCode(500);
            }
        }
    }
}
