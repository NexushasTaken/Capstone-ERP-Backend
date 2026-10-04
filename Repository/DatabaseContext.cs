using ERP.Repository.Configuration.Enum;
using ERP.Repository.Model.AuditLogs;
using ERP.Repository.Model.Forecast;
using ERP.Repository.Model.Inventories;
using ERP.Repository.Model.Orders;
using ERP.Repository.Model.Products;
using ERP.Repository.Model.Sales;
using ERP.Repository.Model.UserAccounts;
using Microsoft.EntityFrameworkCore;

namespace ERP.Repository
{
    public class DatabaseContext(DbContextOptions<DatabaseContext> context) : DbContext(context)
    {
        #region UserAccounts
        public DbSet<UserAccount> UserAccounts { get; set; }
        public DbSet<UserInformation> UserInformations { get; set; }
        public DbSet<UserRole> UserRoles { get; set; }
        #endregion

        #region Inventory
        public DbSet<Inventory> Inventories { get; set; }
        public DbSet<DamagedInventory> DamagedInventories { get; set; }
        public DbSet<InventoryLabel> InventoryLabels { get; set; }
        public DbSet<InventoryStatus> InventoryStatuses { get; set; }
        public DbSet<InventoryTransaction> InventoryTransactions { get; set; }
        public DbSet<Warehouse> Warehouses { get; set; }
        #endregion

        #region Product
        public DbSet<Product> Products { get; set; }
        public DbSet<Category> Categories { get; set; }
        #endregion

        #region Order
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderType> OrderTypes { get; set; }
        public DbSet<OrderStatus> OrderStatuses { get; set; }
        public DbSet<DeliveryDriver> DeliveryDrivers { get; set; }
        public DbSet<OrderLine> OrderLines { get; set; }
        public DbSet<ForecastResult> ForecastResults { get; set; }
        #endregion

        #region Sales
        public DbSet<Sale> Sales { get; set; }
        #endregion

        #region AuditLogs
        public DbSet<AuditLog> AuditLogs { get; set; }
        #endregion

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            #region Setting up Primary Key

            #region UserAccounts
            modelBuilder.Entity<UserAccount>()
                .HasKey(k => k.Id);
            modelBuilder.Entity<UserInformation>()
                .HasKey(k => k.Id);
            modelBuilder.Entity<UserRole>()
                .HasKey(k => k.Id);
            #endregion

            #region Inventory
            modelBuilder.Entity<Inventory>()
                .HasKey(p => p.Id);
            modelBuilder.Entity<InventoryStatus>()
                .HasKey(p => p.Id);
            modelBuilder.Entity<InventoryTransaction>()
                .HasKey(p => p.Id);
            modelBuilder.Entity<Warehouse>()
                .HasKey(p => p.Id);
            modelBuilder.Entity<DamagedInventory>()
                .HasKey(p => p.Id);
            modelBuilder.Entity<InventoryLabel>()
                .HasKey(p => p.Id);
            #endregion

            #region Orders
            modelBuilder.Entity<Order>()
                .HasKey(p => p.Id);
            modelBuilder.Entity<OrderType>()
                .HasKey(p => p.Id);
            modelBuilder.Entity<OrderStatus>()
                .HasKey(p => p.Id);
            modelBuilder.Entity<DeliveryDriver>()
                .HasKey(p => p.Id);
            modelBuilder.Entity<OrderLine>()
                .HasKey(p => p.Id);
            #endregion

            #region Products
            modelBuilder.Entity<Product>()
                .HasKey(p => p.Id);
            modelBuilder.Entity<Category>()
                .HasKey(p => p.Id);
            #endregion

            #region Sales
            modelBuilder.Entity<Sale>()
                .HasKey(p => p.Id);
            #endregion

            modelBuilder.Entity<ForecastResult>()
            .HasKey(k => k.Id);

            modelBuilder.Entity<AuditLog>()
                .HasKey(k => k.Id);
            #endregion

