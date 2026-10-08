using HotelManagement.Enums;
using HotelManagement.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HotelManagement.Data.Configuration
{
    public class PaymentConfiguration : IEntityTypeConfiguration<Payment>
    {
        public void Configure(EntityTypeBuilder<Payment> builder)
        {
            // Table
            builder.ToTable("Payments");
            // Primary Key
            builder.HasKey(p => p.PaymentId);
            // InvoiceId
            builder.Property(p => p.InvoiceId);
            builder.Property(p => p.BookingId);
            builder.Property(p => p.RequestKey).HasMaxLength(160);
            builder.Property(p => p.RefundSource).HasMaxLength(80);
            builder.Property(p => p.InvoiceSnapshotJson);
            // Amount
            builder.Property(p => p.Amount)
                   .IsRequired()
                   .HasPrecision(18, 2);
            // PaymentType
            builder.Property(p => p.PaymentType)
                   .IsRequired()
                   .HasConversion<string>()
                   .HasDefaultValue(PaymentTypeEnums.Payment);
            // PaymentMethod
            builder.Property(p => p.PaymentMethod)
                   .HasMaxLength(30);
            // TransactionCode
            builder.Property(p => p.TransactionCode)
                   .HasMaxLength(100);
            // Status
            builder.Property(p => p.Status)
                   .IsRequired()
                   .HasConversion<string>()
                   .HasDefaultValue(PaymentEnums.Pending);
            // CreatedByUserId
            builder.Property(p => p.CreatedByUserId);
            // PaymentDate
            builder.Property(p => p.PaymentDate);
            // CreateAt
            builder.Property(p => p.CreateAt)
                   .HasDefaultValueSql("GETDATE()");
            // Relationships
            // Invoice 1 - N Payment
            builder.HasOne(p => p.Invoice)
                   .WithMany(i => i.Payments)
                   .HasForeignKey(p => p.InvoiceId)
                   .OnDelete(DeleteBehavior.Cascade);
            builder.HasOne(p => p.Booking)
                   .WithMany(b => b.Payments)
                   .HasForeignKey(p => p.BookingId)
                   .OnDelete(DeleteBehavior.Restrict);
            // User 1 - N Payment
            builder.HasOne(p => p.CreatedByUser)
                   .WithMany(u => u.CreatedPayments)
                   .HasForeignKey(p => p.CreatedByUserId)
                   .OnDelete(DeleteBehavior.SetNull);
            builder.HasIndex(p => p.RequestKey)
                   .IsUnique()
                   .HasFilter("[RequestKey] IS NOT NULL AND [Status] <> 'Failed'");
        }
    }
}
