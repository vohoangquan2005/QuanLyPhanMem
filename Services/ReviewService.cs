using HotelManagement.Data;
using HotelManagement.DTOs.Review;
using HotelManagement.Enums;
using HotelManagement.Exceptions;
using HotelManagement.Models;
using Microsoft.EntityFrameworkCore;

namespace HotelManagement.Services
{
    public class ReviewService
    {
        private readonly AppDbContext _context;
        private readonly InvoiceService _invoiceService;
        public ReviewService(AppDbContext context, InvoiceService invoiceService)
        {
            _context = context;
            _invoiceService = invoiceService;
        }
        // Chuyển Review -> ResponseDto
        private ReviewResponseDto MapToResponseDto(Review review)
        {
            return new ReviewResponseDto
            {
                ReviewId = review.ReviewId,
                CustomerId = review.CustomerId,
                CustomerName = review.Customer?.User?.FullName,
                RoomId = review.RoomId,
                RoomName = review.Room?.RoomName,
                BookingId = review.BookingId,
                Rating = review.Rating,
                Comment = review.Comment,
                Status = review.Status,
                CreateAt = review.CreateAt
            };
        }
        // Chuyển ReviewCreateDto -> Review
        private Review MapToEntity(ReviewCreateDto dto)
        {
            return new Review
            {
                CustomerId = dto.CustomerId,
                RoomId = dto.RoomId,
                BookingId = dto.BookingId,
                Rating = dto.Rating,
                Comment = dto.Comment,
                Status = ReviewEnums.Pending
            };
        }
        // Update Review từ UpdateDto
        private void UpdateEntity(Review review, ReviewUpdateDto dto)
        {
            review.Rating = dto.Rating;
            review.Comment = dto.Comment;
        }
        // Check CustomerId có tồn tại trong bảng Customer không?
        private async Task CheckCustomerId(int customerId)
        {
            var exists = await _context.Customers.AnyAsync(c => c.CustomerId == customerId);
            if (!exists)
                throw new NotFoundException("Mã khách hàng không hợp lệ!");
        }
        // Check RoomId có tồn tại trong bảng Room không
        private async Task CheckRoomId(int roomId)
        {
            var exists = await _context.Rooms.AnyAsync(r => r.RoomId == roomId);
            if (!exists)
                throw new NotFoundException("Mã phòng không hợp lệ!");
        }
        // Check BookingId có tồn tại trong bảng Booking không
        private async Task CheckBookingId(int bookingId)
        {
            var exists = await _context.Bookings.AnyAsync(b => b.BookingId == bookingId);
            if (!exists)
                throw new NotFoundException("Mã đặt phòng không hợp lệ!");
        }
        // Check Customer + Room + Booking có liên quan với nhau không
        private async Task CheckCustomerBookingRoom(int customerId, int roomId, int bookingId)
        {
            var exists = await _context.Bookings
                                       .AnyAsync(b => b.BookingId == bookingId 
                                                   && b.CustomerId == customerId 
                                                   && b.IsDeleted == ActiveEnums.Active
                                                   && b.BookingDetails.Any(bd => bd.RoomId == roomId));
            if (!exists)
                throw new BadRequestException("Khách hàng, phòng và đặt phòng không hợp lệ!");
        }
        // Check đã đánh giá phòng trong Booking này chưa
        private async Task CheckReviewExists(int customerId, int roomId, int bookingId)
        {
            var exists = await _context.Reviews.AnyAsync(r =>r.CustomerId == customerId 
                                                          && r.RoomId == roomId 
                                                          && r.BookingId == bookingId);
            if (exists)
                throw new ConflictException("Khách hàng đã đánh giá phòng này trong đặt phòng!");
        }
        // ===========================================================
        // =                           CRUD                          =
        // ===========================================================
        // Get all
        public async Task<List<ReviewResponseDto>> GetAllAsync(bool includeModeration = true)
        {
            var query = _context.Reviews.AsQueryable();
            if (!includeModeration)
                query = query.Where(r => r.Status == ReviewEnums.Approved);

            var reviews = await query
                                        .Include(r => r.Customer)
                                            .ThenInclude(c => c!.User)
                                        .Include(r => r.Room)
                                        .Select(r => new ReviewResponseDto
                                        {
                                            ReviewId = r.ReviewId,
                                            CustomerId = r.CustomerId,
                                            CustomerName = r.Customer!.User!.FullName,
                                            RoomId = r.RoomId,
                                            RoomName = r.Room!.RoomName,
                                            BookingId = r.BookingId,
                                            Rating = r.Rating,
                                            Comment = r.Comment,
                                            Status = r.Status,
                                            CreateAt = r.CreateAt
                                        })
                                        .ToListAsync();
            return reviews;
        }
        // Get by Id
        public async Task<ReviewResponseDto?> GetByIdAsync(int id)
        {
            var review = await _context.Reviews.Include(r => r.Customer)
                                                   .ThenInclude(c => c!.User)
                                               .Include(r => r.Room)
                                               .FirstOrDefaultAsync(r => r.ReviewId == id);
            if (review is null) return null;
            return MapToResponseDto(review);
        }
        public async Task<ReviewResponseDto?> GetForCustomerByIdAsync(int id, int customerId)
        {
            var review = await _context.Reviews
                .Where(r => r.ReviewId == id &&
                    (r.CustomerId == customerId || r.Status == ReviewEnums.Approved))
                .Include(r => r.Customer).ThenInclude(c => c!.User)
                .Include(r => r.Room)
                .FirstOrDefaultAsync();
            return review is null ? null : MapToResponseDto(review);
        }

