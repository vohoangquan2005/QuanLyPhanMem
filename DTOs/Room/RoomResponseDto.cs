using HotelManagement.Enums;

using HotelManagement.DTOs.RoomImage;

namespace HotelManagement.DTOs.Room
{
    public class RoomResponseDto
    {
        public int RoomId { get; set; }
        public string? RoomName { get; set; }
        public int RoomTypeId { get; set; }
        public string? RoomTypeName { get; set; }
        public int Capacity { get; set; }       // Sức chứa
        public string? Location { get; set; }   // Vị trí: Tầng a khu b
        public decimal PricePerNight { get; set; }// Tiền phòng 1 đêm
        public string? Description { get; set; }
        public RoomEnums Status { get; set; }
        public ActiveEnums IsDeleted { get; set; } // Dùng để xóa tạm
        public DateTime? CreateAt { get; set; }
        public DateTime? UpdateAt { get; set; }
        public List<RoomImageResponseDto> RoomImages { get; set; } = new();
    }
}
