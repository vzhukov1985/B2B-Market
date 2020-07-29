using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Core.DBModels;
using Core;
using System.Configuration;
using System.Linq;
using System.Collections.ObjectModel;

namespace Core.DBModels
{
    public class MarketDbContext : DbContext
    {
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlServer(Settings.Default.DbConnectionString);
                optionsBuilder.EnableSensitiveDataLogging(true); //TODO: Delete while deployment
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Contract>().HasKey(k => new { k.ClientId, k.SupplierId });
            modelBuilder.Entity<Favorite>().HasKey(f => new { f.ClientUserId, f.ProductId });
            modelBuilder.Entity<CurrentOrder>().HasKey(k => new { k.ClientId, k.OfferId });
            modelBuilder.Entity<ArchivedRequestsStatus>().HasKey(k => new { k.ArchivedRequestId, k.ArchivedRequestStatusTypeId });

            modelBuilder.Entity<Contract>().HasOne(c => c.Client).WithMany(cc => cc.Contracts).HasForeignKey(c => c.ClientId);
            modelBuilder.Entity<Contract>().HasOne(s => s.Supplier).WithMany(sc => sc.Contracts).HasForeignKey(s => s.SupplierId);

            modelBuilder.Entity<ClientUser>().HasOne(c => c.Client).WithMany(cc => cc.Users).HasForeignKey(u => u.ClientId);

            modelBuilder.Entity<Product>().Property(p => p.Code).Metadata.SetAfterSaveBehavior(PropertySaveBehavior.Ignore);
            modelBuilder.Entity<Product>().HasOne(p => p.Description).WithOne().HasForeignKey<ProductDescription>(pd => pd.ProductId);

        }

        public virtual DbSet<Supplier> Suppliers { get; set; }
        public virtual DbSet<Client> Clients { get; set; }
        public virtual DbSet<Contract> Contracts { get; set; }
        public virtual DbSet<ClientUser> ClientsUsers { get; set; }
        public virtual DbSet<Product> Products { get; set; }
        public virtual DbSet<ProductCategory> ProductCategories { get; set; }
        public virtual DbSet<QuantityUnit> QuantityUnits { get; set; }
        public virtual DbSet<VolumeType> VolumeTypes { get; set; }
        public virtual DbSet<VolumeUnit> VolumeUnits { get; set; }
        public virtual DbSet<Favorite> Favorites { get; set; }
        public virtual DbSet<Offer> Offers { get; set; }
        public virtual DbSet<ProductExtraPropertyType> ProductExtraPropertyTypes { get; set; }
        public virtual DbSet<ProductExtraProperty> ProductExtraProperties { get; set; }
        public virtual DbSet<MidCategory> MidCategories { get; set; }
        public virtual DbSet<TopCategory> TopCategories { get; set; }
        public virtual DbSet<CurrentOrder> CurrentOrders { get; set; }
        public virtual DbSet<ArchivedRequestStatusType> ArchivedRequestStatusTypes { get; set; }
        public virtual DbSet<ArchivedRequest> ArchivedRequests { get; set; }
        public virtual DbSet<ArchivedOrder> ArchivedOrders { get; set; }
        public virtual DbSet<ArchivedSupplier> ArchivedSuppliers { get; set; }
        public virtual DbSet<ArchivedRequestsStatus> ArchivedRequestsStatuses { get; set; }
        public virtual DbSet<ProductDescription> ProductDescriptions { get; set; }
        public virtual DbSet<MatchProductExtraPropertyType> MatchProductExtraPropertyTypes { get; set; }
        public virtual DbSet<MatchProductExtraProperty> MatchProductExtraProperties { get; set; }
        public virtual DbSet<MatchProductCategory> MatchProductCategories { get; set; }
        public virtual DbSet<MatchQuantityUnit> MatchQuantityUnits { get; set; }
        public virtual DbSet<MatchVolumeType> MatchVolumeTypes { get; set; }
        public virtual DbSet<MatchVolumeUnit> MatchVolumeUnits { get; set; }
        public virtual DbSet<MatchOffer> MatchOffers { get; set; }
        public virtual DbSet<UnmatchedPic> UnmatchedPics { get; set; }
        public virtual DbSet<ConflictedPic> ConflictedPics { get; set; }
        public virtual DbSet<UnmatchedDescription> UnmatchedDescriptions { get; set; }
        public virtual DbSet<ConflictedDescription> ConflictedDescriptions { get; set; }



