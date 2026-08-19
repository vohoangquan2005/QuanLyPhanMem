namespace HotelManagement.DTOs.Promotion
{
    public class PromotionUpdateRequestDto
    {
        public PromotionUpdateDto Promotion { get; set; } = new();
        public List<ApplyPromotionUpdateDto> ApplyPromotions { get; set; }  = new();
    }
}
