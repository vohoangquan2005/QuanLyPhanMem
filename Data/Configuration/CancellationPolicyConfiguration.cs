using HotelManagement.Enums;
using HotelManagement.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HotelManagement.Data.Configuration
{
    public class CancellationPolicyConfiguration : IEntityTypeConfiguration<CancellationPolicy>
    {
        public void Configure(EntityTypeBuilder<CancellationPolicy> builder)
        {
            // Table
            builder.ToTable("CancellationPolicies");
            // Primary Key
            builder.HasKey(cp => cp.CancellationPolicyId);
            // PolicyName
            builder.Property(cp => cp.PolicyName)
                   .IsRequired()
                   .HasMaxLength(100);
            // CancellationHours
            builder.Property(cp => cp.CancellationTime)
                   .IsRequired();
            // CancellatioonUnitTime
            builder.Property(cp => cp.CancellationTimeUnit)
                   .IsRequired()
                   .HasConversion<string>()
                   .HasDefaultValue(CancellationTimeUnitEnums.Hour);
            // RefundPercentage
            builder.Property(cp => cp.RefundPercentage)
                   .IsRequired()
                   .HasPrecision(5, 2);
            // Description
            builder.Property(cp => cp.Description)
                   .HasMaxLength(500);
            // Isdeleted
            builder.Property(cp => cp.IsDeleted)
                   .IsRequired()
                   .HasConversion<string>()
                   .HasDefaultValue(ActiveEnums.Active);
            // CreateAt
            builder.Property(cp => cp.CreateAt)
                   .HasDefaultValueSql("GETDATE()");
            // UpdateAt
            builder.Property(cp => cp.UpdateAt)
                   .HasDefaultValueSql("GETDATE()");
        }
    }
}