        // public static readonly string b2bDataDir = @"\\192.168.1.1\Media Server\B2B FTP Server"; Local
        public static readonly string b2bDataDir = @"D:/B2B FTP Server Mirror";

        public static void AddRemoveProductToFavourites(Product selectedProduct, ClientUser User)
        {
            using (MarketDbContext db = new MarketDbContext())
            {

                if (selectedProduct.IsFavoriteForUser)
                {
                    selectedProduct.IsFavoriteForUser = false;
                    Favorite favoriteToRemove = db.Favorites.Where(f => (f.ClientUserId == User.Id) && (f.ProductId == selectedProduct.Id)).FirstOrDefault();
                    db.Favorites.Remove(favoriteToRemove);
                }
                else
                {
                    selectedProduct.IsFavoriteForUser = true;
                    db.Favorites.Add(new Favorite
                    {
                        ClientUserId = User.Id,
                        ProductId = selectedProduct.Id
                    });
                }
                db.SaveChanges();
                User.FavoriteProducts = new ObservableCollection<Favorite>(db.Favorites.Include(f => f.Product).Where(f => f.ClientUserId == User.Id));
            }
        }

        public static List<Guid> GetUnusedMatchQuantityUnitsIds()
        {
            using (MarketDbContext db = new MarketDbContext())
            {
                IEnumerable<Guid> allMatchOffers = db.MatchOffers.Select(mo => mo.MatchQuantityUnitId).Distinct();
                return db.MatchQuantityUnits.Where(mqu => !allMatchOffers.Contains(mqu.Id)).Select(mqu => mqu.Id).ToList();
            }
        }
        public static List<MatchQuantityUnit> GetUnusedMatchQuantityUnits()
        {
            using (MarketDbContext db = new MarketDbContext())
            {
                IEnumerable<Guid> allMatchOffers = db.MatchOffers.Select(mo => mo.MatchQuantityUnitId).Distinct();
                return db.MatchQuantityUnits.Where(mqu => !allMatchOffers.Contains(mqu.Id)).Include(qu => qu.Supplier).ToList();
            }
        }
        public static List<Guid> GetUnusedQuantityUnitsIds()
        {
            using (MarketDbContext db = new MarketDbContext())
            {
                List<Guid> allMatchQuantityUnits = db.MatchQuantityUnits.Select(mqu => mqu.QuantityUnitId == null ? Guid.Empty : (Guid)mqu.QuantityUnitId).Distinct().ToList();
                List<Guid> allOffers = db.Offers.Select(o => o.QuantityUnitId).Distinct().ToList();
                List<Guid> allGuids = allMatchQuantityUnits.Union(allOffers).Distinct().ToList();
                return db.QuantityUnits.Where(mqu => !allGuids.Contains(mqu.Id)).Select(qu => qu.Id).ToList();
            }
        }

        public static List<QuantityUnit> GetUnusedQuantityUnits()
        {
            using (MarketDbContext db = new MarketDbContext())
            {
                List<Guid> allMatchQuantityUnits = db.MatchQuantityUnits.Select(mqu => mqu.QuantityUnitId == null ? Guid.Empty : (Guid)mqu.QuantityUnitId).Distinct().ToList();
                List<Guid> allOffers = db.Offers.Select(o => o.QuantityUnitId).Distinct().ToList();
                List<Guid> allGuids = allMatchQuantityUnits.Union(allOffers).Distinct().ToList();
                return db.QuantityUnits.Where(mqu => !allGuids.Contains(mqu.Id)).ToList();
            }
        }

