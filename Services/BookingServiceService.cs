using HotelManagement.Data;
using HotelManagement.DTOs.Booking;
using HotelManagement.DTOs.BookingService;
using HotelManagement.Enums;
using HotelManagement.Exceptions;
using HotelManagement.Models;
using Microsoft.EntityFrameworkCore;
using BookingServiceModel = HotelManagement.Models.BookingService;

namespace HotelManagement.Services
{
    public class BookingServiceService
    {
        private readonly AppDbContext _context;
        public BookingServiceService(AppDbContext context)
        {
            _context = context;
        }
        // Chuyển BookingService -> ResponseDto
        private BookingServiceResponseDto MapToResponseDto(BookingServiceModel bookingService)
        {
            return new BookingServiceResponseDto
            {
                BookingServiceId = bookingService.BookingServiceId,
                BookingId = bookingService.BookingId,
                ServiceId = bookingService.ServiceId,
                ServiceName = bookingService.Service?.ServiceName,
                Quantity = bookingService.Quantity,
                UnitPrice = bookingService.UnitPrice,
                TotalAmount = bookingService.TotalAmount,
                CreateAt = bookingService.CreateAt
            };
        }
        // Chuyển CreateDto -> BookingService
        private BookingServiceModel MapToEntity(BookingServiceCreateDto dto, Service service)
        {
            return new BookingServiceModel
            {
                BookingId = dto.BookingId,
                ServiceId = dto.ServiceId,
                Quantity = dto.Quantity,
                UnitPrice = service.UnitPrice,
                TotalAmount = dto.Quantity * service.UnitPrice,
            };
        }
        // Chuyển CreateOnlineDto -> BookingService
        private BookingServiceModel MapToEntityOnline(BookingServiceCreateOnlineDto dto, Service service, int booking)
        {
            return new BookingServiceModel
            {
                BookingId = booking,
                ServiceId = dto.ServiceId,
                Quantity = dto.Quantity,
                UnitPrice = service.UnitPrice,
                TotalAmount = dto.Quantity * service.UnitPrice,
            };
        }
        // Update BookingService từ UpdateDto
        private void UpdateEntity(BookingServiceModel bookingService, BookingServiceUpdateDto dto)
        {
            bookingService.Quantity = dto.Quantity;
            bookingService.TotalAmount = dto.Quantity * bookingService.UnitPrice;
        }
        // Check bookingId
        private async Task<Booking> GetValidBooking(int bookingId)
        {
            var booking = await _context.Bookings
                                        .FirstOrDefaultAsync(b => b.BookingId == bookingId);
            if (booking is null)
                throw new NotFoundException("Mã đặt phòng không tồn tại. Không thể thao tác!");
            if (booking.IsDeleted == ActiveEnums.Inactive)
                throw new BadRequestException("Đặt phòng đã bị xóa. Không thể thao tác!");
            if (booking.Status == BookingEnums.Cancelled)
                throw new BadRequestException("Đặt phòng đã bị hủy. Không thể thao tác!");
            if (booking.Status == BookingEnums.Rejected)
                throw new BadRequestException("Đặt phòng đã bị từ chối. Không thể thao tác!");
            if (booking.Status == BookingEnums.Completed)
                throw new BadRequestException("Đặt phòng đã hoàn thành. Không thể thao tác!");
            return booking;
        }
        // Check mã đặt phòng BookingId đã đặt dịch vụ ServiceId chưa?
        private async Task CheckBookingService(int bookingId, int serviceId)
        {
            var exists = await _context.BookingServices.AnyAsync(bs => bs.BookingId == bookingId
                                                                   && bs.ServiceId == serviceId);
            if (exists)
                throw new ConflictException("Dịch vụ đã được thêm vào đặt phòng trước đó rồi!");
        }
        // Lấy dịch vụ đang Active
        private async Task<Service> GetActiveService(int serviceId)
        {
            var service = await _context.Services
                                        .FirstOrDefaultAsync(s =>s.ServiceId == serviceId 
                                                              && s.IsDeleted == ActiveEnums.Active);
            if (service is null)
                throw new NotFoundException("Dịch vụ không tồn tại hoặc đã bị xóa!");
            return service;
        }
        
