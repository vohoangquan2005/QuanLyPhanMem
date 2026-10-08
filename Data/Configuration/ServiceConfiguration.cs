using HotelManagement.Enums;
using HotelManagement.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HotelManagement.Data.Configuration
{
    public class ServiceConfiguration : IEntityTypeConfiguration<Service>
    {
        public void Configure(EntityTypeBuilder<Service> builder)
        {
            // Table
            builder.ToTable("Services");
            // Primary Key
            builder.HasKey(s => s.ServiceId);
            // ServiceName
            builder.Property(s => s.ServiceName)
                   .IsRequired()
                   .HasMaxLength(100);
            // UnitPrice
            builder.Property(s => s.UnitPrice)
                   .IsRequired()
                   .HasPrecision(18, 2);
            // Unit
            builder.Property(s => s.Unit)
                   .IsRequired()
                   .HasMaxLength(50);
            // Description
            builder.Property(s => s.Description)
                   .HasMaxLength(500);
            // IsDeleted
            builder.Property(s => s.IsDeleted)
                   .IsRequired()
                   .HasConversion<string>()
                   .HasDefaultValue(ActiveEnums.Active);
            // CreateAt
            builder.Property(s => s.CreateAt)
                   .HasDefaultValueSql("GETDATE()");
            // UpdateAt
            builder.Property(s => s.UpdateAt)
                   .HasDefaultValueSql("GETDATE()");
        }
    }
}
