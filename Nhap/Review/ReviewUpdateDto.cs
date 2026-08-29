using System.ComponentModel.DataAnnotations;

namespace HotelManagement.DTOs.Review
{
    public class ReviewUpdateDto
    {
        [Required(ErrorMessage = "Vui lòng nhập số sao!")]
        [Range(1, 5, ErrorMessage = "Số sao phải từ 1 đến 5!")]
        public int Rating { get; set; }
        [StringLength(1000, ErrorMessage = "Nội dung đánh giá tối đa 1000 ký tự!")]
        public string? Comment { get; set; }
    }
}
