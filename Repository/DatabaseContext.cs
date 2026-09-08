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
        public DbSet<UserPosition> UserPositions { get; set; }
        public DbSet<UserType> UserTypes { get; set; }
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
        #endregion

        #region Sales
        public DbSet<Sale> Sales { get; set; }
        #endregion

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            #region Setting up Primary Key

            #region UserAccounts
            modelBuilder.Entity<UserAccount>()
                .HasKey(k => k.Id);
            modelBuilder.Entity<UserInformation>()
                .HasKey(k => k.Id);
            modelBuilder.Entity<UserPosition>()
                .HasKey(k => k.Id);
            modelBuilder.Entity<UserType>()
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

            #endregion

            #region Setting up Relationships


            #region UserAccounts    
            modelBuilder.Entity<UserAccount>()
                .HasOne(u => u.UserType)
                .WithMany(u => u.UserAccounts)
                .HasForeignKey(u => u.UserTypeId);  

            modelBuilder.Entity<UserAccount>()
                .HasOne(u => u.UserPosition)
                .WithMany(u => u.UserAccounts)
                .HasForeignKey(u => u.UserPositionId);

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

            #endregion

        }

    }
}