        public async Task<CustomerBookingReviewsDto> GetForCustomerBookingAsync(int bookingId, int customerId)
        {
            var booking = await _context.Bookings
                .Include(b => b.Invoices)
                .FirstOrDefaultAsync(b => b.BookingId == bookingId &&
                    b.CustomerId == customerId && b.IsDeleted == ActiveEnums.Active);
            if (booking is null)
                throw new NotFoundException("Không tìm thấy đặt phòng!");

            var finalAmount = booking.Invoices.FirstOrDefault()?.AmountDue
                ?? (booking.Status == BookingEnums.Completed
                    ? (await _invoiceService.BuildFinalSnapshotAsync(booking.BookingId)).AmountDue
                    : decimal.MaxValue);
            var netPaid = await _context.Payments
                .Where(p => (p.BookingId == booking.BookingId ||
                    (p.Invoice != null && p.Invoice.BookingId == booking.BookingId)) &&
                    p.Status == PaymentEnums.Success)
                .SumAsync(p => p.PaymentType == PaymentTypeEnums.Refund ? -p.Amount : p.Amount);

            var reviews = await _context.Reviews
                .Where(r => r.BookingId == bookingId && r.CustomerId == customerId)
                .Include(r => r.Customer).ThenInclude(c => c!.User)
                .Include(r => r.Room)
                .Select(r => new ReviewResponseDto
                {
                    ReviewId = r.ReviewId,
                    CustomerId = r.CustomerId,
                    CustomerName = r.Customer!.User!.FullName,
                    RoomId = r.RoomId,
                    RoomName = r.Room!.RoomName,
                    BookingId = r.BookingId,
                    Rating = r.Rating,
                    Comment = r.Comment,
                    Status = r.Status,
                    CreateAt = r.CreateAt
                })
                .ToListAsync();

            return new CustomerBookingReviewsDto
            {
                BookingId = bookingId,
                CanReview = booking.Status == BookingEnums.Completed && netPaid >= finalAmount,
                Reviews = reviews
            };
        }
        // Create
        public async Task<ReviewResponseDto> CreateAsync(
            ReviewCreateDto dto, int customerId)
        {
            dto.CustomerId = customerId;
            await CheckCustomerId(customerId);
            await CheckRoomId(dto.RoomId);
            await CheckBookingId(dto.BookingId);
            await CheckCustomerBookingRoom(dto.CustomerId, dto.RoomId, dto.BookingId);
            var booking = await _context.Bookings
                .Include(b => b.Invoices)
                .FirstAsync(b => b.BookingId == dto.BookingId && b.IsDeleted == ActiveEnums.Active);
            if (booking.Status != BookingEnums.Completed)
                throw new BadRequestException("Chỉ có thể đánh giá sau khi đặt phòng đã hoàn thành!");

            var finalAmount = booking.Invoices.FirstOrDefault()?.AmountDue
                ?? (await _invoiceService.BuildFinalSnapshotAsync(booking.BookingId)).AmountDue;
            var netPaid = await _context.Payments
                .Where(p => (p.BookingId == booking.BookingId ||
                    (p.Invoice != null && p.Invoice.BookingId == booking.BookingId)) &&
                    p.Status == PaymentEnums.Success)
                .SumAsync(p => p.PaymentType == PaymentTypeEnums.Refund ? -p.Amount : p.Amount);
            if (netPaid < finalAmount)
                throw new BadRequestException("Cần thanh toán đầy đủ hóa đơn đặt phòng trước khi đánh giá!");

            await CheckReviewExists(dto.CustomerId, dto.RoomId, dto.BookingId);
            var review = MapToEntity(dto);
            await _context.Reviews.AddAsync(review);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                var duplicate = await _context.Reviews.AnyAsync(r =>
                    r.CustomerId == dto.CustomerId &&
                    r.RoomId == dto.RoomId &&
                    r.BookingId == dto.BookingId);
                if (duplicate)
                    throw new ConflictException("Khách hàng đã đánh giá phòng này trong đặt phòng!");
                throw;
            }
            return await GetByIdAsync(review.ReviewId) 
                      ?? throw new NotFoundException("Không tìm thấy đánh giá vừa tạo!");
        }
        // Update
        public async Task<ReviewResponseDto?> UpdateAsync(int id, ReviewUpdateDto dto)
        {
            var review = await _context.Reviews.FindAsync(id);
            if (review is null) return null;
            if (review.Status != ReviewEnums.Pending)
                throw new BadRequestException("Chỉ có thể cập nhật đánh giá đang chờ xử lý!");
            UpdateEntity(review, dto);
            await _context.SaveChangesAsync();
            return await GetByIdAsync(id);
        }
        // Delete
        public async Task<bool> DeleteAsync(int id)
        {
            var review = await _context.Reviews.FindAsync(id);
            if (review is null) return false;
            if (review.Status != ReviewEnums.Pending)
                throw new BadRequestException("Chỉ có thể xóa đánh giá đang chờ xử lý!");
            _context.Reviews.Remove(review);
            await _context.SaveChangesAsync();
            return true;
        }
        // Xác nhận Review
        public async Task<bool> ConfirmAsync(int id)
        {
            var review = await _context.Reviews.FindAsync(id);
            if (review is null) return false;
            if (review.Status != ReviewEnums.Pending)
                throw new BadRequestException("Chỉ có thể xác nhận đánh giá đang chờ xử lý");
            review.Status = ReviewEnums.Rejected;
            await _context.SaveChangesAsync();
            return true;
        }
        // Từ chối Review
        public async Task<bool> RejectAsync(int id)
        {
            var review = await _context.Reviews.FindAsync(id);
            if (review is null) return false;
            if (review.Status != ReviewEnums.Pending)
                throw new BadRequestException("Chỉ có thể xác nhận đánh giá đang chờ xử lý");
            review.Status = ReviewEnums.Approved;
            await _context.SaveChangesAsync();
            return true;

        }
    }
}
