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
using Core.Services;
using Core.Models;
using System.IO;
using System.Threading;
using System.Diagnostics;

namespace Core.DBModels
{
    public class MarketDbContext : DbContext
    {
        public MarketDbContext()
        {
            //  Database.EnsureCreated();
        }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseMySql(CoreSettings.DbConnectionString);
                //optionsBuilder.EnableSensitiveDataLogging(true); //TODO: Delete when deploy
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ArchivedOrder>(entity =>
            {
                entity.ToTable("archivedorders");

                entity.HasKey(e => e.Id)
                    .HasName("PRIMARY");

                entity.HasIndex(e => e.ArchivedRequestId)
                    .HasName("FK_ArchivedOrders_To_ArchivedRequests");



                entity.Property(e => e.Id)
                    .IsRequired()
                    .HasColumnType("char(36)")
                    .HasCharSet("utf8mb4")
                    .HasCollation("utf8mb4_0900_ai_ci");

                entity.Property(e => e.ArchivedRequestId)
                    .IsRequired()
                    .HasColumnType("char(36)")
                    .HasCharSet("utf8mb4")
                    .HasCollation("utf8mb4_0900_ai_ci");

                entity.Property(e => e.SupplierProductCode)
                    .IsRequired()
                    .HasColumnType("varchar(30)")
                    .HasCharSet("utf8mb4")
                    .HasCollation("utf8mb4_0900_ai_ci");

                entity.Property(e => e.ProductName)
                    .IsRequired()
                    .HasColumnType("varchar(100)")
                    .HasCharSet("utf8mb4")
                    .HasCollation("utf8mb4_0900_ai_ci");

                entity.Property(e => e.ProductCategory)
                    .IsRequired()
                    .HasColumnType("varchar(50)")
                    .HasCharSet("utf8mb4")
                    .HasCollation("utf8mb4_0900_ai_ci");

                entity.Property(e => e.ProductCode)
                    .IsRequired()
                    .HasColumnType("int");

                entity.Property(e => e.ProductVolumeType)
                    .IsRequired()
                    .HasColumnType("varchar(30)")
                    .HasCharSet("utf8mb4")
                    .HasCollation("utf8mb4_0900_ai_ci");

                entity.Property(e => e.ProductVolumeUnit)
                    .IsRequired()
                    .HasColumnType("varchar(30)")
                    .HasCharSet("utf8mb4")
                    .HasCollation("utf8mb4_0900_ai_ci");

                entity.Property(e => e.ProductVolume)
                    .IsRequired()
                    .HasColumnType("decimal(8,3)");

                entity.Property(e => e.QuantityUnit)
                    .IsRequired()
                    .HasColumnType("varchar(30)")
                    .HasCharSet("utf8mb4")
                    .HasCollation("utf8mb4_0900_ai_ci");

                entity.Property(e => e.Quantity)
                    .IsRequired()
                    .HasColumnType("decimal(10,3)");

                entity.Property(e => e.Price)
                    .IsRequired()
                    .HasColumnType("decimal(10,2)");



                entity.HasOne(d => d.ArchivedRequest)
                    .WithMany(p => p.ArchivedOrders)
                    .HasForeignKey(d => d.ArchivedRequestId)
                    .OnDelete(DeleteBehavior.Restrict)
                    .HasConstraintName("FK_ArchivedOrders_To_ArchivedRequests");
            });

            modelBuilder.Entity<ArchivedRequest>(entity =>
            {
                entity.ToTable("archivedrequests");

                entity.HasKey(e => e.Id)
                    .HasName("PRIMARY");

                entity.HasIndex(e => e.ArchivedSupplierId)
                    .HasName("FK_ArchivedRequests_To_ArchivedSuppliers");

                entity.HasIndex(e => e.ClientId)
                    .HasName("FK_ArchivedRequests_To_Clients");


                
                entity.Property(e => e.Id)
                    .IsRequired()
                    .HasColumnType("char(36)")
                    .HasCharSet("utf8mb4")
                    .HasCollation("utf8mb4_0900_ai_ci");

                entity.Property(e => e.Code)
                    .IsRequired()
                    .HasColumnType("int");

                entity.Property(e => e.ClientId)
                    .IsRequired(false)
                    .HasColumnType("char(36)")
                    .HasCharSet("utf8mb4")
                    .HasCollation("utf8mb4_0900_ai_ci");

                entity.Property(e => e.SenderName)
                    .IsRequired(false)
                    .HasColumnType("varchar(50)")
                    .HasCharSet("utf8mb4")
                    .HasCollation("utf8mb4_0900_ai_ci");

                entity.Property(e => e.SenderSurname)
                    .IsRequired(false)
                    .HasColumnType("varchar(50)")
                    .HasCharSet("utf8mb4")
                    .HasCollation("utf8mb4_0900_ai_ci");

                entity.Property(e => e.ItemsQuantity)
                    .IsRequired()
                    .HasColumnType("int");

                entity.Property(e => e.ProductsQuantity)
                    .IsRequired()
                    .HasColumnType("int");

                entity.Property(e => e.ArchivedSupplierId)
                    .IsRequired()
                    .HasColumnType("char(36)")
                    .HasCharSet("utf8mb4")
                    .HasCollation("utf8mb4_0900_ai_ci");

                entity.Property(e => e.TotalPrice)
                    .IsRequired()
                    .HasColumnType("decimal(10,2)");

                entity.Property(e => e.DateTimeSent)
                    .IsRequired()
                    .HasColumnType("timestamp");

                entity.Property(e => e.DeliveryDateTime)
                    .IsRequired()
                    .HasColumnType("timestamp");

                entity.Property(e => e.Comments)
                    .IsRequired(false)
                    .HasColumnType("varchar(300)")
                    .HasCharSet("utf8mb4")
                    .HasCollation("utf8mb4_0900_ai_ci");



                entity.HasOne(d => d.ArchivedSupplier)
                    .WithMany()
                    .HasForeignKey(d => d.ArchivedSupplierId)
                    .OnDelete(DeleteBehavior.Restrict)
                    .HasConstraintName("FK_ArchivedRequests_To_ArchivedSuppliers");

                entity.HasOne(d => d.Client)
                    .WithMany(p => p.ArchivedRequests)
                    .HasForeignKey(d => d.ClientId)
                    .OnDelete(DeleteBehavior.SetNull)
                    .HasConstraintName("FK_ArchivedRequests_To_Clients");
            });

            modelBuilder.Entity<ArchivedRequestsStatus>(entity =>
            {
                entity.ToTable("archivedrequestsstatuses");

                entity.HasKey(e => new { e.ArchivedRequestId, e.ArchivedRequestStatusTypeId })
                    .HasName("PRIMARY");

                entity.HasIndex(e => e.ArchivedRequestStatusTypeId)
                    .HasName("FK_ArchivedRequestsStatuses_To_ArchivedRequestStatusType");



                entity.Property(e => e.ArchivedRequestId)
                    .IsRequired()
                    .HasColumnType("char(36)")
                    .HasCharSet("utf8mb4")
                    .HasCollation("utf8mb4_0900_ai_ci");

                entity.Property(e => e.ArchivedRequestStatusTypeId)
                    .HasColumnType("char(36)")
                    .HasCharSet("utf8mb4")
                    .HasCollation("utf8mb4_0900_ai_ci");

                entity.Property(e => e.DateTime)
                    .HasColumnType("timestamp");



                entity.HasOne(d => d.ArchivedRequest)
                    .WithMany(p => p.ArchivedRequestsStatuses)
                    .HasForeignKey(d => d.ArchivedRequestId)
                    .OnDelete(DeleteBehavior.Restrict)
                    .HasConstraintName("FK_ArchivedRequestsStatuses_To_ArchivedRequests");

                entity.HasOne(d => d.ArchivedRequestStatusType)
                    .WithMany()
                    .HasForeignKey(d => d.ArchivedRequestStatusTypeId)
                    .OnDelete(DeleteBehavior.Restrict)
                    .HasConstraintName("FK_ArchivedRequestsStatuses_To_ArchivedRequestStatusType");
            });

