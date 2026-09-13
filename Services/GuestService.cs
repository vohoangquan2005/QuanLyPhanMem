using HotelManagement.Data;
using HotelManagement.DTOs.Guest;
using HotelManagement.Enums;
using HotelManagement.Exceptions;
using HotelManagement.Models;
using Microsoft.EntityFrameworkCore;

namespace HotelManagement.Services
{
    public class GuestService
    {
        private readonly AppDbContext _context;
        private readonly AuditLogService _auditLogService;
        public GuestService(AppDbContext context, AuditLogService auditLogService)
        {
            _context = context;
            _auditLogService = auditLogService;
        }
        // Chuyển Guest -> ResponseDto
        private GuestResponseDto MapToResponseDto(Guest guest)
        {
            return new GuestResponseDto
            {
                GuestId = guest.GuestId,
                BookingDetailId = guest.BookingDetailId,
                FullName = guest.FullName,
                DateOfBirth = guest.DateOfBirth,
                Gender = guest.Gender,
                Nationality = guest.Nationality,
                PhoneNumber = guest.PhoneNumber,
                Email = guest.Email,
                Note = guest.Note,
                CreateAt = guest.CreateAt,
                UpdateAt = guest.UpdateAt
            };
        }
        private Guest MapToEntity(GuestCreateDto dto)
        {
            return new Guest
            {
                BookingDetailId = dto.BookingDetailId,
                FullName = dto.FullName,
                DateOfBirth = dto.DateOfBirth,
                Gender = dto.Gender,
                Nationality = dto.Nationality,
                PhoneNumber = dto.PhoneNumber,
                Email = dto.Email,
                Note = dto.Note
            };
        }
        // Cập nhật Guest từ GuestUpdateDto
        private void UpdateEntity(Guest guest, GuestUpdateDto dto)
        {
            guest.BookingDetailId = dto.BookingDetailId;
            guest.FullName = dto.FullName;
            guest.DateOfBirth = dto.DateOfBirth;
            guest.Gender = dto.Gender;
            guest.Nationality = dto.Nationality;
            guest.PhoneNumber = dto.PhoneNumber;
            guest.Email = dto.Email;
            guest.Note = dto.Note;
        }
        // Check BookingDetailId có tồn tại trong bảng BookingDetail không?
        private async Task CheckBookingDetailId(int id)
        {
            var exists = await _context.BookingDetails.AnyAsync(bd => bd.BookingDetailId == id);
            if (!exists)
                throw new NotFoundException("Mã chi tiết đặt phòng không tồn tại!");
        }
        // Check số lượng khách với sức chứa của phòng
        private async Task CheckGuestCount(int bookingDetailId)
        {
            var bookingDetail = await _context.BookingDetails.FirstOrDefaultAsync(bd => bd.BookingDetailId == bookingDetailId);
            if (bookingDetail is null)
                throw new NotFoundException("Mã chi tiết đặt phòng không tồn tại!");
            var guestCount = await _context.Guests.CountAsync(g => g.BookingDetailId == bookingDetailId);
            if (guestCount >= bookingDetail.GuestCount)
                throw new BadRequestException("Số lượng khách đã đạt giới hạn của chi tiết đặt phòng!");
        }
        // ===========================================================
        // =                           CRUD                          =
        // ===========================================================
        // Get all 
        public async Task<List<GuestResponseDto>> GetAllAsync()
        {
            var guests = await _context.Guests.Select(g => new GuestResponseDto
            {
                GuestId = g.GuestId,
                BookingDetailId = g.BookingDetailId,
                FullName = g.FullName,
                DateOfBirth = g.DateOfBirth,
                Gender = g.Gender,
                Nationality = g.Nationality,
                PhoneNumber = g.PhoneNumber,
                Email = g.Email,
                Note = g.Note,
                CreateAt = g.CreateAt,
                UpdateAt = g.UpdateAt
            }).ToListAsync();
            return guests;
        }
        // Get by Id
        public async Task<GuestResponseDto?> GetByIdAsync(int id)
        {
            var guest = await _context.Guests.FindAsync(id);
            if (guest is null) return null;
            return MapToResponseDto(guest);
        }
        // Create 
        public async Task<GuestResponseDto> CreateAsync(GuestCreateDto dto)
        {
            await CheckBookingDetailId(dto.BookingDetailId);
            await CheckGuestCount(dto.BookingDetailId);
            var guest = MapToEntity(dto);
            await _context.Guests.AddAsync(guest);
            await _context.SaveChangesAsync();
            await _auditLogService.LogAsync(AuditLogEnums.Create,
                                            "Guest",
                                            guest.GuestId,
                                            null,
                                            $"BookingDetailId: {guest.BookingDetailId}, " +
                                            $"FullName: {guest.FullName}, " +
                                            $"PhoneNumber: {guest.PhoneNumber}");
            return (await GetByIdAsync(guest.GuestId))!;
        }
        // Update
        public async Task<GuestResponseDto?> UpdateAsync(int id, GuestUpdateDto dto)
        {
            var guest = await _context.Guests.FindAsync(id);
            if (guest is null) return null;
            await CheckBookingDetailId(dto.BookingDetailId);
            // Chỉ cần check khi chuyển khách này sang chi tiết phòng khác
            if (guest.BookingDetailId != dto.BookingDetailId)
                await CheckGuestCount(dto.BookingDetailId);
            var oldValue = $"BookingDetailId: {guest.BookingDetailId}, " +
                           $"FullName: {guest.FullName}, " +
                           $"DateOfBirth: {guest.DateOfBirth}, " +
                           $"Gender: {guest.Gender}, " +
                           $"Nationality: {guest.Nationality}, " +
                           $"PhoneNumber: {guest.PhoneNumber}, " +
                           $"Email: {guest.Email}, " +
                           $"Note: {guest.Note}";
            UpdateEntity(guest, dto);
            await _context.SaveChangesAsync();
            var newValue = $"BookingDetailId: {guest.BookingDetailId}, " +
                           $"FullName: {guest.FullName}, " +
                           $"DateOfBirth: {guest.DateOfBirth}, " +
                           $"Gender: {guest.Gender}, " +
                           $"Nationality: {guest.Nationality}, " +
                           $"PhoneNumber: {guest.PhoneNumber}, " +
                           $"Email: {guest.Email}, " +
                           $"Note: {guest.Note}";
            await _auditLogService.LogAsync(AuditLogEnums.Update,
                                            "Guest",
                                            guest.GuestId,
                                            oldValue,
                                            newValue);
            return await GetByIdAsync(id);
        }
        // Delete
        public async Task<bool> DeleteAsync(int id)
        {
            var guest = await _context.Guests.FindAsync(id);
            if (guest is null) return false;
            _context.Guests.Remove(guest);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
