using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Reflection.Emit;
using DemoSanBong.ViewModels;

namespace DemoSanBong.Models
{
    public class AppDbContext : IdentityDbContext<AppUser>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<CustomerLevel> CustomerLevels { get; set; }
        public DbSet<FeedBack> FeedBacks { get; set; }

        public DbSet<Field> Fields { get; set; }
        public DbSet<FieldImage> FieldImages { get; set; }
        public DbSet<FieldRate> FieldRates { get; set; }

        public DbSet<Booking> Bookings { get; set; }
        public DbSet<BookingDetail> BookingDetails { get; set; }

        public DbSet<Service> Services { get; set; }
        public DbSet<ServiceRate> ServiceRates { get; set; }

        public DbSet<Invoice> Invoices { get; set; }
        public DbSet<InvoiceService> InvoiceServices { get; set; }

        public DbSet<GuestBill> GuestBills { get; set; }
        public DbSet<BillDetail> BillDetails { get; set; }
        
        public DbSet<Rules> Rules { get; set; }



        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<AppUser>(entity => { entity.ToTable(name: "Users"); });
            builder.Entity<IdentityRole>(entity => { entity.ToTable(name: "Roles"); });
            builder.Entity<IdentityUserRole<string>>(entity => { entity.ToTable("UserRoles"); });
            builder.Entity<IdentityUserClaim<string>>(entity => { entity.ToTable("UserClaims"); });
            builder.Entity<IdentityUserLogin<string>>(entity => { entity.ToTable("UserLogins"); });
            builder.Entity<IdentityRoleClaim<string>>(entity => { entity.ToTable("RoleClaims"); });
            builder.Entity<IdentityUserToken<string>>(entity => { entity.ToTable("UserTokens"); });

            builder.Entity<BookingDetail>().HasKey(i => new { i.BookingId, i.FieldId });
            builder.Entity<InvoiceService>().HasKey(i => new { i.ServiceId, i.InvoiceId });
            builder.Entity<BillDetail>().HasKey(i => new { i.ServiceId, i.GuestBillId });
            builder.Entity<FieldRate>().HasKey(i => new { i.FieldId, i.EffectiveDate });
            builder.Entity<ServiceRate>().HasKey(i => new { i.ServiceId, i.EffectiveDate });
            builder.Entity<FieldImage>().HasKey(i => new { i.FieldId, i.FileName });
            builder.Entity<FeedBack>().HasKey(i => new { i.CusId });

            builder.Entity<AppUser>()
                .HasOne(i => i.CustomerLevel)
                .WithMany()
                .HasForeignKey(i => i.Level);
            builder.Entity<FeedBack>()
                .HasOne(i => i.Customer)
                .WithMany()
                .HasForeignKey(i => i.CusId);

            builder.Entity<FieldRate>()
                .HasOne(i => i.Field)
                .WithMany()
                .HasForeignKey(i => i.FieldId);
            builder.Entity<FieldImage>()
                .HasOne(i => i.Field)
                .WithMany()
                .HasForeignKey(i => i.FieldId);

            builder.Entity<ServiceRate>()
                .HasOne(i => i.Service)
                .WithMany()
                .HasForeignKey(i => i.ServiceId);

            builder.Entity<Booking>()
                .HasOne(i => i.Customer)
                .WithMany()
                .HasForeignKey(i => i.CusID)
                .OnDelete(DeleteBehavior.SetNull);
            builder.Entity<BookingDetail>()
                .HasOne(i => i.Booking)
                .WithMany()
                .HasForeignKey(i => i.BookingId);
            builder.Entity<BookingDetail>()
                .HasOne(i => i.Field)
                .WithMany()
                .HasForeignKey(i => i.FieldId);

            builder.Entity<Invoice>()
                .HasOne(i => i.Booking)
                .WithMany()
                .HasForeignKey(i => i.BookingId);
            builder.Entity<Invoice>()
                .HasOne(i => i.Cashier)
                .WithMany()
                .HasForeignKey(i => i.CashierId);
            builder.Entity<InvoiceService>()
                .HasOne(i => i.Invoice)
                .WithMany()
                .HasForeignKey(i => i.InvoiceId);
            builder.Entity<InvoiceService>()
                .HasOne(i => i.Service)
                .WithMany()
                .HasForeignKey(i => i.ServiceId);

            builder.Entity<GuestBill>()
                .HasOne(i => i.Cashier)
                .WithMany()
                .HasForeignKey(i => i.CashierId);
            builder.Entity<BillDetail>()
                .HasOne(i => i.GuestBill)
                .WithMany()
                .HasForeignKey(i => i.GuestBillId);
            builder.Entity<BillDetail>()
                .HasOne(i => i.Service)
                .WithMany()
                .HasForeignKey(i => i.ServiceId);
        }
    }
}