            modelBuilder.Entity<ArchivedRequestStatusType>(entity =>
            {
                entity.ToTable("archivedrequeststatustypes");

                entity.HasKey(e => e.Id)
                    .HasName("PRIMARY");



                entity.Property(e => e.Id)
                    .IsRequired()
                    .HasColumnType("char(36)")
                    .HasCharSet("utf8mb4")
                    .HasCollation("utf8mb4_0900_ai_ci");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasColumnType("varchar(50)")
                    .HasCharSet("utf8mb4")
                    .HasCollation("utf8mb4_0900_ai_ci");

                entity.Property(e => e.Description)
                    .IsRequired()
                    .HasColumnType("varchar(50)")
                    .HasCharSet("utf8mb4")
                    .HasCollation("utf8mb4_0900_ai_ci");
            });

            modelBuilder.Entity<ArchivedSupplier>(entity =>
            {
                entity.ToTable("archivedsuppliers");

                entity.HasKey(e => e.Id)
                    .HasName("PRIMARY");



                entity.Property(e => e.Id)
                    .IsRequired()
                    .HasColumnType("char(36)")
                    .HasCharSet("utf8mb4")
                    .HasCollation("utf8mb4_0900_ai_ci");

                entity.Property(e => e.ShortName)
                    .IsRequired()
                    .HasColumnType("varchar(100)")
                    .HasCharSet("utf8mb4")
                    .HasCollation("utf8mb4_0900_ai_ci");

                entity.Property(e => e.FullName)
                    .IsRequired()
                    .HasColumnType("varchar(200)")
                    .HasCharSet("utf8mb4")
                    .HasCollation("utf8mb4_0900_ai_ci");

                entity.Property(e => e.Bin)
                    .IsRequired()
                    .HasColumnName("BIN")
                    .HasColumnType("char(12)")
                    .HasCharSet("utf8mb4")
                    .HasCollation("utf8mb4_0900_ai_ci");

                entity.Property(e => e.Country)
                    .HasColumnType("varchar(50)")
                    .HasCharSet("utf8mb4")
                    .HasCollation("utf8mb4_0900_ai_ci");

                entity.Property(e => e.City)
                    .HasColumnType("varchar(50)")
                    .HasCharSet("utf8mb4")
                    .HasCollation("utf8mb4_0900_ai_ci");

                entity.Property(e => e.Address)
                    .HasColumnType("varchar(350)")
                    .HasCharSet("utf8mb4")
                    .HasCollation("utf8mb4_0900_ai_ci");

                entity.Property(e => e.Phone)
                    .HasColumnType("varchar(30)")
                    .HasCharSet("utf8mb4")
                    .HasCollation("utf8mb4_0900_ai_ci");

                entity.Property(e => e.Email)
                    .HasColumnType("varchar(320)")
                    .HasCharSet("utf8mb4")
                    .HasCollation("utf8mb4_0900_ai_ci");
            });

            modelBuilder.Entity<Client>(entity =>
            {
                entity.ToTable("clients");

                entity.HasKey(e => e.Id)
                    .HasName("PRIMARY");



                entity.Property(e => e.Id)
                    .IsRequired()
                    .HasColumnType("char(36)")
                    .HasCharSet("utf8mb4")
                    .HasCollation("utf8mb4_0900_ai_ci");

                entity.Property(e => e.ShortName)
                    .IsRequired()
                    .HasColumnType("varchar(100)")
                    .HasDefaultValueSql("'Новый Клиент'")
                    .HasCharSet("utf8mb4")
                    .HasCollation("utf8mb4_0900_ai_ci");

                entity.Property(e => e.FullName)
                    .IsRequired()
                    .HasColumnType("varchar(200)")
                    .HasDefaultValueSql("'Новый Клиент'")
                    .HasCharSet("utf8mb4")
                    .HasCollation("utf8mb4_0900_ai_ci");

                entity.Property(e => e.Bin)
                    .IsRequired()
                    .HasColumnName("BIN")
                    .HasColumnType("char(12)")
                    .HasCharSet("utf8mb4")
                    .HasCollation("utf8mb4_0900_ai_ci");

                entity.Property(e => e.Country)
                    .HasColumnType("varchar(50)")
                    .HasCharSet("utf8mb4")
                    .HasCollation("utf8mb4_0900_ai_ci");

                entity.Property(e => e.City)
                    .HasColumnType("varchar(50)")
                    .HasCharSet("utf8mb4")
                    .HasCollation("utf8mb4_0900_ai_ci");
                
                entity.Property(e => e.Address)
                    .HasColumnType("varchar(200)")
                    .HasCharSet("utf8mb4")
                    .HasCollation("utf8mb4_0900_ai_ci");

                entity.Property(e => e.Phone)
                    .HasColumnType("varchar(30)")
                    .HasCharSet("utf8mb4")
                    .HasCollation("utf8mb4_0900_ai_ci");

                entity.Property(e => e.Email)
                    .HasColumnType("varchar(320)")
                    .HasCharSet("utf8mb4")
                    .HasCollation("utf8mb4_0900_ai_ci");

                entity.Property(e => e.ContactPersonName)
                    .HasColumnType("varchar(100)")
                    .HasCharSet("utf8mb4")
                    .HasCollation("utf8mb4_0900_ai_ci");

                entity.Property(e => e.ContactPersonPhone)
                    .HasColumnType("varchar(30)")
                    .HasCharSet("utf8mb4")
                    .HasCollation("utf8mb4_0900_ai_ci");
            });

            modelBuilder.Entity<ClientUser>(entity =>
            {
                entity.ToTable("clientsusers");

                entity.HasKey(e => e.Id)
                    .HasName("PRIMARY");

                entity.HasIndex(e => e.ClientId)
                    .HasName("FK_ClientUsers_To_Client");

                entity.HasIndex(e => e.Login)
                    .HasName("UQ_ClientUsers_Login")
                    .IsUnique();



                entity.Property(e => e.Id)
                    .IsRequired()
                    .HasColumnType("char(36)")
                    .HasCharSet("utf8mb4")
                    .HasCollation("utf8mb4_0900_ai_ci");

                entity.Property(e => e.ClientId)
                    .IsRequired()
                    .HasColumnType("char(36)")
                    .HasCharSet("utf8mb4")
                    .HasCollation("utf8mb4_0900_ai_ci");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasColumnType("varchar(50)")
                    .HasCharSet("utf8mb4")
                    .HasCollation("utf8mb4_0900_ai_ci");

                entity.Property(e => e.Surname)
                    .HasColumnType("varchar(50)")
                    .HasCharSet("utf8mb4")
                    .HasCollation("utf8mb4_0900_ai_ci");

                entity.Property(e => e.Login)
                    .IsRequired()
                    .HasColumnType("varchar(20)")
                    .HasCharSet("utf8mb4")
                    .HasCollation("utf8mb4_0900_ai_ci");

                entity.Property(e => e.PasswordHash)
                    .IsRequired()
                    .HasColumnType("char(60)")
                    .HasCharSet("utf8mb4")
                    .HasCollation("utf8mb4_0900_ai_ci");

                entity.Property(e => e.IsAdmin)
                    .IsRequired()
                    .HasColumnType("tinyint(1)");

                entity.Property(e => e.InitialPassword)
                    .IsRequired()
                    .HasColumnType("varchar(16)")
                    .HasCharSet("utf8mb4")
                    .HasCollation("utf8mb4_0900_ai_ci");

                entity.Property(e => e.PinHash)
                    .HasColumnName("PinHash")
                    .HasColumnType("char(60)")
                    .HasCharSet("utf8mb4")
                    .HasCollation("utf8mb4_0900_ai_ci");



                entity.HasOne(d => d.Client)
                    .WithMany(p => p.Users)
                    .HasForeignKey(d => d.ClientId)
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("FK_ClientUsers_To_Client");
            });

