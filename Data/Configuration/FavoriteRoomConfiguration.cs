using HotelManagement.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HotelManagement.Data.Configuration
{
    public class FavoriteRoomConfiguration : IEntityTypeConfiguration<FavoriteRoom>
    {
        public void Configure(EntityTypeBuilder<FavoriteRoom> builder)
        {
            // Table
            builder.ToTable("FavoriteRooms");
            // Primary Key
            builder.HasKey(fr => fr.FavoriteRoomId);
            // CustomerId
            builder.Property(fr => fr.CustomerId)
                   .IsRequired();
            // RoomId
            builder.Property(fr => fr.RoomId)
                   .IsRequired();
            // CreateAt
            builder.Property(fr => fr.CreateAt)
                   .HasDefaultValueSql("GETDATE()");
            // Relationships
            // Customer 1 - N FavoriteRoom
            builder.HasOne(fr => fr.Customer)
                   .WithMany(c => c.FavoriteRooms)
                   .HasForeignKey(fr => fr.CustomerId)
                   .OnDelete(DeleteBehavior.Cascade);
            // Room 1 - N FavoriteRoom
            builder.HasOne(fr => fr.Room)
                   .WithMany(r => r.FavoriteRooms)
                   .HasForeignKey(fr => fr.RoomId)
                   .OnDelete(DeleteBehavior.Cascade);
            // Không cho cùng một Customer yêu thích cùng một Room nhiều lần
            builder.HasIndex(fr => new
            {
                fr.CustomerId,
                fr.RoomId
            }).IsUnique();
        }
    }
}
