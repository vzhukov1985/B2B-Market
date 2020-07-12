using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Core.Models;
using Core;
using System.Configuration;
using System.Linq;
using System.Collections.ObjectModel;

namespace Core.Models
{
    public class MarketDbContext: DbContext
    {
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlServer(Settings.Default.DbConnectionString);
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Contract>().HasKey(k => new { k.ClientId, k.SupplierId });
            modelBuilder.Entity<Favorite>().HasKey(f => new { f.ClientUserId, f.ProductId });
            modelBuilder.Entity<ProductExtraProperty>().HasKey(k => new { k.ProductId, k.PropertyTypeId });
            modelBuilder.Entity<CurrentOrder>().HasKey(k => new { k.ClientId, k.OfferId});
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
    }
}
