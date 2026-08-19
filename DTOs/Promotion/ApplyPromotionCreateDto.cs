using System.ComponentModel.DataAnnotations;

namespace HotelManagement.DTOs.Promotion
{
    public class ApplyPromotionCreateDto
    {
        [Range(1, int.MaxValue, ErrorMessage = "Mã loại phòng không hợp lệ!")]
        public int RoomTypeId { get; set; }
    }
}
