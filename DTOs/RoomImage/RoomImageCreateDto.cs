using System.ComponentModel.DataAnnotations;

namespace HotelManagement.DTOs.RoomImage
{
    public class RoomImageCreateDto
    {
        [Required(ErrorMessage = "Vui lòng nhập mã phòng!")]
        [Range(1, int.MaxValue, ErrorMessage = "Mã phòng không hợp lệ!")]
        public int RoomId { get; set; }
        [Required(ErrorMessage = "Vui lòng chọn hình ảnh!")]
        public IFormFile Image { get; set; } = null!;
        [Range(0, 1, ErrorMessage = "IsPrimary chỉ được phép là 0 hoặc 1!")]
        public int IsPrimary { get; set; }      // Ảnh chính 1 <Yes> 0 <No>
        [Range(0, int.MaxValue, ErrorMessage = "Thứ tự hiển thị không được âm!")]
        public int DisplayOrder { get; set; }   // Thứ tự hiển thị
    }
}
