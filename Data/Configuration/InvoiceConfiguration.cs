using HotelManagement.Enums;
using HotelManagement.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HotelManagement.Data.Configuration
{
    public class InvoiceConfiguration : IEntityTypeConfiguration<Invoice>
    {
        public void Configure(EntityTypeBuilder<Invoice> builder)
        {
            // Table
            builder.ToTable("Invoices");
            // Primary Key
            builder.HasKey(i => i.InvoiceId);
            // BookingId
            builder.Property(i => i.BookingId)
                   .IsRequired();
            // SubTotal
            builder.Property(i => i.SubTotal)
                   .IsRequired()
                   .HasPrecision(18, 2);
            // TaxAmount
            builder.Property(i => i.TaxAmount)
                   .IsRequired()
                   .HasPrecision(18, 2)
                   .HasDefaultValue(0m);
            // DiscountAmount
            builder.Property(i => i.DiscountAmount)
                   .IsRequired()
                   .HasPrecision(18, 2);
            // AmountDue
            builder.Property(i => i.AmountDue)
                   .IsRequired()
                   .HasPrecision(18, 2);
            // Status
            builder.Property(i => i.Status)
                   .IsRequired()
                   .HasConversion<string>()
                   .HasDefaultValue(InvoiceEnums.Unpaid);
            // CreatedByUserId
            builder.Property(i => i.CreatedByUserId);
            // CreateAt
            builder.Property(i => i.CreateAt)
                   .HasDefaultValueSql("GETDATE()");
            builder.Property(i => i.CustomerFullName).HasMaxLength(200);
            builder.Property(i => i.CustomerCCCD).HasMaxLength(30);
            builder.Property(i => i.CustomerNationality).HasMaxLength(100);
            builder.Property(i => i.LineItemsJson);
            builder.HasIndex(i => i.BookingId).IsUnique();
            // Relationships
            // Booking 1 - N Invoice
            builder.HasOne(i => i.Booking)
                   .WithMany(b => b.Invoices)
                   .HasForeignKey(i => i.BookingId)
                   .OnDelete(DeleteBehavior.Restrict);
            // User 1 - N Invoice
            builder.HasOne(i => i.CreatedByUser)
                   .WithMany(u => u.CreatedInvoices)
                   .HasForeignKey(i => i.CreatedByUserId)
                   .OnDelete(DeleteBehavior.SetNull);
        }
    }
}
