namespace HotelManagement.DTOs.RoomImage
{
    public class RoomImageResponseDto
    {
        public int RoomImageId { get; set; }
        public int RoomId { get; set; }
        public string? RoomName { get; set; }
        public string? ImageUrl { get; set; }
        public int IsPrimary { get; set; }      // Ảnh chính 1 <Yes> 0 <No>
        public int DisplayOrder { get; set; }   // Thứ tự hiển thị
        public DateTime? CreateAt { get; set; }
        public DateTime? UpdateAt { get; set; }
    }
}