        // Tính tiền dịch  vụ của booking
        private async Task RecalculateBookingAmount(Booking booking)
        {
            var additionalServiceAmount = await _context.BookingServices
                                                        .Where(bs => bs.BookingId == booking.BookingId)
                                                        .SumAsync(bs => bs.TotalAmount);
            booking.AdditionalServiceAmount = additionalServiceAmount;
            booking.TotalAmount = booking.RoomAmount
                                + booking.PackageAmount
                                + booking.AdditionalServiceAmount
                                - booking.DiscountAmount;
            if (booking.TotalAmount < 0)
                booking.TotalAmount = 0;
        }
        // ===========================================================
        // =                           CRUD                          =
        // ===========================================================
        // Get all 
        public async Task<List<BookingServiceResponseDto>> GetAllAsync()
        {
            var bookingServices = await _context.BookingServices
                                                .Select(bs => new BookingServiceResponseDto
                                                {
                                                    BookingServiceId = bs.BookingServiceId,
                                                    BookingId = bs.BookingId,
                                                    ServiceId = bs.ServiceId,
                                                    ServiceName = bs.Service!.ServiceName,
                                                    Quantity = bs.Quantity,
                                                    UnitPrice = bs.UnitPrice,
                                                    TotalAmount = bs.TotalAmount,
                                                    CreateAt = bs.CreateAt
                                                })
                                                .ToListAsync();
            return bookingServices;
        }
        // Get by Id
        public async Task<BookingServiceResponseDto?> GetByIdAsync(int id)
        {
            var bookingService = await _context.BookingServices
                                               .Include(bs => bs.Service)
                                               .FirstOrDefaultAsync(bs => bs.BookingServiceId == id);
            if (bookingService is null) return null;
            return MapToResponseDto(bookingService);
        }
        // Lấy dánh sách dịch vụ đặt đã của khách hàng đang đăng nhập
        // Create
        public async Task<BookingServiceResponseDto> CreateAsync(BookingServiceCreateDto dto)
        {
            await using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var booking = await GetValidBooking(dto.BookingId);
                var service = await GetActiveService(dto.ServiceId);
                await CheckBookingService(dto.BookingId, dto.ServiceId);
                var bookingService = MapToEntity(dto, service);
                await _context.BookingServices.AddAsync(bookingService);
                await _context.SaveChangesAsync();
                await RecalculateBookingAmount(booking);
                booking.UpdateAt = DateTime.Now;
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                return (await GetByIdAsync(bookingService.BookingServiceId))!;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
        // Create online
        public async Task<BookingServiceResponseDto> CreateOnlineAsync(BookingServiceCreateOnlineDto dto, int bookingId)
        {
            await using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var booking = await GetValidBooking(bookingId);
                var service = await GetActiveService(dto.ServiceId);
                await CheckBookingService(bookingId, dto.ServiceId);
                var bookingService = MapToEntityOnline(dto, service, bookingId);
                await _context.BookingServices.AddAsync(bookingService);
                await _context.SaveChangesAsync();
                await RecalculateBookingAmount(booking);
                booking.UpdateAt = DateTime.Now;
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                return (await GetByIdAsync(bookingService.BookingServiceId))!;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
        // Update
        public async Task<BookingServiceResponseDto?> UpdateAsync(int id, BookingServiceUpdateDto dto)
        {
            await using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var bookingService = await _context.BookingServices
                                                   .FirstOrDefaultAsync(bs => bs.BookingServiceId == id);
                if (bookingService is null) return null;
                var booking = await GetValidBooking(bookingService.BookingId);
                UpdateEntity(bookingService, dto);
                await RecalculateBookingAmount(booking);
                booking.UpdateAt = DateTime.Now;
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                return await GetByIdAsync(id);
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
        // Delete
        public async Task<bool> DeleteAsync(int id)
        {
            await using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var bookingService = await _context.BookingServices
                                                   .FirstOrDefaultAsync(bs => bs.BookingServiceId == id);
                if (bookingService is null) return false;
                var booking = await GetValidBooking(bookingService.BookingId);
                _context.BookingServices.Remove(bookingService);
                await _context.SaveChangesAsync();
                await RecalculateBookingAmount(booking);
                booking.UpdateAt = DateTime.Now;
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                return true;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
    }
}