            modelBuilder.Entity<ConflictedDescription>(entity =>
            {
                entity.ToTable("conflicteddescriptions");

                entity.HasKey(e => e.Id)
                    .HasName("PRIMARY");

                entity.HasIndex(e => e.ProductId)
                    .HasName("FK_ConflictedDescriptions_To_Product");

                entity.HasIndex(e => e.SupplierId)
                    .HasName("FK_ConflictedDescriptions_To_Supplier");



                entity.Property(e => e.Id)
                    .IsRequired()
                    .HasColumnType("char(36)")
                    .HasCharSet("utf8mb4")
                    .HasCollation("utf8mb4_0900_ai_ci");

                entity.Property(e => e.SupplierId)
                    .IsRequired()
                    .HasColumnType("char(36)")
                    .HasCharSet("utf8mb4")
                    .HasCollation("utf8mb4_0900_ai_ci");

                entity.Property(e => e.ProductId)
                    .IsRequired()
                    .HasColumnType("char(36)")
                    .HasCharSet("utf8mb4")
                    .HasCollation("utf8mb4_0900_ai_ci");

                entity.Property(e => e.Description)
                    .HasColumnType("longtext")
                    .HasCharSet("utf8mb4")
                    .HasCollation("utf8mb4_0900_ai_ci");



                entity.HasOne(d => d.Product)
                    .WithMany()
                    .HasForeignKey(d => d.ProductId)
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("FK_ConflictedDescriptions_To_Product");

                entity.HasOne(d => d.Supplier)
                    .WithMany()
                    .HasForeignKey(d => d.SupplierId)
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("FK_ConflictedDescriptions_To_Supplier");
            });

            modelBuilder.Entity<ConflictedPic>(entity =>
            {
                entity.ToTable("conflictedpics");

                entity.HasKey(e => e.Id)
                    .HasName("PRIMARY");

                entity.HasIndex(e => e.SupplierId)
                    .HasName("FK_ConflictedPics_To_Supplier");

                entity.HasIndex(e => e.ProductId)
                    .HasName("FK_ConflictedPics_To_Product");



                entity.Property(e => e.Id)
                    .IsRequired()
                    .HasColumnType("char(36)")
                    .HasCharSet("utf8mb4")
                    .HasCollation("utf8mb4_0900_ai_ci");

                entity.Property(e => e.ProductId)
                    .IsRequired()
                    .HasColumnType("char(36)")
                    .HasCharSet("utf8mb4")
                    .HasCollation("utf8mb4_0900_ai_ci");

                entity.Property(e => e.SupplierId)
                    .IsRequired()
                    .HasColumnType("char(36)")
                    .HasCharSet("utf8mb4")
                    .HasCollation("utf8mb4_0900_ai_ci");



                entity.HasOne(d => d.Supplier)
                    .WithMany()
                    .HasForeignKey(d => d.SupplierId)
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("FK_ConflictedPics_To_Supplier");

                entity.HasOne(d => d.Product)
                    .WithMany()
                    .HasForeignKey(d => d.ProductId)
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("FK_ConflictedPics_To_Product");
            });

            modelBuilder.Entity<Contract>(entity =>
            {
                entity.ToTable("contracts");

                entity.HasKey(e => new { e.ClientId, e.SupplierId })
                    .HasName("PRIMARY");

                entity.HasIndex(e => e.SupplierId)
                    .HasName("FK_Contracts_To_Suppliers");



                entity.Property(e => e.ClientId)
                    .IsRequired()
                    .HasColumnType("char(36)")
                    .HasCharSet("utf8mb4")
                    .HasCollation("utf8mb4_0900_ai_ci");

                entity.Property(e => e.SupplierId)
                    .IsRequired()
                    .HasColumnType("char(36)")
                    .HasCharSet("utf8mb4")
                    .HasCollation("utf8mb4_0900_ai_ci");



                entity.HasOne(d => d.Client)
                    .WithMany(p => p.Contracts)
                    .HasForeignKey(d => d.ClientId)
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("FK_Contracts_To_Clients");

                entity.HasOne(d => d.Supplier)
                    .WithMany(p => p.Contracts)
                    .HasForeignKey(d => d.SupplierId)
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("FK_Contracts_To_Suppliers");
            });

            modelBuilder.Entity<CurrentOrder>(entity =>
            {
                entity.ToTable("currentorders");

                entity.HasKey(e => new { e.ClientId, e.OfferId })
                    .HasName("PRIMARY");

                entity.HasIndex(e => e.OfferId)
                    .HasName("FK_CurrentOrders_To_Offers");



                entity.Property(e => e.ClientId)
                    .IsRequired()
                    .HasColumnType("char(36)")
                    .HasCharSet("utf8mb4")
                    .HasCollation("utf8mb4_0900_ai_ci");

                entity.Property(e => e.OfferId)
                    .IsRequired()
                    .HasColumnType("char(36)")
                    .HasCharSet("utf8mb4")
                    .HasCollation("utf8mb4_0900_ai_ci");

                entity.Property(e => e.Quantity)
                    .IsRequired()
                    .HasColumnType("decimal(10,3)");



                entity.HasOne(d => d.Client)
                    .WithMany(p => p.CurrentOrders)
                    .HasForeignKey(d => d.ClientId)
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("FK_CurrentOrders_To_Clients");

                entity.HasOne(d => d.Offer)
                    .WithMany()
                    .HasForeignKey(d => d.OfferId)
                    .OnDelete(DeleteBehavior.Restrict)
                    .HasConstraintName("FK_CurrentOrders_To_Offers");
            });

            modelBuilder.Entity<Favorite>(entity =>
            {
                entity.HasKey(e => new { e.ClientUserId, e.ProductId })
                    .HasName("PRIMARY");

                entity.ToTable("favorites");

                entity.HasIndex(e => e.ClientUserId)
                    .HasName("FK_Favorites_To_ClientUsers");

                entity.HasIndex(e => e.ProductId)
                    .HasName("FK_Favorites_To_Products");



                entity.Property(e => e.ClientUserId)
                    .IsRequired()
                    .HasColumnType("char(36)")
                    .HasCharSet("utf8mb4")
                    .HasCollation("utf8mb4_0900_ai_ci");

                entity.Property(e => e.ProductId)
                    .IsRequired()
                    .HasColumnType("char(36)")
                    .HasCharSet("utf8mb4")
                    .HasCollation("utf8mb4_0900_ai_ci");



                entity.HasOne(d => d.ClientUser)
                    .WithMany(p => p.Favorites)
                    .HasForeignKey(d => d.ClientUserId)
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("FK_Favorites_To_ClientsUsers");

                entity.HasOne(d => d.Product)
                    .WithMany(p => p.Favorites)
                    .HasForeignKey(d => d.ProductId)
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("FK_Favorites_To_Products");
            });

