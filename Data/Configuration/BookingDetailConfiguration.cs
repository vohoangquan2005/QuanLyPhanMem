using HotelManagement.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HotelManagement.Data.Configuration
{
    public class BookingDetailConfiguration : IEntityTypeConfiguration<BookingDetail>
    {
        public void Configure(EntityTypeBuilder<BookingDetail> builder) 
        {
            builder.ToTable("BookingDetails");
            // Primary Key
            builder.HasKey(bd => bd.BookingDetailId);
            // BookingId
            builder.Property(bd => bd.BookingId)
                   .IsRequired();
            // RoomId
            builder.Property(bd => bd.RoomId)
                   .IsRequired();
            // ActualCheckIn
            builder.Property(bd => bd.ActualCheckIn);
            // ActualCheckOut
            builder.Property(bd => bd.ActualCheckOut);
            // PricePerNight
            builder.Property(bd => bd.PricePerNight)
                   .IsRequired()
                   .HasPrecision(18, 2);
            // TotalAmount
            builder.Property(bd => bd.TotalAmount)
                   .IsRequired()
                   .HasPrecision(18, 2);
            // GuestCount
            builder.Property(bd => bd.GuestCount)
                   .IsRequired();
            // CreateAt
            builder.Property(bd => bd.CreateAt)
                   .HasDefaultValueSql("GETDATE()");
            // Relationship
            // Booking 1 - N BookingDetail
            builder.HasOne(bd => bd.Booking)
                   .WithMany(b => b.BookingDetails)
                   .HasForeignKey(bd => bd.BookingId)
                   .OnDelete(DeleteBehavior.Cascade);
            // Room 1 - N BookingDetail
            builder.HasOne(bd => bd.Room)
                   .WithMany(r => r.BookingDetails)
                   .HasForeignKey(bd => bd.RoomId)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