            #region Setting up Relationships


            #region UserAccounts
            modelBuilder.Entity<UserAccount>()
                .HasOne(u => u.UserRole)
                .WithMany(u => u.UserAccounts)
                .HasForeignKey(u => u.UserRoleId);

            modelBuilder.Entity<UserInformation>()
                .HasOne(i => i.UserAccount)
                .WithOne(u => u.UserInformation)
                .HasForeignKey<UserInformation>(i => i.UserAccountId);

            #endregion

            modelBuilder.Entity<Inventory>()
                .HasOne(i => i.InventoryStatus)
                .WithMany(s => s.Inventories)
                .HasForeignKey(i => i.StatusId);

            modelBuilder.Entity<Inventory>()
                .HasOne(i => i.Warehouse)
                .WithMany(w => w.Inventory)
                .HasForeignKey(i => i.WarehouseId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<Inventory>()
                .HasOne(i => i.Product)
                .WithMany(w => w.Inventory)
                .HasForeignKey(i => i.ProductId);

            modelBuilder.Entity<InventoryTransaction>()
             .HasOne(t => t.Inventory)
             .WithMany(i => i.InventoryTransactions)
             .HasForeignKey(t => t.InventoryId);

            modelBuilder.Entity<DamagedInventory>()
                .HasOne(d => d.Inventory)
                .WithMany(i => i.DamagedInventories)
                .HasForeignKey(d => d.InventoryId);

            modelBuilder.Entity<InventoryTransaction>()
                .HasOne(d => d.InventoryLabel)
                .WithMany(i => i.InventoryTransactions)
                .HasForeignKey(d => d.InventoryLabelId);

            modelBuilder.Entity<Product>()
                .HasOne(i => i.Category)
                .WithMany(w => w.Products)
                .HasForeignKey(i => i.CategoryId);

            modelBuilder.Entity<Order>()
                .HasOne(i => i.OrderType)
                .WithMany(w => w.Orders)
                .HasForeignKey(i => i.OrderTypeId);

            modelBuilder.Entity<Order>()
                .HasOne(i => i.OrderStatus)
                .WithMany(w => w.Orders)
                .HasForeignKey(i => i.OrderStatusId);

            modelBuilder.Entity<Order>()
                .HasOne(i => i.DeliveryDriver)
                .WithMany(w => w.Orders)
                .HasForeignKey(i => i.DeliveryDriverId);

            modelBuilder.Entity<OrderLine>()
                .HasOne(i => i.Order)
                .WithMany(w => w.OrderLines)
                .HasForeignKey(i => i.OrderId);

            modelBuilder.Entity<OrderLine>()
                .HasOne(i => i.Product)
                .WithMany(w => w.OrderLines)
                .HasForeignKey(i => i.ProductId);

            modelBuilder.Entity<OrderLine>()
                .HasOne(i => i.Inventory)
                .WithMany(w => w.OrderLines)
                .HasForeignKey(i => i.InventoryId);

            modelBuilder.Entity<ForecastResult>()
                .HasOne(f => f.Inventory)
                .WithMany(f => f.ForecastResults)
                .HasForeignKey(f => f.InventoryId);

            #region AuditLogs
            modelBuilder.Entity<AuditLog>()
                .HasOne(a => a.UserAccount)
                .WithMany()
                .HasForeignKey(a => a.UserAccountId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<AuditLog>()
                .Property(a => a.Module)
                .HasConversion<string>();

            modelBuilder.Entity<AuditLog>()
                .Property(a => a.Action)
                .HasConversion<string>();

            modelBuilder.Entity<AuditLog>()
                .HasIndex(a => a.Created_At)
                .IsDescending();
            #endregion
            #endregion

            #region Seeding Default Values
            var seededAt = new DateTime(2025, 8, 1, 0, 0, 0, DateTimeKind.Utc);

            modelBuilder.Entity<UserRole>().HasData(
                new UserRole { Id = (int)UserRoleEnum.Owner, Role = "owner", IsActive = true, Created_At = seededAt },
                new UserRole { Id = (int)UserRoleEnum.Secretary, Role = "secretary", IsActive = true, Created_At = seededAt });

            modelBuilder.Entity<OrderType>().HasData(
                new OrderType { Id = (int)OrderTypeEnum.Walkin, Type = "walkin", IsActive = true, Created_At = seededAt },
                new OrderType { Id = (int)OrderTypeEnum.Delivery, Type = "delivery", IsActive = true, Created_At = seededAt });

            modelBuilder.Entity<OrderStatus>().HasData(
                new OrderStatus { Id = (int)OrderStatusEnum.Completed, Status = "completed", IsActive = true, Created_At = seededAt },
                new OrderStatus { Id = (int)OrderStatusEnum.Processing, Status = "processing", IsActive = true, Created_At = seededAt },
                new OrderStatus { Id = (int)OrderStatusEnum.Shipped, Status = "shipped", IsActive = true, Created_At = seededAt },
                new OrderStatus { Id = (int)OrderStatusEnum.Cancelled, Status = "cancelled", IsActive = true, Created_At = seededAt });

            modelBuilder.Entity<InventoryStatus>().HasData(
                new InventoryStatus { Id = (int)InventoryStatusEnum.Available, Status = "available", IsActive = true, Created_At = seededAt },
                new InventoryStatus { Id = (int)InventoryStatusEnum.LowStock, Status = "low stock", IsActive = true, Created_At = seededAt },
                new InventoryStatus { Id = (int)InventoryStatusEnum.Critical, Status = "critical", IsActive = true, Created_At = seededAt });

            modelBuilder.Entity<InventoryLabel>().HasData(
                new InventoryLabel { Id = (int)InventoryLabelEnum.Purchase, Type = "purchase", IsActive = true, Created_At = seededAt },
                new InventoryLabel { Id = (int)InventoryLabelEnum.Return, Type = "return", IsActive = true, Created_At = seededAt },
                new InventoryLabel { Id = (int)InventoryLabelEnum.Restock, Type = "restock", IsActive = true, Created_At = seededAt },
                new InventoryLabel { Id = (int)InventoryLabelEnum.Damage, Type = "damage", IsActive = true, Created_At = seededAt });

            // Default accounts (password "mypassword"). Salt and hash are fixed literals so the model stays
            // deterministic; Password = Base64(SHA256(password + salt)), same as TokenManagerService.Hashed.
            // Account Id 1 must be the owner, since AccountValidation locks its role.
            modelBuilder.Entity<UserAccount>().HasData(
                new UserAccount
                {
                    Id = 1,
                    UserRoleId = (int)UserRoleEnum.Owner,
                    Email = "owner@gmail.com",
                    Salt = "OmUBGSIov6YDrqLo4SaFuw==",
                    Password = "0ULmzYhoKo1MJ9WyeS3WAktEqN/QYeXPEb7z7akCUxM=",
                    IsActive = true,
                    Created_At = seededAt
                },
                new UserAccount
                {
                    Id = 2,
                    UserRoleId = (int)UserRoleEnum.Secretary,
                    Email = "secretary@gmail.com",
                    Salt = "ELFJLUKQLNZ1usWM/oILqA==",
                    Password = "lgKIp/sJWDvi74qQ/TYwXAsb7Bv/Z1fpdrDxsanRmqA=",
                    IsActive = true,
                    Created_At = seededAt
                });

            modelBuilder.Entity<UserInformation>().HasData(
                new UserInformation { Id = 1, UserAccountId = 1, FirstName = "Bill", LastName = "Gates", IsActive = true, Created_At = seededAt },
                new UserInformation { Id = 2, UserAccountId = 2, FirstName = "Mark", LastName = "Zucherbeard", IsActive = true, Created_At = seededAt });
            #endregion

        }

    }
}