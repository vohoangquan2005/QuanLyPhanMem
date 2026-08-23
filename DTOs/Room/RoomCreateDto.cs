using HotelManagement.Enums;
using System.ComponentModel.DataAnnotations;

namespace HotelManagement.DTOs.Room
{
    public class RoomCreateDto
    {
        [Required(ErrorMessage = "Vui lòng nhập tên phòng!")]
        [StringLength(50, ErrorMessage = "Tên phòng không vượt quá 50 ký tự!")]
        public string? RoomName { get; set; }
        [Range(1, int.MaxValue, ErrorMessage = "Mã loại phòng không hợp lệ!")]
        public int RoomTypeId { get; set; }
        [Required(ErrorMessage = "Vui lòng nhập sức chứa của phòng!")]
        public int Capacity { get; set; }       // Sức chứa
        [StringLength(100, ErrorMessage = "Địa chỉ không vượt quá 100 ký tự!")]
        public string? Location { get; set; }   // Vị trí: Tầng a khu b
        [Required(ErrorMessage = "Vui lòng nhập giá tiền 1 đêm!")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Giá phòng phải lớn hơn 0!")]
        public decimal PricePerNight { get; set; }// Tiền phòng 1 đêm
        [StringLength(500, ErrorMessage = "Mô tả không vượt quá 500 ký tự!")]
        public string? Description { get; set; }
    }
}
