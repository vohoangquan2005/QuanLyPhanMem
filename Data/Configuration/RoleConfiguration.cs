using HotelManagement.Enums;
using HotelManagement.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
namespace HotelManagement.Data.Configuration
{
    public class RoleConfiguration : IEntityTypeConfiguration<Role>
    {
        public void Configure(EntityTypeBuilder<Role> builder)
        {
            // Table
            builder.ToTable("Roles");
            // Primary Key
            builder.HasKey(r => r.RoleId);
            // RoleName
            builder.Property(r => r.RoleName)
                   .IsRequired()
                   .HasMaxLength(50);
            // Description
            builder.Property(r => r.Description)
                   .HasMaxLength(255);
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
            // Unique RoleName
            builder.HasIndex(r => r.RoleName)
                   .IsUnique();
            // Relationship
            // Roles 1-n Users
            builder.HasMany(r => r.Users)
                   .WithOne(u => u.Role)
                   .HasForeignKey(u => u.RoleId)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
