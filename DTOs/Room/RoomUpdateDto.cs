using HotelManagement.Enums;
using System.ComponentModel.DataAnnotations;

namespace HotelManagement.DTOs.Room
{
    public class RoomUpdateDto
    {
        [Required(ErrorMessage = "Vui lòng nhập tên phòng!")]
        [StringLength(50, ErrorMessage = "Tên phòng không vượt quá 50 ký tự!")]
        public string? RoomName { get; set; }
        [Range(1, int.MaxValue, ErrorMessage = "Mã loại phòng không hợp lệ!")]
        public int RoomTypeId { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Sức chứa phải lớn hơn 0!")]
        public int Capacity { get; set; }   // Sức chứa
        [StringLength(100, ErrorMessage = "Vị trí không vượt quá 100 ký tự!")]
        public string? Location { get; set; }   // Địa chỉ: ví dụ tầng A dãy B
        [Range(0.01, double.MaxValue, ErrorMessage = "Giá phòng phải lớn hơn 0!")]
        public decimal PricePerNight { get; set; }  // Tiền phòng 1 đêm
        [StringLength(500, ErrorMessage = "Mô tả không vượt quá 500 ký tự!")]
        public string? Description { get; set; }
        public RoomEnums Status { get; set; }
    }
}
