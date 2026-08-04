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

            #endregion

            #region Setting up Relationships

            modelBuilder.Entity<UserAccount>()
                .HasOne(u => u.UserInformation)
                .WithOne(i => i.UserAccount)
                .HasForeignKey<UserInformation>(i => i.UserAccountId);

            modelBuilder.Entity<UserAccount>()
                .HasOne(u => u.UserType)
                .WithMany()
                .HasForeignKey(u => u.UserTypeId);  

            modelBuilder.Entity<UserAccount>()
                .HasOne(u => u.UserPosition)
                .WithMany()
                .HasForeignKey(u => u.UserPositionId);

            modelBuilder.Entity<UserInformation>()
                .HasOne(i => i.UserAccount)
                .WithOne(u => u.UserInformation)
                .HasForeignKey<UserInformation>(i => i.UserAccountId);

            #endregion

        }

    }
}
