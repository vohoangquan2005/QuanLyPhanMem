using HotelManagement.Enums;
using System.ComponentModel.DataAnnotations;

namespace HotelManagement.DTOs.RoomType
{
    public class RoomTypeUpdateDto
    {
        [Required(ErrorMessage = "Vui lòng nhập tên loại phòng!")]
        [StringLength(50, ErrorMessage = "Tên loại phòng không vượt quá 50 ký tự!")]
        public string? RoomTypeName { get; set; }
        [Required(ErrorMessage = "Vui lòng nhập số lượng giường!")]
        [Range(0, int.MaxValue, ErrorMessage = "Số giường không được nhỏ hơn 0!")]
        public int BedCount { get; set; }
        [Required(ErrorMessage = "Vui lòng nhập loại giường!")]
        [StringLength(50, ErrorMessage = "Loại giường không vượt quá 50 ký tự!")]
        public string? BedType { get; set; }
        [Required(ErrorMessage = "Vui lòng nhập kích thước phòng!")]
        public string? Size { get; set; }           // Kích thước dài x rộng
        [StringLength(500, ErrorMessage = "Mô tả không vượt quá 500 ký tự!")]
        public string? Description { get; set; }
    }
}
