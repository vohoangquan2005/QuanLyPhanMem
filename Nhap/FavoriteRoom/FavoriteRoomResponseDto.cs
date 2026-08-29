namespace HotelManagement.DTOs.FavoriteRoom
{
    public class FavoriteRoomResponseDto
    {
        public int FavoriteRoomId { get; set; }
        public int CustomerId { get; set; }
        public string? CustomerName { get; set; }
        public int RoomId { get; set; }
        public string? RoomName { get; set; }
        public DateTime? CreateAt { get; set; }
    }
}
