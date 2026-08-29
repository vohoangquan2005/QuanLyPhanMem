using System.ComponentModel.DataAnnotations;

namespace HotelManagement.DTOs.FavoriteRoom
{
    public class FavoriteRoomCreateDto
    {
        [Required(ErrorMessage = "Vui lòng nhập mã phòng!")]
        [Range(1, int.MaxValue, ErrorMessage = "Mã phòng không hợp lệ!")]
        public int RoomId { get; set; }
    }
}
