using ERP.Repository.Configuration.Enum;
using ERP.Repository.Model;
using ERP.Repository.Model.AuditLogs;
using ERP.Repository.Model.Forecast;
using ERP.Repository.Model.Inventories;
using ERP.Repository.Model.Orders;
using ERP.Repository.Model.Products;
using ERP.Repository.Model.Purchases;
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
        public DbSet<StockAdjustment> StockAdjustments { get; set; }
        public DbSet<InventoryLabel> InventoryLabels { get; set; }
        public DbSet<InventoryStatus> InventoryStatuses { get; set; }
        public DbSet<InventoryTransaction> InventoryTransactions { get; set; }
        #endregion

        #region Product
        public DbSet<Product> Products { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Unit> Units { get; set; }
        #endregion

        #region Order
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderType> OrderTypes { get; set; }
        public DbSet<OrderStatus> OrderStatuses { get; set; }
        public DbSet<Discount> Discounts { get; set; }
        public DbSet<DeliveryDriver> DeliveryDrivers { get; set; }
        public DbSet<OrderLine> OrderLines { get; set; }
        public DbSet<Sale> Sales { get; set; }
        public DbSet<SaleReturn> SaleReturns { get; set; }
        public DbSet<ForecastResult> ForecastResults { get; set; }
        public DbSet<ForecastWeek> ForecastWeeks { get; set; }
        #endregion

        #region Purchases
        public DbSet<Purchase> Purchases { get; set; }
        public DbSet<PurchaseLine> PurchaseLines { get; set; }
        public DbSet<PurchaseStatus> PurchaseStatuses { get; set; }
        #endregion

        #region AuditLogs
        public DbSet<AuditLog> AuditLogs { get; set; }
        #endregion

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            #region Setting up Primary Key

            #region UserAccounts
            modelBuilder.Entity<UserAccount>().HasKey(k => k.Id);
            modelBuilder.Entity<UserInformation>().HasKey(k => k.Id);
            modelBuilder.Entity<UserRole>().HasKey(k => k.Id);
            #endregion

            #region Inventory
            modelBuilder.Entity<Inventory>().HasKey(p => p.Id);
            modelBuilder.Entity<InventoryStatus>().HasKey(p => p.Id);
            modelBuilder.Entity<InventoryTransaction>().HasKey(p => p.Id);
            modelBuilder.Entity<StockAdjustment>().HasKey(p => p.Id);
            modelBuilder.Entity<InventoryLabel>().HasKey(p => p.Id);
            #endregion

            #region Orders
            modelBuilder.Entity<Order>().HasKey(p => p.Id);
            modelBuilder.Entity<OrderType>().HasKey(p => p.Id);
            modelBuilder.Entity<OrderStatus>().HasKey(p => p.Id);
            modelBuilder.Entity<DeliveryDriver>().HasKey(p => p.Id);
            modelBuilder.Entity<OrderLine>().HasKey(p => p.Id);
            modelBuilder.Entity<Sale>().HasKey(p => p.Id);
            modelBuilder.Entity<SaleReturn>().HasKey(p => p.Id);

            modelBuilder.Entity<Discount>().HasKey(p => p.Id);

            // The default keeps direct inserts (scripts/main.py) valid without naming the column.
            modelBuilder.Entity<Order>().Property(o => o.DiscountId).HasDefaultValue((int)DiscountTypeEnum.None);
            #endregion

            #region Products
            modelBuilder.Entity<Product>().HasKey(p => p.Id);
            modelBuilder.Entity<Category>().HasKey(p => p.Id);
            modelBuilder.Entity<Unit>().HasKey(p => p.Id);

            // The default keeps direct inserts (scripts/main.py) and existing products valid: they become pc.
            modelBuilder.Entity<Product>().Property(p => p.UnitId).HasDefaultValue(Unit.PieceId);
            #endregion

            #region Purchases
            modelBuilder.Entity<Purchase>().HasKey(p => p.Id);
            modelBuilder.Entity<PurchaseLine>().HasKey(p => p.Id);
            modelBuilder.Entity<PurchaseStatus>().HasKey(p => p.Id);
            #endregion

            modelBuilder.Entity<ForecastResult>().HasKey(k => k.Id);
            modelBuilder.Entity<ForecastWeek>().HasKey(k => k.Id);

            modelBuilder.Entity<AuditLog>().HasKey(k => k.Id);
            #endregion

            #region Soft Delete Default
            // IsActive defaults to true in the database too. The sentinel is true (matching the C# initializer),
            // so EF only leaves the column out of an INSERT when it's true and still writes an explicit false.
            foreach (
                var entity in modelBuilder
                    .Model.GetEntityTypes()
                    .Where(e => typeof(BaseModel).IsAssignableFrom(e.ClrType))
            )
            {
                modelBuilder
                    .Entity(entity.ClrType)
                    .Property(nameof(BaseModel.IsActive))
                    .HasDefaultValue(true)
                    .HasSentinel(true);
            }
            #endregion

            #region Setting up Relationships


            #region UserAccounts
            modelBuilder
                .Entity<UserAccount>()
                .HasOne(u => u.UserRole)
                .WithMany(u => u.UserAccounts)
                .HasForeignKey(u => u.UserRoleId);

            modelBuilder
                .Entity<UserInformation>()
                .HasOne(i => i.UserAccount)
                .WithOne(u => u.UserInformation)
                .HasForeignKey<UserInformation>(i => i.UserAccountId);

            #endregion

            modelBuilder
                .Entity<Inventory>()
                .HasOne(i => i.InventoryStatus)
                .WithMany(s => s.Inventories)
                .HasForeignKey(i => i.StatusId);

            modelBuilder
                .Entity<Inventory>()
                .HasOne(i => i.Product)
                .WithMany(w => w.Inventory)
                .HasForeignKey(i => i.ProductId);

            // One stock record per product; deleted rows don't block re-adding.
            modelBuilder.Entity<Inventory>().HasIndex(i => i.ProductId).IsUnique().HasFilter("\"IsActive\" = true");

            modelBuilder
                .Entity<InventoryTransaction>()
                .HasOne(t => t.Inventory)
                .WithMany(i => i.InventoryTransactions)
                .HasForeignKey(t => t.InventoryId);

            modelBuilder
                .Entity<StockAdjustment>()
                .HasOne(a => a.Inventory)
                .WithMany(i => i.StockAdjustments)
                .HasForeignKey(a => a.InventoryId);

            // A damaged return writes at most one adjustment.
            modelBuilder
                .Entity<StockAdjustment>()
                .HasOne(a => a.SaleReturn)
                .WithOne(r => r.Adjustment)
                .HasForeignKey<StockAdjustment>(a => a.SaleReturnId);

            modelBuilder.Entity<StockAdjustment>().HasIndex(a => a.Created_At);

            modelBuilder
                .Entity<InventoryTransaction>()
                .HasOne(d => d.InventoryLabel)
                .WithMany(i => i.InventoryTransactions)
                .HasForeignKey(d => d.InventoryLabelId);

            modelBuilder
                .Entity<Product>()
                .HasOne(i => i.Category)
                .WithMany(w => w.Products)
                .HasForeignKey(i => i.CategoryId);

            modelBuilder
                .Entity<Product>()
                .HasOne(p => p.Unit)
                .WithMany(u => u.Products)
                .HasForeignKey(p => p.UnitId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder
                .Entity<Order>()
                .HasOne(i => i.OrderType)
                .WithMany(w => w.Orders)
                .HasForeignKey(i => i.OrderTypeId);

            modelBuilder
                .Entity<Order>()
                .HasOne(i => i.OrderStatus)
                .WithMany(w => w.Orders)
                .HasForeignKey(i => i.OrderStatusId);

            modelBuilder
                .Entity<Order>()
                .HasOne(i => i.Discount)
                .WithMany(w => w.Orders)
                .HasForeignKey(i => i.DiscountId);

            modelBuilder
                .Entity<Order>()
                .HasOne(i => i.DeliveryDriver)
                .WithMany(w => w.Orders)
                .HasForeignKey(i => i.DeliveryDriverId);

            modelBuilder
                .Entity<OrderLine>()
                .HasOne(i => i.Order)
                .WithMany(w => w.OrderLines)
                .HasForeignKey(i => i.OrderId);

            modelBuilder
                .Entity<OrderLine>()
                .HasOne(i => i.Product)
                .WithMany(w => w.OrderLines)
                .HasForeignKey(i => i.ProductId);

            modelBuilder
                .Entity<OrderLine>()
                .HasOne(i => i.Inventory)
                .WithMany(w => w.OrderLines)
                .HasForeignKey(i => i.InventoryId);

            // One sale per completed order.
            modelBuilder.Entity<Sale>().HasOne(s => s.Order).WithOne(o => o.Sale).HasForeignKey<Sale>(s => s.OrderId);

            modelBuilder.Entity<Sale>().HasIndex(s => s.Created_At);

            modelBuilder.Entity<SaleReturn>().HasOne(r => r.Sale).WithMany(s => s.Returns).HasForeignKey(r => r.SaleId);

            modelBuilder.Entity<SaleReturn>().HasIndex(r => r.Created_At);

            modelBuilder
                .Entity<ForecastResult>()
                .HasOne(f => f.Product)
                .WithMany(p => p.ForecastResults)
                .HasForeignKey(f => f.ProductId);

            modelBuilder
                .Entity<ForecastWeek>()
                .HasOne(w => w.ForecastResult)
                .WithMany(f => f.Weeks)
                .HasForeignKey(w => w.ForecastResultId)
                .OnDelete(DeleteBehavior.Cascade);

            #region Purchases
            modelBuilder
                .Entity<Purchase>()
                .HasOne(p => p.PurchaseStatus)
                .WithMany(s => s.Purchases)
                .HasForeignKey(p => p.PurchaseStatusId);

            modelBuilder
                .Entity<PurchaseLine>()
                .HasOne(l => l.Purchase)
                .WithMany(p => p.Lines)
                .HasForeignKey(l => l.PurchaseId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder
                .Entity<PurchaseLine>()
                .HasOne(l => l.Product)
                .WithMany(p => p.PurchaseLines)
                .HasForeignKey(l => l.ProductId);

            modelBuilder.Entity<Purchase>().HasIndex(p => p.OrderedOn);
            #endregion

            #region AuditLogs
            modelBuilder
                .Entity<AuditLog>()
                .HasOne(a => a.UserAccount)
                .WithMany()
                .HasForeignKey(a => a.UserAccountId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<AuditLog>().Property(a => a.Module).HasConversion<string>();

            modelBuilder.Entity<AuditLog>().Property(a => a.Action).HasConversion<string>();

            modelBuilder.Entity<AuditLog>().HasIndex(a => a.Created_At).IsDescending();
            #endregion
            #endregion

            #region Seeding Default Values
            var seededAt = new DateTime(2025, 8, 1, 0, 0, 0, DateTimeKind.Utc);

            modelBuilder
                .Entity<UserRole>()
                .HasData(
                    new UserRole { Id = (int)UserRoleEnum.Owner, Role = "owner" },
                    new UserRole { Id = (int)UserRoleEnum.Secretary, Role = "secretary" }
                );

            modelBuilder
                .Entity<Discount>()
                .HasData(
                    new Discount
                    {
                        Id = (int)DiscountTypeEnum.None,
                        Type = "none",
                        Percent = 0m,
                    },
                    new Discount
                    {
                        Id = (int)DiscountTypeEnum.Senior,
                        Type = "senior",
                        Percent = 20m,
                    },
                    new Discount
                    {
                        Id = (int)DiscountTypeEnum.Pwd,
                        Type = "pwd",
                        Percent = 20m,
                    },
                    new Discount
                    {
                        Id = (int)DiscountTypeEnum.SoloParent,
                        Type = "solo_parent",
                        Percent = 10m,
                    },
                    new Discount
                    {
                        Id = (int)DiscountTypeEnum.Custom,
                        Type = "custom",
                        Percent = null,
                    }
                );

            modelBuilder
                .Entity<OrderType>()
                .HasData(
                    new OrderType { Id = (int)OrderTypeEnum.Walkin, Type = "walkin" },
                    new OrderType { Id = (int)OrderTypeEnum.Delivery, Type = "delivery" }
                );

            modelBuilder
                .Entity<OrderStatus>()
                .HasData(
                    new OrderStatus { Id = (int)OrderStatusEnum.Completed, Status = "completed" },
                    new OrderStatus { Id = (int)OrderStatusEnum.Processing, Status = "processing" },
                    new OrderStatus { Id = (int)OrderStatusEnum.Shipped, Status = "shipped" },
                    new OrderStatus { Id = (int)OrderStatusEnum.Cancelled, Status = "cancelled" }
                );

            modelBuilder
                .Entity<PurchaseStatus>()
                .HasData(
                    new PurchaseStatus { Id = (int)PurchaseStatusEnum.Ordered, Status = "ordered" },
                    new PurchaseStatus { Id = (int)PurchaseStatusEnum.Received, Status = "received" },
                    new PurchaseStatus { Id = (int)PurchaseStatusEnum.Cancelled, Status = "cancelled" }
                );

            modelBuilder
                .Entity<InventoryStatus>()
                .HasData(
                    new InventoryStatus { Id = (int)InventoryStatusEnum.Available, Status = "available" },
                    new InventoryStatus { Id = (int)InventoryStatusEnum.LowStock, Status = "low stock" },
                    new InventoryStatus { Id = (int)InventoryStatusEnum.Critical, Status = "critical" }
                );

            modelBuilder
                .Entity<InventoryLabel>()
                .HasData(
                    new InventoryLabel { Id = (int)InventoryLabelEnum.Sale, Type = "sale" },
                    new InventoryLabel { Id = (int)InventoryLabelEnum.Return, Type = "return" },
                    new InventoryLabel { Id = (int)InventoryLabelEnum.Adjustment, Type = "adjustment" },
                    new InventoryLabel { Id = (int)InventoryLabelEnum.Received, Type = "received" }
                );

            // Default units; the owner can add more. Ids 1-7, so the migration moves the Id sequence past them.
            modelBuilder
                .Entity<Unit>()
                .HasData(
                    new[] { "pc", "box", "pack", "bar", "roll", "set", "pair" }.Select(
                        (name, i) =>
                            new Unit
                            {
                                Id = i + 1,
                                Name = name,
                                IsActive = true,
                                Created_At = seededAt,
                            }
                    )
                );

            // Default accounts (password "mypassword"). Salt and hash are fixed literals so the model stays
            // deterministic; Password = Base64(SHA256(password + salt)), same as TokenManagerService.Hashed.
            // Account Id 1 must be the owner, since AccountValidation locks its role.
            modelBuilder
                .Entity<UserAccount>()
                .HasData(
                    new UserAccount
                    {
                        Id = 1,
                        UserRoleId = (int)UserRoleEnum.Owner,
                        Email = "owner@inventory.local",
                        Salt = "OmUBGSIov6YDrqLo4SaFuw==",
                        Password = "0ULmzYhoKo1MJ9WyeS3WAktEqN/QYeXPEb7z7akCUxM=",
                        IsActive = true,
                        Created_At = seededAt,
                    },
                    new UserAccount
                    {
                        Id = 2,
                        UserRoleId = (int)UserRoleEnum.Secretary,
                        Email = "secretary@inventory.local",
                        Salt = "ELFJLUKQLNZ1usWM/oILqA==",
                        Password = "lgKIp/sJWDvi74qQ/TYwXAsb7Bv/Z1fpdrDxsanRmqA=",
                        IsActive = true,
                        Created_At = seededAt,
                    }
                );

            modelBuilder
                .Entity<UserInformation>()
                .HasData(
                    new UserInformation
                    {
                        Id = 1,
                        UserAccountId = 1,
                        FirstName = "System",
                        LastName = "Owner",
                        Created_At = seededAt,
                    },
                    new UserInformation
                    {
                        Id = 2,
                        UserAccountId = 2,
                        FirstName = "System",
                        LastName = "Secretary",
                        Created_At = seededAt,
                    }
                );
            #endregion
        }
    }
}
