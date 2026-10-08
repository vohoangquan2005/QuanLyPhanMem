using HotelManagement.Enums;
using HotelManagement.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HotelManagement.Data.Configuration
{
    public class CancellationConfiguration : IEntityTypeConfiguration<Cancellation>
    {
        public void Configure(EntityTypeBuilder<Cancellation> builder)
        {
            // Table
            builder.ToTable("Cancellations");

            // Primary Key
            builder.HasKey(c => c.CancellationId);
            // BookingId
            builder.Property(c => c.BookingId)
                   .IsRequired();
            // CancellationPolicyId
            builder.Property(c => c.CancellationPolicyId)
                   .IsRequired();
            // Reason
            builder.Property(c => c.Reason)
                   .HasMaxLength(500);
            // RefundPercentage
            builder.Property(c => c.RefundPercentage)
                   .IsRequired()
                   .HasPrecision(5, 2);
            // RefundAmount
            builder.Property(c => c.RefundAmount)
                   .IsRequired()
                   .HasPrecision(18, 2);
            // Status
            builder.Property(c => c.Status)
                   .IsRequired()
                   .HasConversion<string>()
                   .HasDefaultValue(CancellationEnums.Pending);
            // CreateAt
            builder.Property(c => c.CreateAt)
                   .HasDefaultValueSql("GETDATE()");
            // Relationships
            // Booking 1 - N Cancellation
            builder.HasOne(c => c.Booking)
                   .WithMany(b => b.Cancellations)
                   .HasForeignKey(c => c.BookingId)
                   .OnDelete(DeleteBehavior.Restrict);
            // CancellationPolicy 1 - N Cancellation
            builder.HasOne(c => c.CancellationPolicy)
                   .WithMany(cp => cp.Cancellations)
                   .HasForeignKey(c => c.CancellationPolicyId)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
