using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Core.Models;
using Core;
using System.Configuration;

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
            modelBuilder.Entity<Offer>().HasKey(k => new { k.ProductId, k.SupplierId, k.QuantityUnitId });

            modelBuilder.Entity<Contract>().HasOne(c => c.Client).WithMany(cc => cc.Contracts).HasForeignKey(c => c.ClientId);
            modelBuilder.Entity<Contract>().HasOne(s => s.Supplier).WithMany(sc => sc.Contracts).HasForeignKey(s => s.SupplierId);
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
    }
}