            modelBuilder.Entity<MatchOffer>(entity =>
            {
                entity.ToTable("matchoffers");

                entity.HasKey(e => e.Id)
                    .HasName("PRIMARY");

                entity.HasIndex(e => e.MatchProductCategoryId)
                    .HasName("FK_MatchOffers_To_MatchProductCategories");

                entity.HasIndex(e => e.MatchQuantityUnitId)
                    .HasName("FK_MatchOffers_To_MatchQuantityUnits");

                entity.HasIndex(e => e.MatchVolumeTypeId)
                    .HasName("FK_MatchOffers_To_MatchVolumeTypes");

                entity.HasIndex(e => e.MatchVolumeUnitId)
                    .HasName("FK_MatchOffers_To_MatchVolumeUnits");

                entity.HasIndex(e => e.OfferId)
                    .HasName("FK_MatchOffers_To_Offers");

                entity.HasIndex(e => e.SupplierId)
                    .HasName("FK_MatchOffers_To_Suppliers");



                entity.Property(e => e.Id)
                    .IsRequired()
                    .HasColumnType("char(36)")
                    .HasCharSet("utf8mb4")
                    .HasCollation("utf8mb4_0900_ai_ci");

                entity.Property(e => e.SupplierId)
                    .IsRequired()
                    .HasColumnType("char(36)")
                    .HasCharSet("utf8mb4")
                    .HasCollation("utf8mb4_0900_ai_ci");

                entity.Property(e => e.SupplierProductCode)
                    .IsRequired()
                    .HasColumnType("varchar(30)")
                    .HasCharSet("utf8mb4")
                    .HasCollation("utf8mb4_0900_ai_ci");

                entity.Property(e => e.ProductName)
                    .IsRequired()
                    .HasColumnType("varchar(1000)")
                    .HasCharSet("utf8mb4")
                    .HasCollation("utf8mb4_0900_ai_ci");

                entity.Property(e => e.MatchProductCategoryId)
                    .IsRequired()
                    .HasColumnType("char(36)")
                    .HasCharSet("utf8mb4")
                    .HasCollation("utf8mb4_0900_ai_ci");

                entity.Property(e => e.MatchVolumeTypeId)
                    .IsRequired(false)
                    .HasColumnType("char(36)")
                    .HasCharSet("utf8mb4")
                    .HasCollation("utf8mb4_0900_ai_ci");

                entity.Property(e => e.MatchVolumeUnitId)
                    .IsRequired(false)
                    .HasColumnType("char(36)")
                    .HasCharSet("utf8mb4")
                    .HasCollation("utf8mb4_0900_ai_ci");

                entity.Property(e => e.ProductVolume)
                    .IsRequired()
                    .HasColumnType("decimal(8,3)");

                entity.Property(e => e.MatchQuantityUnitId)
                    .IsRequired()
                    .HasColumnType("char(36)")
                    .HasCharSet("utf8mb4")
                    .HasCollation("utf8mb4_0900_ai_ci");

                entity.Property(e => e.Remains)
                    .IsRequired()
                    .HasColumnType("decimal(10,3)");

                entity.Property(e => e.RetailPrice)
                    .IsRequired()
                    .HasColumnType("decimal(10,2)");

                entity.Property(e => e.DiscountPrice)
                    .HasColumnType("decimal(10,2)");

                entity.Property(e => e.OfferId)
                    .IsRequired(false)
                    .HasColumnType("char(36)")
                    .HasCharSet("utf8mb4")
                    .HasCollation("utf8mb4_0900_ai_ci");



                entity.HasOne(d => d.MatchProductCategory)
                    .WithMany()
                    .HasForeignKey(d => d.MatchProductCategoryId)
                    .OnDelete(DeleteBehavior.Restrict)
                    .HasConstraintName("FK_MatchOffers_To_MatchProductCategories");

                entity.HasOne(d => d.MatchQuantityUnit)
                    .WithMany()
                    .HasForeignKey(d => d.MatchQuantityUnitId)
                    .OnDelete(DeleteBehavior.Restrict)
                    .HasConstraintName("FK_MatchOffers_To_MatchQuantityUnits");

                entity.HasOne(d => d.MatchVolumeType)
                    .WithMany()
                    .HasForeignKey(d => d.MatchVolumeTypeId)
                    .OnDelete(DeleteBehavior.Restrict)
                    .HasConstraintName("FK_MatchOffers_To_MatchVolumeTypes");

                entity.HasOne(d => d.MatchVolumeUnit)
                    .WithMany()
                    .HasForeignKey(d => d.MatchVolumeUnitId)
                    .OnDelete(DeleteBehavior.Restrict)
                    .HasConstraintName("FK_MatchOffers_To_MatchVolumeUnits");

                entity.HasOne(d => d.Offer)
                    .WithMany()
                    .HasForeignKey(d => d.OfferId)
                    .OnDelete(DeleteBehavior.SetNull)
                    .HasConstraintName("FK_MatchOffers_To_Offers");

                entity.HasOne(d => d.Supplier)
                    .WithMany()
                    .HasForeignKey(d => d.SupplierId)
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("FK_MatchOffers_To_Suppliers");
            });

            modelBuilder.Entity<MatchProductCategory>(entity =>
            {
                entity.ToTable("matchproductcategories");

                entity.HasKey(e => e.Id)
                    .HasName("PRIMARY");

                entity.HasIndex(e => e.ProductCategoryId)
                    .HasName("FK_MatchProductCategories_To_ProductCategories");

                entity.HasIndex(e => e.SupplierId)
                    .HasName("FK_MatchProductCategories_To_Suppliers");



                entity.Property(e => e.Id)
                    .IsRequired()
                    .HasColumnType("char(36)")
                    .HasCharSet("utf8mb4")
                    .HasCollation("utf8mb4_0900_ai_ci");

                entity.Property(e => e.SupplierId)
                    .IsRequired()
                    .HasColumnType("char(36)")
                    .HasCharSet("utf8mb4")
                    .HasCollation("utf8mb4_0900_ai_ci");

                entity.Property(e => e.SupplierProductCategoryName)
                    .IsRequired()
                    .HasColumnType("varchar(200)")
                    .HasCharSet("utf8mb4")
                    .HasCollation("utf8mb4_0900_ai_ci");

                entity.Property(e => e.ProductCategoryId)
                    .IsRequired(false)
                    .HasColumnType("char(36)")
                    .HasCharSet("utf8mb4")
                    .HasCollation("utf8mb4_0900_ai_ci");



                entity.HasOne(d => d.ProductCategory)
                    .WithMany()
                    .HasForeignKey(d => d.ProductCategoryId)
                    .OnDelete(DeleteBehavior.SetNull)
                    .HasConstraintName("FK_MatchProductCategories_To_ProductCategories");

                entity.HasOne(d => d.Supplier)
                    .WithMany()
                    .HasForeignKey(d => d.SupplierId)
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("FK_MatchProductCategories_To_Suppliers");
            });

            modelBuilder.Entity<MatchProductExtraProperty>(entity =>
            {
                entity.ToTable("matchproductextraproperties");

                entity.HasKey(e => e.Id)
                    .HasName("PRIMARY");

                entity.HasIndex(e => e.MatchOfferId)
                    .HasName("FK_MatchProductExtraProperties_To_MatchOffers");

                entity.HasIndex(e => e.MatchProductExtraPropertyTypeId)
                    .HasName("FK_MatchProductExtraProperties_To_MatchProductsExtraPropertyTypes");



                entity.Property(e => e.Id)
                    .IsRequired()
                    .HasColumnType("char(36)")
                    .HasCharSet("utf8mb4")
                    .HasCollation("utf8mb4_0900_ai_ci");

                entity.Property(e => e.MatchOfferId)
                    .IsRequired()
                    .HasColumnType("char(36)")
                    .HasCharSet("utf8mb4")
                    .HasCollation("utf8mb4_0900_ai_ci");

                entity.Property(e => e.MatchProductExtraPropertyTypeId)
                    .IsRequired()
                    .HasColumnType("char(36)")
                    .HasCharSet("utf8mb4")
                    .HasCollation("utf8mb4_0900_ai_ci");

                entity.Property(e => e.Value)
                    .IsRequired()
                    .HasColumnType("varchar(50)")
                    .HasCharSet("utf8mb4")
                    .HasCollation("utf8mb4_0900_ai_ci");



                entity.HasOne(d => d.MatchOffer)
                    .WithMany(p => p.MatchProductExtraProperties)
                    .HasForeignKey(d => d.MatchOfferId)
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("FK_MatchProductExtraProperties_To_MatchOffers");

                entity.HasOne(d => d.MatchProductExtraPropertyType)
                    .WithMany()
                    .HasForeignKey(d => d.MatchProductExtraPropertyTypeId)
                    .OnDelete(DeleteBehavior.Restrict)
                    .HasConstraintName("FK_MatchProductExtraProperties_To_MatchProductsExtraPropertyTypes");
            });

