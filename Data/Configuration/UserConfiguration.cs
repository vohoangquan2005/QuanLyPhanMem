using HotelManagement.Enums;
using HotelManagement.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
namespace HotelManagement.Data.Configuration
{
    public class UserConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            // Table
            builder.ToTable("Users");
            // Primary key
            builder.HasKey(u => u.UserId);
            // FullName
            builder.Property(u => u.FullName)
                   .IsRequired()
                   .HasMaxLength(50);
            // Email
            builder.Property(u => u.Email)
                   .IsRequired()
                   .HasMaxLength(100);
            builder.HasIndex(u => u.Email)
                   .IsUnique();
            // HashPassword
            builder.Property(u => u.HashPassword)
                   .IsRequired()
                   .HasMaxLength(255);
            // RoleId
            builder.Property(u => u.RoleId)
                   .IsRequired();
            // Locked
            builder.Property(u => u.IsLocked)
                   .IsRequired()
                   .HasConversion<string>()
                   .HasDefaultValue(ActiveEnums.Active);
            // Gender
            builder.Property(u => u.Gender)
                   .HasMaxLength(10);
            // DateOfBirth
            builder.Property(u => u.DateOfBirth);
            // PhoneNumber
            builder.Property(u => u.PhoneNumber)
                   .HasMaxLength(15);
            builder.HasIndex(u => u.PhoneNumber)
                   .IsUnique();
            // Avatar
            builder.Property(u => u.Avatar)
                   .HasMaxLength(500);
            // IsDeleted
            builder.Property(u => u.IsDeleted)
                   .IsRequired()
                   .HasConversion<string>()
                   .HasDefaultValue(ActiveEnums.Active);
            // CreateAt
            builder.Property(u => u.CreateAt)
                   .HasDefaultValueSql("GETDATE()");
            // UpdateAt
            builder.Property(u => u.UpdateAt)
                   .HasDefaultValueSql("GETDATE()");
        }
    }
}
