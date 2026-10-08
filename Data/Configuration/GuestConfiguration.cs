using HotelManagement.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HotelManagement.Data.Configuration
{
    public class GuestConfiguration : IEntityTypeConfiguration<Guest>
    {
        public void Configure(EntityTypeBuilder<Guest> builder)
        {
            // Table
            builder.ToTable("Guests");
            // Primary Key
            builder.HasKey(g => g.GuestId);
            // BookingDetailId
            builder.Property(g => g.BookingDetailId)
                   .IsRequired();
            // FullName
            builder.Property(g => g.FullName)
                   .IsRequired()
                   .HasMaxLength(100);
            // DateOfBirth
            builder.Property(g => g.DateOfBirth);
            // Gender
            builder.Property(g => g.Gender)
                   .HasMaxLength(20);
            // Nationality
            builder.Property(g => g.Nationality)
                   .HasMaxLength(50)
                   .HasDefaultValue("Việt Nam");
            // PhoneNumber
            builder.Property(g => g.PhoneNumber)
                   .HasMaxLength(10);
            // Email
            builder.Property(g => g.Email)
                   .HasMaxLength(100);
            // Note
            builder.Property(g => g.Note)
                   .HasMaxLength(500);
            // CreateAt
            builder.Property(g => g.CreateAt)
                   .HasDefaultValueSql("GETDATE()");
            // UpdateAt
            builder.Property(g => g.UpdateAt)
                   .HasDefaultValueSql("GETDATE()");
            // Relationship
            // BookingDetail 1 - N Guest
            builder.HasOne(g => g.BookingDetail)
                   .WithMany(bd => bd.Guests)
                   .HasForeignKey(g => g.BookingDetailId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