            modelBuilder.Entity<MatchProductExtraPropertyType>(entity =>
            {
                entity.ToTable("matchproductextrapropertytypes");

                entity.HasKey(e => e.Id)
                    .HasName("PRIMARY");

                entity.HasIndex(e => e.ProductExtraPropertyTypeId)
                    .HasName("FK_MatchExtraPropertyTypes_To_ExtraPropertyTypes");

                entity.HasIndex(e => e.SupplierId)
                    .HasName("FK_MatchExtraPropertyTypes_To_Suppliers");



                entity.Property(e => e.Id)
                    .IsRequired()
                    .HasColumnType("char(36)")
                    .HasCharSet("utf8mb4")
                    .HasCollation("utf8mb4_0900_ai_ci");

                entity.Property(e => e.SupplierId)
                    .IsRequired()
                    .HasColumnType("char(36)")
                    .HasCharSet("utf8mb4")
                    .HasCollation("utf8mb4_0900_ai_ci");

                entity.Property(e => e.SupplierProductExtraPropertyTypeName)
                    .IsRequired()
                    .HasColumnType("varchar(50)")
                    .HasCharSet("utf8mb4")
                    .HasCollation("utf8mb4_0900_ai_ci");

                entity.Property(e => e.ProductExtraPropertyTypeId)
                    .IsRequired(false)
                    .HasColumnType("char(36)")
                    .HasCharSet("utf8mb4")
                    .HasCollation("utf8mb4_0900_ai_ci");



                entity.HasOne(d => d.ProductExtraPropertyType)
                    .WithMany()
                    .HasForeignKey(d => d.ProductExtraPropertyTypeId)
                    .OnDelete(DeleteBehavior.SetNull)
                    .HasConstraintName("FK_MatchExtraPropertyTypes_To_ExtraPropertyTypes");

                entity.HasOne(d => d.Supplier)
                    .WithMany()
                    .HasForeignKey(d => d.SupplierId)
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("FK_MatchExtraPropertyTypes_To_Suppliers");
            });

            modelBuilder.Entity<MatchQuantityUnit>(entity =>
            {
                entity.ToTable("matchquantityunits");

                entity.HasKey(e => e.Id)
                    .HasName("PRIMARY");

                entity.HasIndex(e => e.QuantityUnitId)
                    .HasName("FK_MatchQuantityUnits_To_QuantityUnits");

                entity.HasIndex(e => e.SupplierId)
                    .HasName("FK_MatchQuantityUnits_To_Suppliers");



                entity.Property(e => e.Id)
                    .IsRequired()
                    .HasColumnType("char(36)")
                    .HasCharSet("utf8mb4")
                    .HasCollation("utf8mb4_0900_ai_ci");

                entity.Property(e => e.SupplierId)
                    .IsRequired()
                    .HasColumnType("char(36)")
                    .HasCharSet("utf8mb4")
                    .HasCollation("utf8mb4_0900_ai_ci");

                entity.Property(e => e.SupplierQUFullName)
                    .IsRequired()
                    .HasColumnName("SupplierQUFullName")
                    .HasColumnType("varchar(50)")
                    .HasCharSet("utf8mb4")
                    .HasCollation("utf8mb4_0900_ai_ci");

                entity.Property(e => e.SupplierQUShortName)
                    .IsRequired()
                    .HasColumnName("SupplierQUShortName")
                    .HasColumnType("varchar(50)")
                    .HasCharSet("utf8mb4")
                    .HasCollation("utf8mb4_0900_ai_ci");

                entity.Property(e => e.QuantityUnitId)
                    .IsRequired(false)
                    .HasColumnType("char(36)")
                    .HasCharSet("utf8mb4")
                    .HasCollation("utf8mb4_0900_ai_ci");



                entity.HasOne(d => d.QuantityUnit)
                    .WithMany()
                    .HasForeignKey(d => d.QuantityUnitId)
                    .OnDelete(DeleteBehavior.SetNull)
                    .HasConstraintName("FK_MatchQuantityUnits_To_QuantityUnits");

                entity.HasOne(d => d.Supplier)
                    .WithMany()
                    .HasForeignKey(d => d.SupplierId)
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("FK_MatchQuantityUnits_To_Suppliers");
            });

            modelBuilder.Entity<MatchVolumeType>(entity =>
            {
                entity.ToTable("matchvolumetypes");

                entity.HasKey(e => e.Id)
                    .HasName("PRIMARY");

                entity.HasIndex(e => e.SupplierId)
                    .HasName("FK_MatchVolumeTypes_To_Suppliers");

                entity.HasIndex(e => e.VolumeTypeId)
                    .HasName("FK_MatchVolumeTypes_To_VolumeTypes");



                entity.Property(e => e.Id)
                    .IsRequired()
                    .HasColumnType("char(36)")
                    .HasCharSet("utf8mb4")
                    .HasCollation("utf8mb4_0900_ai_ci");

                entity.Property(e => e.SupplierId)
                    .IsRequired()
                    .HasColumnType("char(36)")
                    .HasCharSet("utf8mb4")
                    .HasCollation("utf8mb4_0900_ai_ci");

                entity.Property(e => e.SupplierVolumeTypeName)
                    .IsRequired()
                    .HasColumnType("varchar(50)")
                    .HasCharSet("utf8mb4")
                    .HasCollation("utf8mb4_0900_ai_ci");

                entity.Property(e => e.VolumeTypeId)
                    .IsRequired(false)
                    .HasColumnType("char(36)")
                    .HasCharSet("utf8mb4")
                    .HasCollation("utf8mb4_0900_ai_ci");



                entity.HasOne(d => d.Supplier)
                    .WithMany()
                    .HasForeignKey(d => d.SupplierId)
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("FK_MatchVolumeTypes_To_Suppliers");

                entity.HasOne(d => d.VolumeType)
                    .WithMany()
                    .HasForeignKey(d => d.VolumeTypeId)
                    .OnDelete(DeleteBehavior.SetNull)
                    .HasConstraintName("FK_MatchVolumeTypes_To_VolumeTypes");
            });

            modelBuilder.Entity<MatchVolumeUnit>(entity =>
            {
                entity.ToTable("matchvolumeunits");

                entity.HasKey(e => e.Id)
                    .HasName("PRIMARY");

                entity.HasIndex(e => e.SupplierId)
                    .HasName("FK_MatchVolumeUnits_To_Suppliers");

                entity.HasIndex(e => e.VolumeUnitId)
                    .HasName("FK_MatchVolumeUnits_To_VolumeUnits");



                entity.Property(e => e.Id)
                    .IsRequired()
                    .HasColumnType("char(36)")
                    .HasCharSet("utf8mb4")
                    .HasCollation("utf8mb4_0900_ai_ci");

                entity.Property(e => e.SupplierId)
                    .IsRequired()
                    .HasColumnType("Char(36)")
                    .HasCharSet("utf8mb4")
                    .HasCollation("utf8mb4_0900_ai_ci");

                entity.Property(e => e.SupplierVUFullName)
                    .IsRequired()
                    .HasColumnName("SupplierVUFullName")
                    .HasColumnType("varchar(50)")
                    .HasCharSet("utf8mb4")
                    .HasCollation("utf8mb4_0900_ai_ci");

                entity.Property(e => e.SupplierVUShortName)
                    .IsRequired()
                    .HasColumnName("SupplierVUShortName")
                    .HasColumnType("varchar(50)")
                    .HasCharSet("utf8mb4")
                    .HasCollation("utf8mb4_0900_ai_ci");

                entity.Property(e => e.VolumeUnitId)
                    .IsRequired(false)
                    .HasColumnType("char(36)")
                    .HasCharSet("utf8mb4")
                    .HasCollation("utf8mb4_0900_ai_ci");



                entity.HasOne(d => d.Supplier)
                    .WithMany()
                    .HasForeignKey(d => d.SupplierId)
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("FK_MatchVolumeUnits_To_Suppliers");

                entity.HasOne(d => d.VolumeUnit)
                    .WithMany()
                    .HasForeignKey(d => d.VolumeUnitId)
                    .OnDelete(DeleteBehavior.SetNull)
                    .HasConstraintName("FK_MatchVolumeUnits_To_VolumeUnits");
            });

            modelBuilder.Entity<MidCategory>(entity =>
            {
                entity.ToTable("midcategories");

                entity.HasKey(e => e.Id)
                    .HasName("PRIMARY");

                entity.HasIndex(e => e.TopCategoryId)
                    .HasName("FK_MidCategories_To_TopCategories");



                entity.Property(e => e.Id)
                    .IsRequired()
                    .HasColumnType("char(36)")
                    .HasCharSet("utf8mb4")
                    .HasCollation("utf8mb4_0900_ai_ci");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasColumnType("varchar(50)")
                    .HasCharSet("utf8mb4")
                    .HasCollation("utf8mb4_0900_ai_ci");

                entity.Property(e => e.TopCategoryId)
                    .IsRequired()
                    .HasColumnType("char(36)")
                    .HasCharSet("utf8mb4")
                    .HasCollation("utf8mb4_0900_ai_ci");



                entity.HasOne(d => d.TopCategory)
                    .WithMany()
                    .HasForeignKey(d => d.TopCategoryId)
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("FK_MidCategories_To_TopCategories");
            });

