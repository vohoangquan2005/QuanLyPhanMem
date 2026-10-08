using HotelManagement.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HotelManagement.Data.Configuration
{
    public class BookingServiceConfiguration : IEntityTypeConfiguration<BookingService>
    {
        public void Configure(EntityTypeBuilder<BookingService> builder) 
        {
            // Table
            builder.ToTable("BookingServices");
            // Primary Key
            builder.HasKey(bs => bs.BookingServiceId);
            // BookingId
            builder.Property(bs => bs.BookingId)
                   .IsRequired();
            // ServiceId
            builder.Property(bs => bs.ServiceId)
                   .IsRequired();
            // Quantity
            builder.Property(bs => bs.Quantity)
                   .IsRequired();
            // UnitPrice
            builder.Property(bs => bs.UnitPrice)
                   .IsRequired()
                   .HasPrecision(18, 2);
            // TotalAmount
            builder.Property(bs => bs.TotalAmount)
                   .IsRequired()
                   .HasPrecision(18, 2);
            // CreateAt
            builder.Property(bs => bs.CreateAt)
                   .HasDefaultValueSql("GETDATE()");
            // Relationship
            // Booking 1 - N BookingService
            builder.HasOne(bs => bs.Booking)
                   .WithMany(b => b.BookingServices)
                   .HasForeignKey(bs => bs.BookingId)
                   .OnDelete(DeleteBehavior.Cascade);
            // Service 1 - N BookingService
            builder.HasOne(bs => bs.Service)
                   .WithMany(s => s.BookingServices)
                   .HasForeignKey(bs => bs.ServiceId)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
