using HotelManagement.Enums;
using HotelManagement.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HotelManagement.Data.Configuration
{
    public class ServicePackageConfiguration : IEntityTypeConfiguration<ServicePackage>
    {
        public void Configure(EntityTypeBuilder<ServicePackage> builder)
        {
            // Table
            builder.ToTable("ServicePackages");
            // Primary Key
            builder.HasKey(sp => sp.ServicePackageId);
            // PackageName
            builder.Property(sp => sp.PackageName)
                   .IsRequired()
                   .HasMaxLength(100);
            // PackagePrice
            builder.Property(sp => sp.PackagePrice)
                   .IsRequired()
                   .HasPrecision(18, 2);
            // Description
            builder.Property(sp => sp.Description)
                   .HasMaxLength(500);
            // IsDeleted
            builder.Property(sp => sp.IsDeleted)
                   .IsRequired()
                   .HasConversion<string>()
                   .HasDefaultValue(ActiveEnums.Active);
            // CreateAt
            builder.Property(sp => sp.CreateAt)
                   .HasDefaultValueSql("GETDATE()");
            // UpdateAt
            builder.Property(sp => sp.UpdateAt)
                   .HasDefaultValueSql("GETDATE()");
        }
    }
}