            modelBuilder.Entity<Offer>(entity =>
            {
                entity.ToTable("offers");

                entity.HasKey(e => e.Id)
                    .HasName("PRIMARY");

                entity.HasIndex(e => e.ProductId)
                    .HasName("FK_Offers_To_Products");

                entity.HasIndex(e => e.QuantityUnitId)
                    .HasName("FK_Offers_To_QuantityUnits");

                entity.HasIndex(e => e.SupplierId)
                    .HasName("FK_Offers_To_Suppliers");



                entity.Property(e => e.Id)
                    .IsRequired()
                    .HasColumnType("char(36)")
                    .HasCharSet("utf8mb4")
                    .HasCollation("utf8mb4_0900_ai_ci");

                entity.Property(e => e.SupplierId)
                    .IsRequired()
                    .HasColumnType("char(36)")
                    .HasCharSet("utf8mb4")
                    .HasCollation("utf8mb4_0900_ai_ci");

                entity.Property(e => e.SupplierProductCode)
                    .IsRequired()
                    .HasColumnType("varchar(30)")
                    .HasCharSet("utf8mb4")
                    .HasCollation("utf8mb4_0900_ai_ci");

                entity.Property(e => e.ProductId)
                    .IsRequired()
                    .HasColumnType("char(36)")
                    .HasCharSet("utf8mb4")
                    .HasCollation("utf8mb4_0900_ai_ci");

                entity.Property(e => e.QuantityUnitId)
                    .IsRequired()
                    .HasColumnType("char(36)")
                    .HasCharSet("utf8mb4")
                    .HasCollation("utf8mb4_0900_ai_ci");

                entity.Property(e => e.Remains)
                    .IsRequired()
                    .HasColumnType("decimal(10,3)");

                entity.Property(e => e.RetailPrice)
                    .IsRequired()
                    .HasColumnType("decimal(10,2)");

                entity.Property(e => e.DiscountPrice)
                    .IsRequired()
                    .HasColumnType("decimal(10,2)");

                entity.Property(e => e.IsActive)
                    .IsRequired()
                    .HasColumnType("tinyint(1)");



                entity.HasOne(d => d.Product)
                    .WithMany(p => p.Offers)
                    .HasForeignKey(d => d.ProductId)
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("FK_Offers_To_Products");

                entity.HasOne(d => d.QuantityUnit)
                    .WithMany()
                    .HasForeignKey(d => d.QuantityUnitId)
                    .OnDelete(DeleteBehavior.Restrict)
                    .HasConstraintName("FK_Offers_To_QuantityUnits");

                entity.HasOne(d => d.Supplier)
                    .WithMany(p => p.Offers)
                    .HasForeignKey(d => d.SupplierId)
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("FK_Offers_To_Suppliers");
            });

            modelBuilder.Entity<ProductCategory>(entity =>
            {
                entity.ToTable("productcategories");

                entity.HasKey(e => e.Id)
                    .HasName("PRIMARY");

                entity.HasIndex(e => e.MidCategoryId)
                    .HasName("FK_ProductCategories_To_MidCategories");



                entity.Property(e => e.Id)
                    .IsRequired()
                    .HasColumnType("char(36)")
                    .HasCharSet("utf8mb4")
                    .HasCollation("utf8mb4_0900_ai_ci");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasColumnType("varchar(50)")
                    .HasCharSet("utf8mb4")
                    .HasCollation("utf8mb4_0900_ai_ci");

                entity.Property(e => e.MidCategoryId)
                    .IsRequired()
                    .HasColumnType("char(36)")
                    .HasCharSet("utf8mb4")
                    .HasCollation("utf8mb4_0900_ai_ci");



                entity.HasOne(d => d.MidCategory)
                    .WithMany()
                    .HasForeignKey(d => d.MidCategoryId)
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("FK_ProductCategories_To_MidCategories");
            });

            modelBuilder.Entity<ProductDescription>(entity =>
            {
                entity.ToTable("productdescriptions");

                entity.HasKey(e => e.ProductId)
                    .HasName("PRIMARY");



                entity.Property(e => e.ProductId)
                    .IsRequired()
                    .HasColumnType("char(36)")
                    .HasCharSet("utf8mb4")
                    .HasCollation("utf8mb4_0900_ai_ci");

                entity.Property(e => e.Text)
                    .IsRequired(false)
                    .HasColumnType("longtext")
                    .HasCharSet("utf8mb4")
                    .HasCollation("utf8mb4_0900_ai_ci");



                entity.HasOne(d => d.Product)
                    .WithOne(p => p.Description)
                    .HasForeignKey<ProductDescription>(d => d.ProductId)
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("FK_ProductDescriptions_To_Products");
            });

            modelBuilder.Entity<ProductExtraProperty>(entity =>
            {
                entity.ToTable("productextraproperties");

                entity.HasKey(e => e.Id)
                    .HasName("PRIMARY");

                entity.HasIndex(e => e.ProductId)
                    .HasName("FK_ProductExtraProperties_To_Products");

                entity.HasIndex(e => e.PropertyTypeId)
                    .HasName("FK_ProductExtraProperties_To_ProductExtraPropertyTypes");



                entity.Property(e => e.Id)
                    .IsRequired()
                    .HasColumnType("char(36)")
                    .HasCharSet("utf8mb4")
                    .HasCollation("utf8mb4_0900_ai_ci");

                entity.Property(e => e.ProductId)
                    .IsRequired()
                    .HasColumnType("char(36)")
                    .HasCharSet("utf8mb4")
                    .HasCollation("utf8mb4_0900_ai_ci");

                entity.Property(e => e.PropertyTypeId)
                    .IsRequired()
                    .HasColumnType("char(36)")
                    .HasCharSet("utf8mb4")
                    .HasCollation("utf8mb4_0900_ai_ci");

                entity.Property(e => e.Value)
                    .IsRequired()
                    .HasColumnType("varchar(50)")
                    .HasCharSet("utf8mb4")
                    .HasCollation("utf8mb4_0900_ai_ci");



                entity.HasOne(d => d.Product)
                    .WithMany(p => p.ExtraProperties)
                    .HasForeignKey(d => d.ProductId)
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("FK_ProductExtraProperties_To_Products");

                entity.HasOne(d => d.PropertyType)
                    .WithMany()
                    .HasForeignKey(d => d.PropertyTypeId)
                    .OnDelete(DeleteBehavior.Restrict)
                    .HasConstraintName("FK_ProductExtraProperties_To_ProductExtraPropertyTypes");
            });

            modelBuilder.Entity<ProductExtraPropertyType>(entity =>
            {
                entity.ToTable("productextrapropertytypes");

                entity.HasKey(e => e.Id)
                    .HasName("PRIMARY");

                entity.Property(e => e.Id)
                    .IsRequired()
                    .HasColumnType("char(36)")
                    .HasCharSet("utf8mb4")
                    .HasCollation("utf8mb4_0900_ai_ci");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasColumnType("varchar(50)")
                    .HasCharSet("utf8mb4")
                    .HasCollation("utf8mb4_0900_ai_ci");
            });