        public static List<Guid> GetUnusedMatchVolumeTypesIds()
        {
            using (MarketDbContext db = new MarketDbContext())
            {
                IEnumerable<Guid> allMatchOffers = db.MatchOffers.Select(mo => mo.MatchVolumeTypeId).Distinct();
                return db.MatchVolumeTypes.Where(mvt => !allMatchOffers.Contains(mvt.Id)).Select(mvt => mvt.Id).ToList();
            }
        }
        public static List<MatchVolumeType> GetUnusedMatchVolumeTypes()
        {
            using (MarketDbContext db = new MarketDbContext())
            {
                IEnumerable<Guid> allMatchOffers = db.MatchOffers.Select(mo => mo.MatchVolumeTypeId).Distinct();
                return db.MatchVolumeTypes.Where(mvt => !allMatchOffers.Contains(mvt.Id)).Include(vt => vt.Supplier).ToList();
            }
        }
        public static List<Guid> GetUnusedVolumeTypesIds()
        {
            using (MarketDbContext db = new MarketDbContext())
            {
                List<Guid> allMatchVolumeTypes = db.MatchVolumeTypes.Select(mvt => mvt.VolumeTypeId == null ? Guid.Empty : (Guid)mvt.VolumeTypeId).Distinct().ToList();
                List<Guid> allProducts = db.Products.Select(p => p.VolumeTypeId).Distinct().ToList();
                List<Guid> allGuids = allMatchVolumeTypes.Union(allProducts).Distinct().ToList();
                return db.VolumeTypes.Where(mvt => !allGuids.Contains(mvt.Id)).Select(vt => vt.Id).ToList();
            }
        }

        public static List<VolumeType> GetUnusedVolumeTypes()
        {
            using (MarketDbContext db = new MarketDbContext())
            {
                List<Guid> allMatchVolumeTypes = db.MatchVolumeTypes.Select(mvt => mvt.VolumeTypeId == null ? Guid.Empty : (Guid)mvt.VolumeTypeId).Distinct().ToList();
                List<Guid> allProducts = db.Products.Select(p => p.VolumeTypeId).Distinct().ToList();
                List<Guid> allGuids = allMatchVolumeTypes.Union(allProducts).Distinct().ToList();
                return db.VolumeTypes.Where(mvt => !allGuids.Contains(mvt.Id)).ToList();
            }
        }
        public static List<Guid> GetUnusedMatchVolumeUnitsIds()
        {
            using (MarketDbContext db = new MarketDbContext())
            {
                IEnumerable<Guid> allMatchOffers = db.MatchOffers.Select(mo => mo.MatchVolumeUnitId).Distinct();
                return db.MatchVolumeUnits.Where(mvu => !allMatchOffers.Contains(mvu.Id)).Select(mvu => mvu.Id).ToList();
            }
        }
        public static List<MatchVolumeUnit> GetUnusedMatchVolumeUnits()
        {
            using (MarketDbContext db = new MarketDbContext())
            {
                IEnumerable<Guid> allMatchOffers = db.MatchOffers.Select(mo => mo.MatchVolumeUnitId).Distinct();
                return db.MatchVolumeUnits.Where(mvu => !allMatchOffers.Contains(mvu.Id)).Include(vu => vu.Supplier).ToList();
            }
        }
        public static List<Guid> GetUnusedVolumeUnitsIds()
        {
            using (MarketDbContext db = new MarketDbContext())
            {
                List<Guid> allMatchVolumeUnits = db.MatchVolumeUnits.Select(mvu => mvu.VolumeUnitId == null ? Guid.Empty : (Guid)mvu.VolumeUnitId).Distinct().ToList();
                List<Guid> allProducts = db.Products.Select(p => p.VolumeUnitId).Distinct().ToList();
                List<Guid> allGuids = allMatchVolumeUnits.Union(allProducts).Distinct().ToList();
                return db.VolumeUnits.Where(mvu => !allGuids.Contains(mvu.Id)).Select(vu => vu.Id).ToList();
            }
        }

