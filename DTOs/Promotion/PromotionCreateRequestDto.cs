namespace HotelManagement.DTOs.Promotion
{
    public class PromotionCreateRequestDto
    {
        public PromotionCreateDto Promotion { get; set; } = new();
        public List<ApplyPromotionCreateDto> ApplyPromotions { get; set; } = new();
    }
}
