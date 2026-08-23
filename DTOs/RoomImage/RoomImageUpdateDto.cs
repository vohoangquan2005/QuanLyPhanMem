using System.ComponentModel.DataAnnotations;

namespace HotelManagement.DTOs.RoomImage
{
    public class RoomImageUpdateDto
    {
        public IFormFile? Image { get; set; }
        [Range(0, 1, ErrorMessage = "IsPrimary chỉ được phép là 0 hoặc 1!")]
        public int IsPrimary { get; set; }      // Ảnh chính 1 <Yes> 0 <No>
        [Range(0, int.MaxValue, ErrorMessage = "Thứ tự hiển thị không được âm!")]
        public int DisplayOrder { get; set; }   // Thứ tự hiển thị
    }
}