        public static List<VolumeUnit> GetUnusedVolumeUnits()
        {
            using (MarketDbContext db = new MarketDbContext())
            {
                List<Guid> allMatchVolumeUnits = db.MatchVolumeUnits.Select(mvu => mvu.VolumeUnitId == null ? Guid.Empty : (Guid)mvu.VolumeUnitId).Distinct().ToList();
                List<Guid> allProducts = db.Products.Select(p => p.VolumeUnitId).Distinct().ToList();
                List<Guid> allGuids = allMatchVolumeUnits.Union(allProducts).Distinct().ToList();
                return db.VolumeUnits.Where(mvu => !allGuids.Contains(mvu.Id)).ToList();
            }
        }
        public static List<Guid> GetUnusedMatchProductExtraPropertyTypesIds()
        {
            using (MarketDbContext db = new MarketDbContext())
            {
                IEnumerable<Guid> allMatchExtraProperties = db.MatchProductExtraProperties.Select(mpep => mpep.MatchProductExtraPropertyTypeId).Distinct();
                return db.MatchProductExtraPropertyTypes.Where(mpept => !allMatchExtraProperties.Contains(mpept.Id)).Select(mpept => mpept.Id).ToList();
            }
        }
        public static List<MatchProductExtraPropertyType> GetUnusedMatchProductExtraPropertyTypes()
        {
            using (MarketDbContext db = new MarketDbContext())
            {
                IEnumerable<Guid> allMatchExtraProperties = db.MatchProductExtraProperties.Select(mpep => mpep.MatchProductExtraPropertyTypeId).Distinct();
                return db.MatchProductExtraPropertyTypes.Where(mpept => !allMatchExtraProperties.Contains(mpept.Id)).Include(pept => pept.Supplier).ToList();
            }
        }
        public static List<Guid> GetUnusedProductExtraPropertyTypesIds()
        {
            using (MarketDbContext db = new MarketDbContext())
            {
                List<Guid> allMatchProductExtraPropertyTypes = db.MatchProductExtraPropertyTypes.Select(mpept => mpept.ProductExtraPropertyTypeId == null ? Guid.Empty : (Guid)mpept.ProductExtraPropertyTypeId).Distinct().ToList();
                List<Guid> allExtraProperties = db.ProductExtraProperties.Select(p => p.PropertyTypeId).Distinct().ToList();
                List<Guid> allGuids = allMatchProductExtraPropertyTypes.Union(allExtraProperties).Distinct().ToList();
                return db.ProductExtraPropertyTypes.Where(mpept => !allGuids.Contains(mpept.Id)).Select(pept => pept.Id).ToList();
            }
        }

        public static List<ProductExtraPropertyType> GetUnusedProductExtraPropertyTypes()
        {
            using (MarketDbContext db = new MarketDbContext())
            {
                List<Guid> allMatchProductExtraPropertyTypes = db.MatchProductExtraPropertyTypes.Select(mpept => mpept.ProductExtraPropertyTypeId == null ? Guid.Empty : (Guid)mpept.ProductExtraPropertyTypeId).Distinct().ToList();
                List<Guid> allExtraProperties = db.ProductExtraProperties.Select(p => p.PropertyTypeId).Distinct().ToList();
                List<Guid> allGuids = allMatchProductExtraPropertyTypes.Union(allExtraProperties).Distinct().ToList();
                return db.ProductExtraPropertyTypes.Where(mpept => !allGuids.Contains(mpept.Id)).ToList();
            }
        }

