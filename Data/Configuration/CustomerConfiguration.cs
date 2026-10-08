using HotelManagement.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HotelManagement.Data.Configuration
{
    public class CustomerConfiguration : IEntityTypeConfiguration<Customer>
    {
        public void Configure(EntityTypeBuilder<Customer> builder)
        {
            // Table
            builder.ToTable("Customers");
            // Primary
            builder.HasKey(c => c.CustomerId);
            // UserId
            builder.Property(c => c.UserId)
                   .IsRequired();
            // CCCD
            builder.Property(c => c.CCCD)
                   .IsRequired()
                   .HasMaxLength(12);
            builder.HasIndex(c => c.CCCD)
                   .IsUnique();
            // Nationality
            builder.Property(c => c.Nationality)
                   .HasMaxLength(50);
            // CreateAt
            builder.Property(c => c.CreateAt)
                   .HasDefaultValueSql("GETDATE()");
            // UpdateAt
            builder.Property(c => c.UpdateAt)
                   .HasDefaultValueSql("GETDATE()");
            // Relationship
            // Customer 1-1 User
            builder.HasOne(c => c.User)
                   .WithOne(u => u.Customer)
                   .HasForeignKey<Customer>(c => c.UserId)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}