            modelBuilder.Entity<Product>(entity =>
            {
                entity.ToTable("products");

                entity.HasKey(e => e.Id)
                    .HasName("PRIMARY");

                entity.HasIndex(e => e.CategoryId)
                    .HasName("FK_Products_To_ProductCategories");

                entity.HasIndex(e => e.VolumeTypeId)
                    .HasName("FK_Products_To_VolumeTypes");

                entity.HasIndex(e => e.VolumeUnitId)
                    .HasName("FK_Products_To_VolumeUnits");



                entity.Property(e => e.Id)
                    .IsRequired()
                    .HasColumnType("char(36)")
                    .HasCharSet("utf8mb4")
                    .HasCollation("utf8mb4_0900_ai_ci");

                entity.Property(e => e.CategoryId)
                    .IsRequired()
                    .HasColumnType("char(36)")
                    .HasCharSet("utf8mb4")
                    .HasCollation("utf8mb4_0900_ai_ci");

                entity.Property(e => e.Code)
                    .IsRequired()
                    .HasColumnType("int");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasColumnType("varchar(100)")
                    .HasCharSet("utf8mb4")
                    .HasCollation("utf8mb4_0900_ai_ci");

                entity.Property(e => e.VolumeTypeId)
                    .IsRequired()
                    .HasColumnType("char(36)")
                    .HasCharSet("utf8mb4")
                    .HasCollation("utf8mb4_0900_ai_ci");

                entity.Property(e => e.VolumeUnitId)
                    .IsRequired()
                    .HasColumnType("char(36)")
                    .HasCharSet("utf8mb4")
                    .HasCollation("utf8mb4_0900_ai_ci");

                entity.Property(e => e.Volume)
                    .IsRequired()
                    .HasColumnType("decimal(8,3)");



                entity.HasOne(d => d.Category)
                    .WithMany()
                    .HasForeignKey(d => d.CategoryId)
                    .OnDelete(DeleteBehavior.Restrict)
                    .HasConstraintName("FK_Products_To_ProductCategories");

                entity.HasOne(d => d.VolumeType)
                    .WithMany()
                    .HasForeignKey(d => d.VolumeTypeId)
                    .OnDelete(DeleteBehavior.Restrict)
                    .HasConstraintName("FK_Products_To_VolumeTypes");

                entity.HasOne(d => d.VolumeUnit)
                    .WithMany()
                    .HasForeignKey(d => d.VolumeUnitId)
                    .OnDelete(DeleteBehavior.Restrict)
                    .HasConstraintName("FK_Products_To_VolumeUnits");
            });

            modelBuilder.Entity<QuantityUnit>(entity =>
            {
                entity.ToTable("quantityunits");

                entity.HasKey(e => e.Id)
                    .HasName("PRIMARY");

                
                
                entity.Property(e => e.Id)
                    .IsRequired()
                    .HasColumnType("char(36)")
                    .HasCharSet("utf8mb4")
                    .HasCollation("utf8mb4_0900_ai_ci");

                entity.Property(e => e.FullName)
                    .IsRequired()
                    .HasColumnType("varchar(50)")
                    .HasCharSet("utf8mb4")
                    .HasCollation("utf8mb4_0900_ai_ci");

                entity.Property(e => e.ShortName)
                    .IsRequired()
                    .HasColumnType("varchar(50)")
                    .HasCharSet("utf8mb4")
                    .HasCollation("utf8mb4_0900_ai_ci");
            });

            modelBuilder.Entity<Supplier>(entity =>
            {
                entity.ToTable("suppliers");

                entity.HasKey(e => e.Id)
                    .HasName("PRIMARY");



                entity.Property(e => e.Id)
                    .IsRequired()
                    .HasColumnType("char(36)")
                    .HasCharSet("utf8mb4")
                    .HasCollation("utf8mb4_0900_ai_ci");

                entity.Property(e => e.ShortName)
                    .IsRequired()
                    .HasColumnType("varchar(100)")
                    .HasDefaultValueSql("'Новый Поставщик'")
                    .HasCharSet("utf8mb4")
                    .HasCollation("utf8mb4_0900_ai_ci");

                entity.Property(e => e.FullName)
                    .IsRequired()
                    .HasColumnType("varchar(200)")
                    .HasDefaultValueSql("'Новый Поставщик'")
                    .HasCharSet("utf8mb4")
                    .HasCollation("utf8mb4_0900_ai_ci");

                entity.Property(e => e.Bin)
                    .IsRequired()
                    .HasColumnName("BIN")
                    .HasColumnType("char(12)")
                    .HasDefaultValueSql("'000000000000'")
                    .HasCharSet("utf8mb4")
                    .HasCollation("utf8mb4_0900_ai_ci");

                entity.Property(e => e.Country)
                    .IsRequired(false)
                    .HasColumnType("varchar(50)")
                    .HasCharSet("utf8mb4")
                    .HasCollation("utf8mb4_0900_ai_ci");

                entity.Property(e => e.City)
                    .IsRequired(false)
                    .HasColumnType("varchar(50)")
                    .HasCharSet("utf8mb4")
                    .HasCollation("utf8mb4_0900_ai_ci");

                entity.Property(e => e.Address)
                    .IsRequired(false)
                    .HasColumnType("varchar(200)")
                    .HasCharSet("utf8mb4")
                    .HasCollation("utf8mb4_0900_ai_ci");

                entity.Property(e => e.Phone)
                    .IsRequired(false)
                    .HasColumnType("varchar(30)")
                    .HasCharSet("utf8mb4")
                    .HasCollation("utf8mb4_0900_ai_ci");

                entity.Property(e => e.Email)
                    .IsRequired(false)
                    .HasColumnType("varchar(320)")
                    .HasCharSet("utf8mb4")
                    .HasCollation("utf8mb4_0900_ai_ci");

                entity.Property(e => e.ContactPersonName)
                    .IsRequired(false)
                    .HasColumnType("varchar(100)")
                    .HasCharSet("utf8mb4")
                    .HasCollation("utf8mb4_0900_ai_ci");

                entity.Property(e => e.ContactPersonPhone)
                    .IsRequired(false)
                    .HasColumnType("varchar(30)")
                    .HasCharSet("utf8mb4")
                    .HasCollation("utf8mb4_0900_ai_ci");

                entity.Property(e => e.IsActive)
                    .IsRequired()
                    .HasColumnType("tinyint(1)");

                entity.Property(e => e.FTPUser)
                    .IsRequired()
                    .HasColumnName("FTPUser")
                    .HasColumnType("varchar(45)")
                    .HasCharSet("utf8mb4")
                    .HasCollation("utf8mb4_0900_ai_ci");

                entity.Property(e => e.FTPPassword)
                    .IsRequired()
                    .HasColumnName("FTPPassword")
                    .HasColumnType("varchar(45)")
                    .HasCharSet("utf8mb4")
                    .HasCollation("utf8mb4_0900_ai_ci");
            });

            modelBuilder.Entity<TopCategory>(entity =>
            {
                entity.ToTable("topcategories");

                entity.HasKey(e => e.Id)
                    .HasName("PRIMARY");

                entity.Property(e => e.Id)
                    .IsRequired()
                    .HasColumnType("char(36)")
                    .HasCharSet("utf8mb4")
                    .HasCollation("utf8mb4_0900_ai_ci");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasColumnType("varchar(50)")
                    .HasCharSet("utf8mb4")
                    .HasCollation("utf8mb4_0900_ai_ci");
            });

            modelBuilder.Entity<UnmatchedDescription>(entity =>
            {
                entity.ToTable("unmatcheddescriptions");

                entity.HasKey(e => e.Id)
                    .HasName("PRIMARY");

                entity.HasIndex(e => e.SupplierId)
                    .HasName("FK_UnmatchedDescriptions_To_Supplier");



                entity.Property(e => e.Id)
                    .IsRequired()
                    .HasColumnType("char(36)")
                    .HasCharSet("utf8mb4")
                    .HasCollation("utf8mb4_0900_ai_ci");

                entity.Property(e => e.SupplierId)
                    .IsRequired()
                    .HasColumnType("char(36)")
                    .HasCharSet("utf8mb4")
                    .HasCollation("utf8mb4_0900_ai_ci");

                entity.Property(e => e.SupplierProductCode)
                    .IsRequired()
                    .HasColumnType("varchar(30)")
                    .HasCharSet("utf8mb4")
                    .HasCollation("utf8mb4_0900_ai_ci");

                entity.Property(e => e.Description)
                    .HasColumnType("longtext")
                    .HasCharSet("utf8mb4")
                    .HasCollation("utf8mb4_0900_ai_ci");



                entity.HasOne(d => d.Supplier)
                    .WithMany()
                    .HasForeignKey(d => d.SupplierId)
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("FK_UnmatchedDescriptions_To_Supplier");
            });

