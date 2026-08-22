namespace HotelManagement.DTOs.Service
{
    public class ServiceSearchDto
    {
        public string? Keyword { get; set; }
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
    }
}
