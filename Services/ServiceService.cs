using HotelManagement.Data;
using HotelManagement.DTOs.Service;
using HotelManagement.Enums;
using HotelManagement.Exceptions;
using HotelManagement.Models;
using Microsoft.EntityFrameworkCore;

namespace HotelManagement.Services
{
    public class ServiceService
    {
        private readonly AppDbContext _context;
        private readonly AuditLogService _auditLogService;
        public ServiceService(AppDbContext context, AuditLogService auditLogService)
        {
            _context = context;
            _auditLogService = auditLogService;
        }
        // Chuyển Service -> Response Dto
        private ServiceResponseDto MapToResponseDto(Service service)
        {
            return new ServiceResponseDto
            {
                ServiceId = service.ServiceId,
                ServiceName = service.ServiceName,
                UnitPrice = service.UnitPrice,
                Unit = service.Unit,
                Description = service.Description,
                IsDeleted = service.IsDeleted,
                CreateAt = service.CreateAt,
                UpdateAt = service.UpdateAt
            };
        }
        // Chuyển CreateDto -> Service
        private Service MapToEntity(ServiceCreateDto dto)
        {
            return new Service
            {
                ServiceName = dto.ServiceName,
                UnitPrice = dto.UnitPrice,
                Unit = dto.Unit,
                Description = dto.Description
            };
        }
        // Update Service từ ServiceUpdateDto
        private void UpdateEntity(Service service, ServiceUpdateDto dto)
        {
            service.ServiceName = dto.ServiceName;
            service.UnitPrice = dto.UnitPrice;
            service.Unit = dto.Unit;
            service.Description = dto.Description;
        }
        // Check trùng Service (Có thể trùng tên nhưng giá sẽ khác nhau)
        private async Task ServiceExists(string serviceName, decimal unitPrice, string unit, string description, int? serviceId = null)
        {
            var exists = await _context.Services
                                       .AnyAsync(s => s.ServiceName == serviceName
                                                   && s.UnitPrice == unitPrice
                                                   && s.Unit == unit
                                                   && s.Description == description
                                                   && (serviceId == null || serviceId != s.ServiceId));
            if (exists)
                throw new ConflictException("Dịch vụ đã tồn tại!");

        }
        // ===========================================================
        // =                           CRUD                          =
        // ===========================================================
        // Get all Active(Lấy service còn hoạt động)
        public async Task<List<ServiceResponseDto>> GetAllAsync()
        {
            var services = await _context.Services.Where(s => s.IsDeleted == ActiveEnums.Active)
                                            .OrderBy(s => s.ServiceName)
                                            .Select(s => new ServiceResponseDto
                                            {
                                                ServiceId = s.ServiceId,
                                                ServiceName = s.ServiceName,
                                                UnitPrice = s.UnitPrice,
                                                Unit = s.Unit,
                                                Description = s.Description,
                                                IsDeleted = s.IsDeleted,
                                                CreateAt = s.CreateAt,
                                                UpdateAt = s.UpdateAt
                                            }).ToListAsync();
            return services;
        }
        // Get all InActive(Lấy service không hoạt động)
        public async Task<List<ServiceResponseDto>> GetDeletedAsync()
        {
            var services = await _context.Services.Where(s => s.IsDeleted == ActiveEnums.Inactive)
                                            .OrderBy(s => s.ServiceName)
                                            .Select(s => new ServiceResponseDto
                                            {
                                                ServiceId = s.ServiceId,
                                                ServiceName = s.ServiceName,
                                                UnitPrice = s.UnitPrice,
                                                Unit = s.Unit,
                                                Description = s.Description,
                                                IsDeleted = s.IsDeleted,
                                                CreateAt = s.CreateAt,
                                                UpdateAt = s.UpdateAt
                                            }).ToListAsync();
            return services;
        }
        // Get by Id
        public async Task<ServiceResponseDto?> GetByIdAsync(int id)
        {
            var service = await _context.Services
                                        .FirstOrDefaultAsync(s => s.ServiceId == id
                                                               && s.IsDeleted == ActiveEnums.Active);
            if (service is null) return null;
            return MapToResponseDto(service);
        }
        // Create 
        public async Task<ServiceResponseDto> CreateAsync(ServiceCreateDto dto)
        {
            await ServiceExists(dto.ServiceName!, dto.UnitPrice, dto.Unit!, dto.Description!);
            var service = MapToEntity(dto);
            await _context.Services.AddAsync(service);
            await _context.SaveChangesAsync();
            await _auditLogService.LogAsync(AuditLogEnums.Create,
                                            "Services",
                                            service.ServiceId,
                                            null,
                                            $"ServiceName: {service.ServiceName}, UnitPrice: {service.UnitPrice}");
            return MapToResponseDto(service);
        }
        // Update
        public async Task<ServiceResponseDto?> UpdateAsync(int id, ServiceUpdateDto dto)
        {
            var service = await _context.Services
                                        .FirstOrDefaultAsync(s => s.ServiceId == id
                                                               && s.IsDeleted == ActiveEnums.Active);
            if (service is null) return null;
            await ServiceExists(dto.ServiceName!, dto.UnitPrice, dto.Unit!, dto.Description!, id);
            var oldValue = $"ServiceName: {service.ServiceName}, " +
                           $"UnitPrice: {service.UnitPrice}, " +
                           $"Unit: {service.Unit}, " +
                           $"Description: {service.Description}";
            UpdateEntity(service, dto);
            await _context.SaveChangesAsync();
            await _auditLogService.LogAsync(AuditLogEnums.Update,
                                            "Services",
                                            service.ServiceId,
                                            oldValue,
                                            $"ServiceName: {service.ServiceName}, " +
                                            $"UnitPrice: {service.UnitPrice}, " +
                                            $"Unit: {service.Unit}, " +
                                            $"Description: {service.Description}");
            return MapToResponseDto(service);
        }
        // Delete
        public async Task<bool> DeleteAsync(int id)
        {
            var service = await _context.Services
                                        .FirstOrDefaultAsync(s => s.ServiceId == id
                                                               && s.IsDeleted == ActiveEnums.Inactive);
            if (service is null) return false;
            // Không cho xóa nếu Service còn trong BookingService
            var bookingService = await _context.BookingServices
                                               .AnyAsync(bs => bs.ServiceId == id);
            if (bookingService)
                throw new BadRequestException("Dịch vụ đã được sử dụng trong đặt dịch vụ. Không thể xóa!");
            _context.Services.Remove(service);
            await _context.SaveChangesAsync();
            return true;
        }
        // Xóa tạm
        public async Task<ServiceResponseDto?> SoftDeleteAsync(int id)
        {
            var service = await _context.Services
                                        .FirstOrDefaultAsync(s => s.ServiceId == id
                                                               && s.IsDeleted == ActiveEnums.Active);
            if (service is null) return null;
            service.IsDeleted = ActiveEnums.Inactive;
            await _context.SaveChangesAsync();
            await _auditLogService.LogAsync(AuditLogEnums.SoftDelete,
                                            "Services",
                                            service.ServiceId,
                                            "IsDeleted: Active",
                                            "IsDeleted: Inactive");
            return MapToResponseDto(service);
        }
        // Khôi phục (Restore)
        public async Task<ServiceResponseDto?> RestoreAsync(int id)
        {
            var service = await _context.Services
                                        .FirstOrDefaultAsync(s => s.ServiceId == id
                                                               && s.IsDeleted == ActiveEnums.Inactive);
            if (service is null) return null;
            service.IsDeleted = ActiveEnums.Active;
            await _context.SaveChangesAsync();
            await _auditLogService.LogAsync(AuditLogEnums.Restore,
                                            "Services",
                                            service.ServiceId,
                                            "IsDeleted: Inactive",
                                            "IsDeleted: Active");
            return MapToResponseDto(service);
        }
        // Tìm kiếm và lọc
        // Tìm kiếm và lọc Service
        public async Task<List<ServiceResponseDto>> SearchAsync(ServiceSearchDto request)
        {
            // Kiểm tra khoảng giá
            if (request.MinPrice.HasValue && request.MaxPrice.HasValue && request.MinPrice > request.MaxPrice)
                throw new BadRequestException("Giá thấp nhất không được lớn hơn giá cao nhất!");
            // Giá không được âm
            if (request.MinPrice.HasValue && request.MinPrice < 0)
                throw new BadRequestException("Giá thấp nhất không được nhỏ hơn 0!");
            if (request.MaxPrice.HasValue && request.MaxPrice < 0)
                throw new BadRequestException("Giá cao nhất không được nhỏ hơn 0!");
            var query = _context.Services
                                .Where(s => s.IsDeleted == ActiveEnums.Active)
                                .AsQueryable();
            // Tìm theo tên
            if (!string.IsNullOrWhiteSpace(request.Keyword))
            {
                var keyword = request.Keyword.Trim();
                query = query.Where(s => s.ServiceName != null 
                                      && s.ServiceName.Contains(keyword));
            }

            // Lọc giá từ
            if (request.MinPrice.HasValue)
                query = query.Where(s => s.UnitPrice >= request.MinPrice.Value);
            // Lọc giá đến
            if (request.MaxPrice.HasValue)
                query = query.Where(s => s.UnitPrice <= request.MaxPrice.Value);
            var services = await query.OrderBy(s => s.ServiceName)
                                      .Select(s => new ServiceResponseDto
                                      {
                                          ServiceId = s.ServiceId,
                                          ServiceName = s.ServiceName,
                                          UnitPrice = s.UnitPrice,
                                          Unit = s.Unit,
                                          Description = s.Description,
                                          IsDeleted = s.IsDeleted,
                                          CreateAt = s.CreateAt,
                                          UpdateAt = s.UpdateAt
                                      })
                                      .ToListAsync();
            return services;
        }
    }
}
