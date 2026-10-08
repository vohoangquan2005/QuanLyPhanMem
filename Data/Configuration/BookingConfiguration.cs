using HotelManagement.Enums;
using HotelManagement.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HotelManagement.Data.Configuration
{
    public class BookingConfiguration : IEntityTypeConfiguration<Booking>
    {
        public void Configure(EntityTypeBuilder<Booking> builder) 
        {
            // Table
            builder.ToTable("Bookings");
            // Primary Key
            builder.HasKey(b => b.BookingId);
            // CustomerId
            builder.Property(b => b.CustomerId)
                   .IsRequired();
            // BookingDate
            builder.Property(b => b.BookingDate)
                   .IsRequired();
            // ExpectedCheckIn
            builder.Property(b => b.ExpectedCheckIn)
                   .IsRequired();
            // ExpectedCheckOut
            builder.Property(b => b.ExpectedCheckOut)
                   .IsRequired();
            // GuestCount
            builder.Property(b => b.GuestCount)
                   .IsRequired();
            // RoomCount
            builder.Property(b => b.RoomCount)
                   .IsRequired();
            // PromotionId
            builder.Property(b => b.PromotionId);
            builder.Property(b => b.PromotionNameSnapshot).HasMaxLength(100);
            builder.Property(b => b.PromotionDiscountTypeSnapshot).HasConversion<string>().HasMaxLength(20);
            builder.Property(b => b.PromotionDiscountValueSnapshot).HasPrecision(18, 2);
            // ServicePackageId
            builder.Property(b => b.ServicePackageId);
            // DepositAmount
            builder.Property(b => b.DepositAmount)
                   .IsRequired()
                   .HasPrecision(18, 2);
            // RoomAmount
            builder.Property(b => b.RoomAmount)
                   .IsRequired()
                   .HasPrecision(18, 2);
            // PackageAmount
            builder.Property(b => b.PackageAmount)
                   .IsRequired()
                   .HasPrecision(18, 2);
            // DiscountAmount
            builder.Property(b => b.DiscountAmount)
                   .IsRequired()
                   .HasPrecision(18, 2);
            // AdditionalServiceAmount
            builder.Property(b => b.AdditionalServiceAmount)
                   .IsRequired()
                   .HasPrecision(18, 2);
            // TotalAmount
            builder.Property(b => b.TotalAmount)
                   .IsRequired()
                   .HasPrecision(18, 2);
            // Note
            builder.Property(b => b.Note)
                   .HasMaxLength(500);
            // Status
            builder.Property(b => b.Status)
                   .IsRequired()
                   .HasConversion<string>()
                   .HasDefaultValue(BookingEnums.Pending);
            // IsDeleted
            builder.Property(r => r.IsDeleted)
                   .IsRequired()
                   .HasConversion<string>()
                   .HasDefaultValue(ActiveEnums.Active);
            // CreateAt
            builder.Property(b => b.CreateAt)
                   .HasDefaultValueSql("GETDATE()");
            // UpdateAt
            builder.Property(b => b.UpdateAt)
                   .HasDefaultValueSql("GETDATE()");
            // Relationship
            // Customer 1 - N Booking
            builder.HasOne(b => b.Customer)
                   .WithMany(c => c.Bookings)
                   .HasForeignKey(b => b.CustomerId)
                   .OnDelete(DeleteBehavior.Restrict);
            // Promotion 1 - N Booking
            builder.HasOne(b => b.Promotion)
                   .WithMany(p => p.Bookings)
                   .HasForeignKey(b => b.PromotionId)
                   .OnDelete(DeleteBehavior.SetNull);
            // ServicePackage 1 - N Booking
            builder.HasOne(b => b.ServicePackage)
                   .WithMany(sp => sp.Bookings)
                   .HasForeignKey(b => b.ServicePackageId)
                   .OnDelete(DeleteBehavior.SetNull);
        }
    }
}
