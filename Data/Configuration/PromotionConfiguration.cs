using HotelManagement.Enums;
using HotelManagement.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HotelManagement.Data.Configuration
{
    public class PromotionConfiguration : IEntityTypeConfiguration<Promotion>
    {
        public void Configure(EntityTypeBuilder<Promotion> builder)
        {
            // Table
            builder.ToTable("Promotions");
            // Primary Key
            builder.HasKey(p => p.PromotionId);
            // PromotionCode
            builder.Property(p => p.PromotionCode)
                   .IsRequired()
                   .HasMaxLength(50);
            builder.HasIndex(p => p.PromotionCode)
                   .IsUnique();
            // PromotionName
            builder.Property(p => p.PromotionName)
                   .IsRequired()
                   .HasMaxLength(100);
            // DiscountValue
            builder.Property(p => p.DiscountValue)
                   .IsRequired()
                   .HasPrecision(18, 2)
                   .HasDefaultValue(0m);
            builder.Property(p => p.DiscountType)
                   .IsRequired()
                   .HasConversion<string>()
                   .HasMaxLength(20)
                   .HasDefaultValue(PromotionDiscountTypeEnums.Percentage);
            // MinAmount
            builder.Property(p => p.MinAmount)
                   .HasPrecision(18, 2)
                   .HasDefaultValue(0m);
            // MaxDisCountAmount
            builder.Property(p => p.MaxDisCountAmount)
                   .HasPrecision(18, 2);
            // UsageLimit
            builder.Property(p => p.UsageLimit);
            // UsedCount
            builder.Property(p => p.UsedCount)
                   .HasDefaultValue(0);
            // StartDate
            builder.Property(p => p.StartDate)
                   .IsRequired();
            // EndDate
            builder.Property(p => p.EndDate)
                   .IsRequired();
            // IsDeleted
            builder.Property(p => p.IsDeleted)
                   .IsRequired()
                   .HasConversion<string>()
                   .HasDefaultValue(ActiveEnums.Active);
            // CreateAt
            builder.Property(p => p.CreateAt)
                   .HasDefaultValueSql("GETDATE()");
            // UpdateAt
            builder.Property(p => p.UpdateAt)
                   .HasDefaultValueSql("GETDATE()");
        }
    }
}
