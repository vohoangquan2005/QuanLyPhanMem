using HotelManagement.DTOs.BookingDetail;
using HotelManagement.DTOs.BookingService;
using HotelManagement.Enums;

namespace HotelManagement.DTOs.Booking
{
    public class BookingResponseDto
    {
        public int BookingId { get; set; }
        // Thông tin từ Customers
        public int CustomerId { get; set; }
        public string? FullName { get; set; }
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        // Thông tin Bookings
        public DateTime BookingDate { get; set; }       // Ngày đặt phòng
        public DateTime ExpectedCheckIn { get; set; }   // Ngày nhận phòng dự kiến
        public DateTime ExpectedCheckOut { get; set; }  // Ngày trả phòng dự kiến
        public int GuestCount { get; set; }             // Số lượng khách ở
        public int RoomCount { get; set; }              // Số phòng đã đặt
        // Thông tin từ Promotions
        public int? PromotionId { get; set; }
        public string? PromotionCode { get; set; }
        public string? PromotionName { get; set; }
        public PromotionDiscountTypeEnums? DiscountType { get; set; }
        public decimal? DiscountValue { get; set; } = 0;
        // Thông tin từ ServicePackages
        public int? ServicePackageId { get; set; }
        public string? PackageName { get; set; }
        // Thông tin tiền
        public decimal DepositAmount { get; set; }      // Tiền đặt cọc
        public decimal RoomAmount { get; set; }         // Tiền phòng (PricePerNight * số ngày ở) của tất cả Booking Detail
        public decimal PackageAmount { get; set; }      // Tiền gói dịch vụ
        public decimal DiscountAmount { get; set; }     // Tiền giảm khuyến mãi
        public decimal AdditionalServiceAmount { get; set; }    // Tiền dịch vụ đặt thêm
        public decimal TotalAmount { get; set; }        // Tổng số tiền của tất cả chi tiết phòng
        public string? Note { get; set; }
        public BookingEnums Status { get; set; }
        public ActiveEnums IsDeleted { get; set; }
        public DateTime? CreateAt { get; set; }
        public DateTime? UpdateAt { get; set; }
        // Danh sách phòng
        public List<BookingDetailResponseDto> BookingDetails { get; set; } = new();

        // Danh sách dịch vụ
        public List<BookingServiceResponseDto> BookingServices { get; set; } = new();
    }
}