        public static List<Guid> GetUnusedMatchProductCategoriesIds()
        {
            using (MarketDbContext db = new MarketDbContext())
            {
                IEnumerable<Guid> allMatchOffers = db.MatchOffers.Select(mo => mo.MatchProductCategoryId).Distinct();
                return db.MatchProductCategories.Where(mvt => !allMatchOffers.Contains(mvt.Id)).Select(mvt => mvt.Id).ToList();
            }
        }
        public static List<MatchProductCategory> GetUnusedMatchProductCategories()
        {
            using (MarketDbContext db = new MarketDbContext())
            {
                IEnumerable<Guid> allMatchOffers = db.MatchOffers.Select(mo => mo.MatchProductCategoryId).Distinct();
                return db.MatchProductCategories.Where(mvt => !allMatchOffers.Contains(mvt.Id)).Include(vt => vt.Supplier).ToList();
            }
        }
        public static List<Guid> GetUnusedProductCategoriesIds()
        {
            using (MarketDbContext db = new MarketDbContext())
            {
                List<Guid> allMatchProductCategories = db.MatchProductCategories.Select(mvt => mvt.ProductCategoryId == null ? Guid.Empty : (Guid)mvt.ProductCategoryId).Distinct().ToList();
                List<Guid> allProducts = db.Products.Select(p => p.CategoryId).Distinct().ToList();
                List<Guid> allGuids = allMatchProductCategories.Union(allProducts).Distinct().ToList();
                return db.ProductCategories.Where(mvt => !allGuids.Contains(mvt.Id)).Select(vt => vt.Id).ToList();
            }
        }

        public static List<ProductCategory> GetUnusedProductCategories()
        {
            using (MarketDbContext db = new MarketDbContext())
            {
                List<Guid> allMatchProductCategories = db.MatchProductCategories.Select(mvt => mvt.ProductCategoryId == null ? Guid.Empty : (Guid)mvt.ProductCategoryId).Distinct().ToList();
                List<Guid> allProducts = db.Products.Select(p => p.CategoryId).Distinct().ToList();
                List<Guid> allGuids = allMatchProductCategories.Union(allProducts).Distinct().ToList();
                return db.ProductCategories.Where(mvt => !allGuids.Contains(mvt.Id)).ToList();
            }
        }

        public static List<Guid> GetUnusedMidCategoriesIds()
        {
            using (MarketDbContext db = new MarketDbContext())
            {
                IEnumerable<Guid> allProductCategories = db.ProductCategories.Select(mo => mo.MidCategoryId == null ? Guid.Empty : (Guid) mo.MidCategoryId).Distinct();
                return db.MidCategories.Where(mc => !allProductCategories.Contains(mc.Id)).Select(mqu => mqu.Id).ToList();
            }
        }
        public static List<MidCategory> GetUnusedMidCategories()
        {
            using (MarketDbContext db = new MarketDbContext())
            {
                IEnumerable<Guid> allProductCategories = db.ProductCategories.Select(mo => mo.MidCategoryId == null ? Guid.Empty : (Guid)mo.MidCategoryId).Distinct();
                return db.MidCategories.Where(mc => !allProductCategories.Contains(mc.Id)).ToList();
            }
        }

        public static List<Guid> GetUnusedTopCategoriesIds()
        {
            using (MarketDbContext db = new MarketDbContext())
            {
                IEnumerable<Guid> allMidCategories = db.MidCategories.Select(mo => mo.TopCategoryId == null ? Guid.Empty : (Guid)mo.TopCategoryId).Distinct();
                return db.TopCategories.Where(mc => !allMidCategories.Contains(mc.Id)).Select(mqu => mqu.Id).ToList();
            }
        }
        public static List<TopCategory> GetUnusedTopCategories()
        {
            using (MarketDbContext db = new MarketDbContext())
            {
                IEnumerable<Guid> allMidCategories = db.MidCategories.Select(mo => mo.TopCategoryId == null ? Guid.Empty : (Guid)mo.TopCategoryId).Distinct();
                return db.TopCategories.Where(mc => !allMidCategories.Contains(mc.Id)).ToList();
            }
        }

        public static List<Product> GetUnusedProducts()
        {
            using (MarketDbContext db = new MarketDbContext())
            {
                IEnumerable<Guid> allOffers = db.Offers.Select(o => o.ProductId).Distinct();
                return db.Products.Where(p => !allOffers.Contains(p.Id)).ToList();
            }
        }
    }
}
