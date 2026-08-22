using HotelManagement.Data;
using HotelManagement.DTOs.RoomType;
using HotelManagement.Enums;
using HotelManagement.Exceptions;
using HotelManagement.Models;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace HotelManagement.Services
{
    public class RoomTypeService
    {
        private readonly AppDbContext _context;
        private readonly AuditLogService _auditLogService;
        public RoomTypeService(AppDbContext context, AuditLogService auditLogService)
        {
            _context = context;
            _auditLogService = auditLogService;
        }
        // Chuyển Room Type -> RooTypeResponseDto
        private RoomTypeResponseDto MapToResponseDto(RoomType roomType)
        {
            return new RoomTypeResponseDto
            {
                RoomTypeId = roomType.RoomTypeId,
                RoomTypeName = roomType.RoomTypeName,
                BedCount = roomType.BedCount,
                BedType = roomType.BedType,
                Size = roomType.Size,
                Description = roomType.Description,
                IsDeleted = roomType.IsDeleted,
                CreateAt = roomType.CreateAt,
                UpdateAt = roomType.UpdateAt
            };
        }
        // Chuyển RoomTypeCreateDto -> RoomType
        private RoomType MapToEntity(RoomTypeCreateDto dto)
        {
            return new RoomType
            {
                RoomTypeName = dto.RoomTypeName,
                BedCount = dto.BedCount,
                BedType = dto.BedType,
                Size = dto.Size,
                Description = dto.Description
            };
        }
        // Cập nhật RoomTyp từ RoomTypeUpdateDto
        private void UpdateEntity(RoomType roomType, RoomTypeUpdateDto dto)
        {
            roomType.RoomTypeName = dto.RoomTypeName;
            roomType.BedCount = dto.BedCount;
            roomType.BedType = dto.BedType;
            roomType.Size = dto.Size;
            roomType.Description = dto.Description;
        }
        // Check trùng tên loại phòng
        private async Task RoomTypeNameExists(string roomTypeName, int? roomTypeId = null)
        {
            var exists = await _context.RoomTypes.AnyAsync(rt => rt.RoomTypeName == roomTypeName
                                                        && (roomTypeId == null || rt.RoomTypeId != roomTypeId));
            if (exists)
                throw new ConflictException("Tên loại phòng đã tồn tại!");
        }
        // ===========================================================
        // =                           CRUD                          =
        // ===========================================================
        // Get all Active(Lấy phòng còn hoạt động)
        public async Task<List<RoomTypeResponseDto>> GetAllAsync()
        {
            var roomTypes = await _context.RoomTypes.Where(rt => rt.IsDeleted == ActiveEnums.Active)
                                                    .OrderBy(rt => rt.RoomTypeName)
                                                    .Select(rt => new RoomTypeResponseDto
                                                    {
                                                        RoomTypeId = rt.RoomTypeId,
                                                        RoomTypeName = rt.RoomTypeName,
                                                        BedCount = rt.BedCount,
                                                        BedType = rt.BedType,
                                                        Size = rt.Size,
                                                        Description = rt.Description,
                                                        IsDeleted = rt.IsDeleted,
                                                        CreateAt = rt.CreateAt,
                                                        UpdateAt = rt.UpdateAt
                                                    })
                                                    .ToListAsync();
            return roomTypes;
        }
        // Get all InActive(Lấy phòng không hoạt động)
        public async Task<List<RoomTypeResponseDto>> GetDeletedAsync()
        {
            var roomTypes = await _context.RoomTypes.Where(rt => rt.IsDeleted == ActiveEnums.Inactive)
                                                    .OrderBy(rt => rt.RoomTypeName)
                                                    .Select(rt => new RoomTypeResponseDto
                                                    {
                                                        RoomTypeId = rt.RoomTypeId,
                                                        RoomTypeName = rt.RoomTypeName,
                                                        BedCount = rt.BedCount,
                                                        BedType = rt.BedType,
                                                        Size = rt.Size,
                                                        Description = rt.Description,
                                                        IsDeleted = rt.IsDeleted,
                                                        CreateAt = rt.CreateAt,
                                                        UpdateAt = rt.UpdateAt
                                                    })
                                                    .ToListAsync();
            return roomTypes;
        }
        // Get by Id
        public async Task<RoomTypeResponseDto?> GetByIdAsync(int id)
        {
            var roomType = await _context.RoomTypes
                                         .FirstOrDefaultAsync(rt => rt.RoomTypeId == id
                                                        && rt.IsDeleted == ActiveEnums.Active);
            if (roomType is null) return null;
            return MapToResponseDto(roomType);
        }
        // Create
        public async Task<RoomTypeResponseDto> CreateAsync(RoomTypeCreateDto dto)
        {
            await RoomTypeNameExists(dto.RoomTypeName!);
            var roomType = MapToEntity(dto);
            await _context.RoomTypes.AddAsync(roomType);
            await _context.SaveChangesAsync();
            await _auditLogService.LogAsync(AuditLogEnums.Create,
                                            "RoomType",
                                            roomType.RoomTypeId,
                                            null,
                                            JsonSerializer.Serialize(roomType));
            return MapToResponseDto(roomType);
        }
        // Update 
        public async Task<RoomTypeResponseDto?> UpdateAsync(int id, RoomTypeUpdateDto dto)
        {
            var roomType = await _context.RoomTypes
                                         .FirstOrDefaultAsync(rt => rt.RoomTypeId == id
                                                                  && rt.IsDeleted == ActiveEnums.Active);
            if (roomType is null) return null;
            await RoomTypeNameExists(dto.RoomTypeName!, id);
            var oldValue = $"RoomTypeName: {roomType.RoomTypeName}, " +
                           $"BedCount: {roomType.BedCount}, " +
                           $"BedType: {roomType.BedType}, " +
                           $"Size: {roomType.Size}, " +
                           $"Description: {roomType.Description}";
            UpdateEntity(roomType, dto);
            await _context.SaveChangesAsync();
            await _auditLogService.LogAsync(AuditLogEnums.Update,
                                            "RoomTypes",
                                            roomType.RoomTypeId,
                                            oldValue,
                                            $"RoomTypeName: {roomType.RoomTypeName}, " +
                                            $"BedCount: {roomType.BedCount}, " +
                                            $"BedType: {roomType.BedType}, " +
                                            $"Size: {roomType.Size}, " +
                                            $"Description: {roomType.Description}");
            return MapToResponseDto(roomType);
        }
        // Delete
        public async Task<bool> DeleteAsync(int id)
        {
            var roomType = await _context.RoomTypes
                                         .FirstOrDefaultAsync(rt => rt.RoomTypeId == id
                                                                 && rt.IsDeleted == ActiveEnums.Inactive);
            if (roomType is null) return false;
            // Không cho xóa nếu loại phòng còn trong Room
            var room = await _context.Rooms
                                     .AnyAsync(r => r.RoomTypeId == id
                                                 && r.IsDeleted == ActiveEnums.Active);
            if (room)
                throw new BadRequestException("Loại phòng còn phòng thuộc loại này. Không thể xóa!");
            // Không cho xóa nếu loại phòng còn trong ApplyPromotion 
            var applyPromotion = _context.ApplyPromotions
                                         .Any(a => a.RoomTypeId == id);
            if (applyPromotion)
                throw new BadRequestException("Loai phòng vẫn còn khuyến mãi áp dụng. Không thể xóa!");
            _context.RoomTypes.Remove(roomType);
            await _context.SaveChangesAsync();
            return true;
        }
        // Xóa tạm
        public async Task<RoomTypeResponseDto?> SoftDeleteAsync(int id)
        {
            var roomType = await _context.RoomTypes
                                         .FirstOrDefaultAsync(r => r.RoomTypeId == id &&
                                                                   r.IsDeleted == ActiveEnums.Active);
            if (roomType is null) return null;
            // Không cho xóa nếu loại phòng còn trong Room
            var room = await _context.Rooms
                                     .AnyAsync(r => r.RoomTypeId == id
                                                 && r.IsDeleted == ActiveEnums.Active);
            if (room)
                throw new BadRequestException("Loại phòng còn phòng thuộc loại này. Không thể xóa!");
            // Không cho xóa nếu loại phòng còn trong ApplyPromotion 
            var applyPromotion = _context.ApplyPromotions
                                         .Any(a => a.RoomTypeId == id);
            if (applyPromotion)
                throw new BadRequestException("Loai phòng vẫn còn khuyến mãi áp dụng. Không thể xóa!");
            roomType.IsDeleted = ActiveEnums.Inactive;
            await _context.SaveChangesAsync();
            await _auditLogService.LogAsync(AuditLogEnums.SoftDelete,
                                            "RoomTypes",
                                            roomType.RoomTypeId,
                                            "IsDeleted: Active",
                                            "IsDeleted: Inactive");
            return MapToResponseDto(roomType);
        }
        // Khôi phục
        public async Task<RoomTypeResponseDto?> RestoreAsync(int id)
        {
            var roomType = await _context.RoomTypes
                                         .FirstOrDefaultAsync(rt => rt.RoomTypeId == id
                                                                 && rt.IsDeleted == ActiveEnums.Inactive);
            if (roomType is null) return null;
            roomType.IsDeleted = ActiveEnums.Active;
            await _context.SaveChangesAsync();
            await _auditLogService.LogAsync(AuditLogEnums.Restore,
                                            "RoomTypes",
                                            roomType.RoomTypeId,
                                            "IsDeleted: Inactive",
                                            "IsDeleted: Active");
            return MapToResponseDto(roomType);
        }
    }
}
