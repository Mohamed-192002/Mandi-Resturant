using Core.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Data
{
    public class SiteDataContext : IdentityDbContext<User, Role, Guid, IdentityUserClaim<Guid>, UserRole, IdentityUserLogin<Guid>, RoleClaims, IdentityUserToken<Guid>>
    {
        public SiteDataContext(DbContextOptions<SiteDataContext> options) : base(options)
        {

        }

        public DbSet<ExpenseType> ExpenseTypes { get; set; }
        public DbSet<Customer> Customers { get; set; }
        public DbSet<Expense> Expenses { get; set; }
        public DbSet<Delivery> Deliveries { get; set; }
        public DbSet<SaleBill> SaleBills { get; set; }
        public DbSet<DeliveryBill> DeliveryBills { get; set; }
        public DbSet<SaleBillDetail> SaleBillDetails { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Hole> Holes { get; set; }
        public DbSet<MeatFilling> MeatFillings { get; set; }
        public DbSet<ChickenFilling> ChickenFillings { get; set; }
        public DbSet<ChickenHoleMovement> ChickenHoleMovements { get; set; }
        public DbSet<MeatHoleMovement> MeatHoleMovements { get; set; }
        public DbSet<Driver> Drivers { get; set; }
        public DbSet<PhoneNumberRecord> PhoneNumberRecords { get; set; }
        public DbSet<DeviceRegistration> DeviceRegistrations { get; set; }
        public DbSet<PrinterRegistration> PrinterRegistrations { get; set; }
        public DbSet<DriverPrice> DriverPrices { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // primary key
            modelBuilder.Entity<IdentityUserLogin<Guid>>().HasNoKey();
            modelBuilder.Entity<IdentityUserToken<Guid>>().HasNoKey();
            modelBuilder.Entity<UserRole>().HasKey(ur => new { ur.UserId, ur.RoleId });

            // relation many to many
            modelBuilder.Entity<UserRole>()
                .HasOne(ur => ur.Role)
                .WithMany(r => r.UserRole)
                .HasForeignKey(ur => ur.RoleId)
                .IsRequired();
            modelBuilder.Entity<UserRole>()
                .HasOne(ur => ur.User)
                .WithMany(r => r.UserRole)
                .HasForeignKey(ur => ur.UserId)
                .IsRequired();

        }
    }
}
