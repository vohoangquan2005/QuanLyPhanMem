using HotelManagement.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
namespace HotelManagement.Data.Configuration
{
    public class RoomImageConfiguration : IEntityTypeConfiguration<RoomImage>
    {
        public void Configure(EntityTypeBuilder<RoomImage> builder)
        {
            // Table
            builder.ToTable("RoomImages");
            // Primary
            builder.HasKey(r => r.RoomImageId);
            // RoomId
            builder.Property(r => r.RoomId)
                   .IsRequired();
            // ImageUrl
            builder.Property(r => r.ImageUrl)
                   .IsRequired()
                   .HasMaxLength(500);
            // IsPrimary
            builder.Property(r => r.IsPrimary)
                   .IsRequired()
                   .HasDefaultValue(0);
            // CreateAt
            builder.Property(r => r.CreateAt)
                   .HasDefaultValueSql("GETDATE()");
            // UpdateAt
            builder.Property(r => r.UpdateAt)
                   .HasDefaultValueSql("GETDATE()");
            // Relationship
            // RoomImage n-1 Room
            builder.HasOne(r => r.Room)
                   .WithMany(room => room.RoomImages)
                   .HasForeignKey(r => r.RoomId)
                   .OnDelete(DeleteBehavior.Cascade);   // Xóa Phòng thì mất Ảnh
        }
    }
}
