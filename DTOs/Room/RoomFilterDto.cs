namespace HotelManagement.DTOs.Room
{
    public class RoomFilterDto
    {
        public int? RoomTypeId { get; set; }
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
        public int? MinCapacity { get; set; }
        public int? MaxCapacity { get; set; }
    }
}