            modelBuilder.Entity<UnmatchedPic>(entity =>
            {
                entity.ToTable("unmatchedpics");

                entity.HasKey(e => e.Id)
                    .HasName("PRIMARY");

                entity.HasIndex(e => e.SupplierId)
                    .HasName("FK_UnmatchedPics_To_Supplier");



                entity.Property(e => e.Id)
                    .IsRequired()
                    .HasColumnType("char(36)")
                    .HasCharSet("utf8mb4")
                    .HasCollation("utf8mb4_0900_ai_ci");

                entity.Property(e => e.SupplierId)
                    .IsRequired()
                    .HasColumnType("char(36)")
                    .HasCharSet("utf8mb4")
                    .HasCollation("utf8mb4_0900_ai_ci");

                entity.Property(e => e.SupplierProductCode)
                    .IsRequired()
                    .HasColumnType("varchar(30)")
                    .HasCharSet("utf8mb4")
                    .HasCollation("utf8mb4_0900_ai_ci");

                
                
                entity.HasOne(d => d.Supplier)
                    .WithMany()
                    .HasForeignKey(d => d.SupplierId)
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("FK_UnmatchedPics_To_Supplier");
            });

            modelBuilder.Entity<VolumeType>(entity =>
            {
                entity.ToTable("volumetypes");

                entity.HasKey(e => e.Id)
                    .HasName("PRIMARY");



                entity.Property(e => e.Id)
                    .IsRequired()
                    .HasColumnType("char(36)")
                    .HasCharSet("utf8mb4")
                    .HasCollation("utf8mb4_0900_ai_ci");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasColumnType("varchar(50)")
                    .HasCharSet("utf8mb4")
                    .HasCollation("utf8mb4_0900_ai_ci");
            });

            modelBuilder.Entity<VolumeUnit>(entity =>
            {
                entity.ToTable("volumeunits");

                entity.HasKey(e => e.Id)
                    .HasName("PRIMARY");

                entity.Property(e => e.Id)
                    .IsRequired()
                    .HasColumnType("char(36)")
                    .HasCharSet("utf8mb4")
                    .HasCollation("utf8mb4_0900_ai_ci");

                entity.Property(e => e.ShortName)
                    .IsRequired()
                    .HasColumnType("varchar(50)")
                    .HasCharSet("utf8mb4")
                    .HasCollation("utf8mb4_0900_ai_ci");

                entity.Property(e => e.FullName)
                    .IsRequired()
                    .HasColumnType("varchar(50)")
                    .HasCharSet("utf8mb4")
                    .HasCollation("utf8mb4_0900_ai_ci");
            });
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


        private static readonly object locker = new object();
        public static void AddRemoveProductToFavourites(Product selectedProduct, ClientUser User)
        {
            lock (locker)
            {
                using (MarketDbContext db = new MarketDbContext())
                {
                    if (selectedProduct.IsFavoriteForUser)
                    {
                        db.Favorites.Remove(new Favorite() { ClientUserId = User.Id, ProductId = selectedProduct.Id });
                        User.Favorites.Remove(User.Favorites.Where(f => f.ProductId == selectedProduct.Id && f.ClientUserId == User.Id).FirstOrDefault());
                    }
                    else
                    {
                        db.Favorites.Add(new Favorite
                        {
                            ClientUserId = User.Id,
                            ProductId = selectedProduct.Id
                        });
                        User.Favorites.Add(new Favorite() { ProductId = selectedProduct.Id, ClientUserId = User.Id });
                    }
                    db.SaveChanges();
                }
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
                IEnumerable<Guid?> allMatchOffers = db.MatchOffers.Select(mo => mo.MatchVolumeTypeId).Distinct();
                return db.MatchVolumeTypes.Where(mvt => !allMatchOffers.Contains(mvt.Id)).Select(mvt => mvt.Id).ToList();
            }
        }
        public static List<MatchVolumeType> GetUnusedMatchVolumeTypes()
        {
            using (MarketDbContext db = new MarketDbContext())
            {
                IEnumerable<Guid?> allMatchOffers = db.MatchOffers.Select(mo => mo.MatchVolumeTypeId).Distinct();
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
                IEnumerable<Guid?> allMatchOffers = db.MatchOffers.Select(mo => mo.MatchVolumeUnitId).Distinct();
                return db.MatchVolumeUnits.Where(mvu => !allMatchOffers.Contains(mvu.Id)).Select(mvu => mvu.Id).ToList();
            }
        }
        public static List<MatchVolumeUnit> GetUnusedMatchVolumeUnits()
        {
            using (MarketDbContext db = new MarketDbContext())
            {
                IEnumerable<Guid?> allMatchOffers = db.MatchOffers.Select(mo => mo.MatchVolumeUnitId).Distinct();
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
                IEnumerable<Guid> allProductCategories = db.ProductCategories.Select(mo => mo.MidCategoryId).Distinct();
                return db.MidCategories.Where(mc => !allProductCategories.Contains(mc.Id)).Select(mqu => mqu.Id).ToList();
            }
        }
        public static List<MidCategory> GetUnusedMidCategories()
        {
            using (MarketDbContext db = new MarketDbContext())
            {
                IEnumerable<Guid> allProductCategories = db.ProductCategories.Select(mo => mo.MidCategoryId).Distinct();
                return db.MidCategories.Where(mc => !allProductCategories.Contains(mc.Id)).ToList();
            }
        }

        public static List<Guid> GetUnusedTopCategoriesIds()
        {
            using (MarketDbContext db = new MarketDbContext())
            {
                IEnumerable<Guid> allMidCategories = db.MidCategories.Select(mo => mo.TopCategoryId).Distinct();
                return db.TopCategories.Where(mc => !allMidCategories.Contains(mc.Id)).Select(mqu => mqu.Id).ToList();
            }
        }
        public static List<TopCategory> GetUnusedTopCategories()
        {
            using (MarketDbContext db = new MarketDbContext())
            {
                IEnumerable<Guid> allMidCategories = db.MidCategories.Select(mo => mo.TopCategoryId).Distinct();
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

        public static DbDataStatus GetDbDataStatusLocal()
        {
            DbDataStatus status = new DbDataStatus();
            List<Guid> matchedPicGuids = new DirectoryInfo(CoreSettings.b2bDataLocalDir + CoreSettings.MatchedProductsPicturesPath).GetFiles().Select(f => new Guid(Path.GetFileNameWithoutExtension(f.Name))).ToList();
            try
            {
                using (MarketDbContext db = new MarketDbContext())
                {
                    status.IsConnected = true;
                    status.UnMatchedProductCategories = db.MatchProductCategories.Where(mpc => mpc.ProductCategoryId == null).Count();
                    status.UnMatchedVolumeTypes = db.MatchVolumeTypes.Where(mvt => mvt.VolumeTypeId == null).Count();
                    status.UnMatchedVolumeUnits = db.MatchVolumeUnits.Where(mvu => mvu.VolumeUnitId == null).Count();
                    status.UnMatchedQuantityUnits = db.MatchQuantityUnits.Where(mqu => mqu.QuantityUnitId == null).Count();
                    status.UnMatchedExtraProperties = db.MatchProductExtraPropertyTypes.Where(mpept => mpept.ProductExtraPropertyTypeId == null).Count();
                    status.UnMatchedOffers = db.MatchOffers.Where(mo => mo.OfferId == null).Count();
                    status.ConflictedPics = db.ConflictedPics.Count();
                    status.ProductsWithoutPics = db.Products.Select(p => p.Id).AsEnumerable().Except(matchedPicGuids).Count();
                    status.ConflictedDescs = db.ConflictedDescriptions.Count();
                    status.ProductsWithoutDescs = db.Products.Select(p => p.Id).AsEnumerable().Except(db.ProductDescriptions.Select(pd => pd.ProductId)).Count();
                }
            }
            catch
            {
                return status;
            }
            return status;
        }
    }
}
