using HotelManagement.Data;
using HotelManagement.DTOs.Room;
using HotelManagement.Enums;
using HotelManagement.Exceptions;
using HotelManagement.Models;
using Microsoft.EntityFrameworkCore;

namespace HotelManagement.Services
{
    public class RoomService
    {
        private readonly AppDbContext _context;
        private readonly AuditLogService _auditLogService;
        public RoomService (AppDbContext context, AuditLogService auditLogService)
        {
            _context = context;
            _auditLogService = auditLogService;
        }
        // Chuyển Room -> RoomResponseDto
        private RoomResponseDto MapToResponseDto(Room room)
        {
            return new RoomResponseDto
            {
                RoomId = room.RoomId,
                RoomName = room.RoomName,
                RoomTypeId = room.RoomTypeId,
                RoomTypeName = room.RoomType?.RoomTypeName,
                Capacity = room.Capacity,
                Location = room.Location,
                PricePerNight = room.PricePerNight,
                Description = room.Description,
                Status = room.Status,
                IsDeleted = room.IsDeleted,
                CreateAt = room.CreateAt,
                UpdateAt = room.UpdateAt,
                RoomImages = room.RoomImages
                    .OrderByDescending(image => image.IsPrimary)
                    .ThenBy(image => image.DisplayOrder)
                    .Select(image => new HotelManagement.DTOs.RoomImage.RoomImageResponseDto
                    {
                        RoomImageId = image.RoomImageId,
                        RoomId = image.RoomId,
                        RoomName = room.RoomName,
                        ImageUrl = image.ImageUrl,
                        IsPrimary = image.IsPrimary,
                        DisplayOrder = image.DisplayOrder,
                        CreateAt = image.CreateAt,
                        UpdateAt = image.UpdateAt
                    }).ToList()
            };
        }
        // Chuyển RoomCreateDto -> Room
        private Room MapToEntity(RoomCreateDto dto)
        {
            return new Room
            {
                RoomName = dto.RoomName,
                RoomTypeId = dto.RoomTypeId,
                Capacity = dto.Capacity,
                Location = dto.Location,
                PricePerNight = dto.PricePerNight,
                Description = dto.Description
            };
        }
        // Cập nhật Room từ RoomUpdateDto
        private void UpdateEntity(Room room, RoomUpdateDto dto)
        {
            room.RoomName = dto.RoomName;
            room.RoomTypeId = dto.RoomTypeId;
            room.Capacity = dto.Capacity;
            room.Location = dto.Location;
            room.PricePerNight = dto.PricePerNight;
            room.Description = dto.Description;
            room.Status = dto.Status;
        }
        // Check trùng room name
        private async Task RoomNameExists(string roomName, int? roomId = null) 
        {
            var exists = await _context.Rooms.AnyAsync(r => r.RoomName == roomName
                                                   && (roomId == null || roomId != r.RoomId));
            if (exists)
                throw new ConflictException("Tên phòng đã tồn tại!");
        }
        // Check RoomTypeId đã tồn tại chưa?
        private async Task CheckRoomTypeId(int roomTypeId) 
        {
            var exists = await _context.RoomTypes
                                       .AnyAsync(r => r.RoomTypeId == roomTypeId 
                                                   && r.IsDeleted == ActiveEnums.Active);
            if (!exists)
                throw new NotFoundException("Loại phòng không tồn tại hoặc đã ngừng hoạt động!");
        }
        // ===========================================================
        // =                           CRUD                          =
        // ===========================================================
        // Get all Active(Lấy room còn hoạt động)
        public async Task<List<RoomResponseDto>> GetAllAsync()
        {
            var rooms = await _context.Rooms
                .Where(r => r.IsDeleted == ActiveEnums.Active)
                .Include(r => r.RoomType)
                .Include(r => r.RoomImages)
                .OrderBy(r => r.RoomTypeId)
                .ThenBy(r => r.RoomName)
                .ToListAsync();
            return rooms.Select(MapToResponseDto).ToList();
        }
        // Get all InActive(Lấy room không hoạt động)
        public async Task<List<RoomResponseDto>> GetDeletedAsync()
        {
            var rooms = await _context.Rooms
                .Where(r => r.IsDeleted == ActiveEnums.Inactive)
                .Include(r => r.RoomType)
                .Include(r => r.RoomImages)
                .OrderBy(r => r.RoomName)
                .ToListAsync();
            return rooms.Select(MapToResponseDto).ToList();
        }
        // Get by Id
        public async Task<RoomResponseDto?> GetByIdAsync(int id)
        {
            var room = await _context.Rooms.Include(r => r.RoomType).Include(r => r.RoomImages)
                                           .FirstOrDefaultAsync(r => r.RoomId == id
                                                                  && r.IsDeleted == ActiveEnums.Active);
            if (room is null) return null;
            return MapToResponseDto(room);
        }
        // Create 
        public async Task<RoomResponseDto> CreateAsync(RoomCreateDto dto)
        {
            await RoomNameExists(dto.RoomName!);
            await CheckRoomTypeId(dto.RoomTypeId);
            var room = MapToEntity(dto);
            await _context.Rooms.AddAsync(room);
            await _context.SaveChangesAsync();

            return (await GetByIdAsync(room.RoomId))!;
        }
        // Update
        public async Task<RoomResponseDto?> UpdateAsync(int id, RoomUpdateDto dto)
        {
            var room = await _context.Rooms
                                     .FirstOrDefaultAsync(r => r.RoomId == id
                                                            && r.IsDeleted== ActiveEnums.Active);
            if (room is null) return null;
            await RoomNameExists(dto.RoomName!, id);
            await CheckRoomTypeId(dto.RoomTypeId);
            UpdateEntity(room, dto);
            await _context.SaveChangesAsync();
            return await GetByIdAsync(id);
        }
        // Delete
        public async Task<bool> DeleteAsync(int id)
        {
            var room = await _context.Rooms
                                     .FirstOrDefaultAsync(r => r.RoomId == id
                                                            && r.IsDeleted == ActiveEnums.Inactive);
            if (room is null) return false;
            // Không cho xóa nếu room vẫn còn trong Review
            var review = await _context.Reviews.AnyAsync(r => r.RoomId == id);
            if (review)
                throw new BadRequestException("Phòng đã có đánh giá. Không thể xóa!");
            // Không cho xóa nếu room vẫn còn trong BookingDetail
            var bookingDetail = await _context.BookingDetails
                                              .AnyAsync(bd => bd.RoomId == id);
            if (bookingDetail)
                throw new BadRequestException("Phòng đã có lịch sử đặt phòng. Không thể xóa!");
            // Favorite tự xóa theo Room
            // Image tự xóa theo Room
            _context.Rooms.Remove(room);
            await _context.SaveChangesAsync();
            return true;
        }
        // Xóa tạm
        public async Task<RoomResponseDto?> SoftDeleteAsync(int id)
        {
            var room = await _context.Rooms
                                     .FirstOrDefaultAsync(r => r.RoomId == id
                                                            && r.IsDeleted == ActiveEnums.Active);
            if (room is null) return null;
            // Không cho xóa nếu room vẫn còn trong BookingDetail
            var bookingDetail = await _context.BookingDetails
                                              .AnyAsync(bd => bd.RoomId == id);
            if (bookingDetail)
                throw new BadRequestException("Phòng đã có lịch sử đặt phòng. Không thể xóa!");
            room.IsDeleted = ActiveEnums.Inactive;
            await _context.SaveChangesAsync();
            return MapToResponseDto(room);
        }
        // Khôi phục (Restore)
        public async Task<RoomResponseDto?> RestoreAsync(int id)
        {
            var room = await _context.Rooms
                                     .FirstOrDefaultAsync(r => r.RoomId == id
                                                            && r.IsDeleted == ActiveEnums.Inactive);
            if (room is null) return null;
            var roomTypeExists = await _context.RoomTypes
                                       .AnyAsync(rt => rt.RoomTypeId == room.RoomTypeId
                                                    && rt.IsDeleted == ActiveEnums.Active);

            if (!roomTypeExists)
                throw new BadRequestException("Loại phòng đã ngừng hoạt động. Không thể khôi phục phòng!");
            room.IsDeleted = ActiveEnums.Active;
            await _context.SaveChangesAsync();
            return await GetByIdAsync(id);
        }
        // Lọc danh sách phòng theo loại/ giá/ sức chứa
        public async Task<List<RoomResponseDto>> FilterAsync(RoomFilterDto filter)
        {
            // Validate
            if (filter.MinPrice.HasValue && filter.MaxPrice.HasValue && filter.MinPrice.Value > filter.MaxPrice.Value)
                throw new BadRequestException("Giá tối thiểu không được lớn hơn giá tối đa!");
            if (filter.MinCapacity.HasValue && filter.MaxCapacity.HasValue && filter.MinCapacity.Value > filter.MaxCapacity.Value)
                throw new BadRequestException("Sức chứa tối thiểu không được lớn hơn sức chứa tối đa!");
            if (filter.MinPrice.HasValue && filter.MinPrice.Value < 0)
                throw new BadRequestException("Giá tối thiểu không được nhỏ hơn 0!");
            if (filter.MaxPrice.HasValue && filter.MaxPrice.Value < 0)
                throw new BadRequestException("Giá tối đa không được nhỏ hơn 0!");
            if (filter.MinCapacity.HasValue && filter.MinCapacity.Value < 0)
                throw new BadRequestException("Sức chứa tối thiểu không được nhỏ hơn 0!");
            if (filter.MaxCapacity.HasValue && filter.MaxCapacity.Value < 0)
                throw new BadRequestException("Sức chứa tối đa không được nhỏ hơn 0!");
            // Filter
            var query = _context.Rooms
                                .Where(r => r.IsDeleted == ActiveEnums.Active)
                                .AsQueryable();
            // Lọc theo loại phòng
            if (filter.RoomTypeId.HasValue)
                query = query.Where(r => r.RoomTypeId == filter.RoomTypeId.Value);
            // Lọc giá tối thiểu
            if (filter.MinPrice.HasValue)
                query = query.Where(r => r.PricePerNight >= filter.MinPrice.Value);
            // Lọc giá tối đa
            if (filter.MaxPrice.HasValue)
                query = query.Where(r => r.PricePerNight <= filter.MaxPrice.Value);
            // Lọc sức chứa tối thiểu
            if (filter.MinCapacity.HasValue)
                query = query.Where(r => r.Capacity >= filter.MinCapacity.Value);
            // Lọc sức chứa tối đa
            if (filter.MaxCapacity.HasValue)
                query = query.Where(r => r.Capacity <= filter.MaxCapacity.Value);
            var rooms = await query
                .Include(r => r.RoomType)
                .Include(r => r.RoomImages)
                .OrderBy(r => r.PricePerNight)
                .ThenBy(r => r.RoomName)
                .ToListAsync();
            return rooms.Select(MapToResponseDto).ToList();
        }
    }
}
