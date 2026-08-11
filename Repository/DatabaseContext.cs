using ERP.Repository.Model.Inventories;
using ERP.Repository.Model.UserAccounts;
using Microsoft.EntityFrameworkCore;

namespace ERP.Repository
{
    public class DatabaseContext(DbContextOptions<DatabaseContext> context) : DbContext(context)
    {
        public DbSet<UserAccount> UserAccounts { get; set; }
        public DbSet<UserInformation> UserInformations { get; set; }
        public DbSet<UserPosition> UserPositions { get; set; }
        public DbSet<UserType> UserTypes { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            #region Setting up Primary Key

            modelBuilder.Entity<UserAccount>()
                .HasKey(k => k.Id);
            modelBuilder.Entity<UserInformation>()
                .HasKey(k => k.Id);
            modelBuilder.Entity<UserPosition>()
                .HasKey(k => k.Id);
            modelBuilder.Entity<UserType>()
                .HasKey(k => k.Id);
            modelBuilder.Entity<Inventory>()
                .Property(p => p.Id)
                .ValueGeneratedOnAdd();
            modelBuilder.Entity<InventoryStatus>()
                .Property(p => p.Id)
                .ValueGeneratedOnAdd();
            modelBuilder.Entity<InventoryTransaction>()
                .Property(p => p.Id)
                .ValueGeneratedOnAdd();
            modelBuilder.Entity<Warehouse>()
                .Property(p => p.Id)
                .ValueGeneratedOnAdd();
            modelBuilder.Entity<DamagedInventory>()
                .Property(p => p.Id)
                .ValueGeneratedOnAdd();
            modelBuilder.Entity<InventoryLabel>()
                .Property(p => p.Id)
                .ValueGeneratedOnAdd();

            #endregion

            #region Setting up Relationships

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

            modelBuilder.Entity<Inventory>()
                .HasOne(i => i.InventoryStatus)
                .WithOne(s => s.Inventory)
                .HasForeignKey<Inventory>(i => i.StatusId);

            modelBuilder.Entity<Inventory>()
                .HasOne(i => i.Warehouse)
                .WithMany(w => w.Inventory)
                .HasForeignKey(i => i.WarehouseId);

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

            #endregion

        }

    }
}
