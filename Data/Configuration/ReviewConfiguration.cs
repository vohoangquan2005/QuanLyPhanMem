using HotelManagement.Enums;
using HotelManagement.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HotelManagement.Data.Configuration
{
    public class ReviewConfiguration : IEntityTypeConfiguration<Review>
    {
        public void Configure(EntityTypeBuilder<Review> builder)
        {
            // Table
            builder.ToTable("Reviews");
            // Primary Key
            builder.HasKey(r => r.ReviewId);
            builder.HasIndex(r => new { r.CustomerId, r.RoomId, r.BookingId })
                   .IsUnique();
            // CustomerId
            builder.Property(r => r.CustomerId)
                   .IsRequired();
            // RoomId
            builder.Property(r => r.RoomId)
                   .IsRequired();
            // BookingId
            builder.Property(r => r.BookingId)
                   .IsRequired();
            // Rating
            builder.Property(r => r.Rating)
                   .IsRequired();
            // Comment
            builder.Property(r => r.Comment)
                   .HasMaxLength(1000);
            // Status
            builder.Property(r => r.Status)
                   .IsRequired()
                   .HasConversion<string>()
                   .HasDefaultValue(ReviewEnums.Pending);
            // CreateAt
            builder.Property(r => r.CreateAt)
                   .HasDefaultValueSql("GETDATE()");
            // Relationships

            // Customer 1 - N Review
            builder.HasOne(r => r.Customer)
                   .WithMany(c => c.Reviews)
                   .HasForeignKey(r => r.CustomerId)
                   .OnDelete(DeleteBehavior.Restrict);
            // Room 1 - N Review
            builder.HasOne(r => r.Room)
                   .WithMany(room => room.Reviews)
                   .HasForeignKey(r => r.RoomId)
                   .OnDelete(DeleteBehavior.Restrict);
            // Booking 1 - N Review
            builder.HasOne(r => r.Booking)
                   .WithMany(b => b.Reviews)
                   .HasForeignKey(r => r.BookingId)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
