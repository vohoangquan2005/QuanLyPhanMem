using HotelManagement.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HotelManagement.Data.Configuration
{
    public class ApplyPromotionConfiguration : IEntityTypeConfiguration<ApplyPromotion>
    {
        public void Configure(EntityTypeBuilder<ApplyPromotion> builder)
        {
            builder.ToTable("ApplyPromotions");
            // Primary Key
            builder.HasKey(a => a.ApplyPromotionId);
            // PromotionId
            builder.Property(a => a.PromotionId)
                   .IsRequired();
            // RoomTypeId
            builder.Property(a => a.RoomTypeId)
                   .IsRequired();
            // CreateAt
            builder.Property(a => a.CreateAt)
                   .HasDefaultValueSql("GETDATE()");
            // UpdateAt
            builder.Property(a => a.UpdateAt)
                   .HasDefaultValueSql("GETDATE()");
            // Relationship
            // Promotion 1 - N PromotionRoomType
            builder.HasOne(a => a.Promotion)
                   .WithMany(p => p.ApplyPromotions)
                   .HasForeignKey(a => a.PromotionId)
                   .OnDelete(DeleteBehavior.Cascade);
            // Relationship
            // RoomType 1 - N PromotionRoomType
            builder.HasOne(a => a.RoomType)
                   .WithMany(rt => rt.ApplyPromotions)
                   .HasForeignKey(a => a.RoomTypeId)
                   .OnDelete(DeleteBehavior.Restrict);
            // Không cho một khuyến mãi áp dụng trùng một loại phòng
            builder.HasIndex(a => new
                    {
                        a.PromotionId,
                        a.RoomTypeId
                    })
                    .IsUnique();
        }
    }
}
