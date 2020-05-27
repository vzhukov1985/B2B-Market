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

            modelBuilder.Entity<Contract>().HasOne(c => c.Client).WithMany(cc => cc.Contracts).HasForeignKey(c => c.ClientId);
            modelBuilder.Entity<Contract>().HasOne(s => s.Supplier).WithMany(sc => sc.Contracts).HasForeignKey(s => s.SupplierId);
        }

        public virtual DbSet<Supplier> Suppliers { get; set; }
        public virtual DbSet<Client> Clients { get; set; }
        public virtual DbSet<Contract> Contracts { get; set; }
    }
}
