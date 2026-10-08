using HotelManagement.Enums;
using HotelManagement.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HotelManagement.Data.Configuration
{
    public class RoomTypeConfiguration : IEntityTypeConfiguration<RoomType>
    {
        public void Configure(EntityTypeBuilder<RoomType> builder)
        {
            // Table
            builder.ToTable("RoomTypes");
            // Primary Key
            builder.HasKey(r => r.RoomTypeId);
            // RoomTypeName
            builder.Property(r => r.RoomTypeName)
                   .IsRequired()
                   .HasMaxLength(50);
            builder.HasIndex(r => r.RoomTypeName)
                   .IsUnique();
            // BedCount
            builder.Property(r => r.BedCount)
                   .IsRequired();
            // BedType
            builder.Property(r => r.BedType)
                   .IsRequired()
                   .HasMaxLength(50);
            // Size
            builder.Property(r => r.Size)
                   .HasMaxLength(50);
            // Description
            builder.Property(r => r.Description)
                   .HasMaxLength(500);
            // IsDeleted
            builder.Property(r => r.IsDeleted)
                   .IsRequired()
                   .HasConversion<string>()
                   .HasDefaultValue(ActiveEnums.Active);
            // CreateAt
            builder.Property(r => r.CreateAt)
                   .HasDefaultValueSql("GETDATE()");
            // UpdateAt
            builder.Property(r => r.UpdateAt)
                   .HasDefaultValueSql("GETDATE()");
        }
    }
}
