using HotelManagement.Data;
using HotelManagement.DTOs.Customer;
using HotelManagement.Enums;
using HotelManagement.Exceptions;
using HotelManagement.Models;
using Microsoft.EntityFrameworkCore;

namespace HotelManagement.Services
{
    public class CustomerService
    {
        private readonly AppDbContext _context;
        private readonly AuditLogService _auditLogService;
        public CustomerService(AppDbContext context, AuditLogService auditLogService)
        {
            _context = context;
            _auditLogService = auditLogService;
        }
        // Chuyển Customer -> ResponseDto
        private CustomerResponseDto MapToResponseDto(Customer customer)
        {
            return new CustomerResponseDto
            {
                CustomerId = customer.CustomerId,
                UserId = customer.UserId,
                CCCD = customer.CCCD,
                Nationality = customer.Nationality,
                FullName = customer.User?.FullName,
                Email = customer.User?.Email,
                Gender = customer.User?.Gender,
                DateOfBirth = customer.User?.DateOfBirth,
                PhoneNumber = customer.User?.PhoneNumber,
                Avatar = customer.User?.Avatar,
                CreateAt = customer.CreateAt,
                UpdateAt = customer.UpdateAt
            };
        }
        // ===========================================================
        // =                           CRUD                          =
        // ===========================================================
        // Get all
        public async Task<List<CustomerResponseDto>> GetAllAsync() 
        {
            var customers = await _context.Customers
                                          .Where(c => c.User != null
                                                   && c.User.IsDeleted == ActiveEnums.Active)
                                           .OrderBy(c => c.User!.FullName)
                                           .Select(c => new CustomerResponseDto
                                           {
                                                CustomerId = c.CustomerId,
                                                UserId = c.UserId,
                                                CCCD = c.CCCD,
                                                Nationality = c.Nationality,
                                                FullName = c.User!.FullName,
                                                Email = c.User.Email,
                                                Gender = c.User.Gender,
                                                DateOfBirth = c.User.DateOfBirth,
                                                PhoneNumber = c.User.PhoneNumber,
                                                Avatar = c.User.Avatar,
                                                CreateAt = c.CreateAt,
                                                UpdateAt = c.UpdateAt
                                           }).ToListAsync();
            return customers;
        }
        // Get by Id
        public async Task<CustomerResponseDto?> GetByIdAsync(int id)
        {
            var customer = await _context.Customers
                                         .Include(c => c.User)
                                            .ThenInclude(u => u!.Role)
                                         .FirstOrDefaultAsync(c => c.CustomerId == id 
                                                                && c.User != null 
                                                                && c.User.IsDeleted == ActiveEnums.Active);
            if (customer == null) return null;
            return MapToResponseDto(customer);
        }
        // Delete
        public async Task<bool> DeleteAsync(int id)
        {
            var customer = await _context.Customers
                                         .Include(c => c.User)
                                            .ThenInclude(u => u!.Role)
                                         .FirstOrDefaultAsync(c => c.CustomerId == id);
            if (customer == null) return false;
            // Không cho xóa Customer nếu đã có booking
            var booking = await _context.Bookings.AnyAsync(c => c.CustomerId == id);
            if (booking)
                throw new BadRequestException("Khách hàng đã đặt phòng. Không thể xóa!");
            // Không cho xóa Customer nếu đã có review
            var review = await _context.Reviews.AnyAsync(c => c.CustomerId == id);
            if (review)
                throw new BadRequestException("Khách hàng đã đánh giá phòng. Không thể xóa!");
            // Nếu User vẫn đang là Customer thì không được xóa Customer riêng
            if (customer.User?.Role?.RoleName == "Customer" && customer.User.IsDeleted == ActiveEnums.Active)
                throw new BadRequestException("Không thể xóa thông tin khách hàng của User đang có vai trò Customer!");
            // FavoriteRoom đã tự động xóa theo Customer do Cascade
            _context.Customers.Remove(customer);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
