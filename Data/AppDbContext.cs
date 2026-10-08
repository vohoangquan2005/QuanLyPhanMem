using HotelManagement.Models;
using Microsoft.EntityFrameworkCore;
namespace HotelManagement.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
        public DbSet<ApplyPromotion> ApplyPromotions { get; set; }
        public DbSet<AuditLog> AuditLogs { get; set; }
        public DbSet<Booking> Bookings { get; set; }
        public DbSet<BookingDetail> BookingDetails { get; set; }
        public DbSet<BookingService> BookingServices { get; set; }
        public DbSet<Cancellation> Cancellations { get; set; }
        public DbSet<CancellationPolicy> CancellationPolicies { get; set; }
        public DbSet<Customer> Customers { get; set; }
        public DbSet<FavoriteRoom> FavoriteRooms { get; set; }
        public DbSet<Guest> Guests { get; set; }
        public DbSet<Invoice> Invoices { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<Promotion> Promotions { get; set; }
        public DbSet<Review> Reviews { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<Room> Rooms { get; set; }
        public DbSet<RoomImage> RoomImages { get; set; }
        public DbSet<RoomType> RoomTypes { get; set; }
        public DbSet<Service> Services { get; set; }
        public DbSet<ServicePackage> ServicePackages { get; set; } 
        public DbSet<User> Users { get; set; }

        // Fluent API
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            // Tự tìm các class Fluent API và áp dụng vào file này
            modelBuilder.ApplyConfigurationsFromAssembly(
                typeof(AppDbContext).Assembly
            );
        }
        // Tự động cập nhật UpdateAt mỗi khi SaveChangesAsync
        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            var entries = ChangeTracker.Entries();
            foreach (var entry in entries)
            {
                if (entry.State == EntityState.Modified)
                {
                    var property = entry.Properties
                        .FirstOrDefault(p => p.Metadata.Name == "UpdateAt");
                    if (property != null)
                    {
                        property.CurrentValue = DateTime.Now;
                    }
                }
            }
            return await base.SaveChangesAsync(cancellationToken);
        }
    }
}