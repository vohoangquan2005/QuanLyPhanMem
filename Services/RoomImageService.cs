using HotelManagement.Data;
using HotelManagement.DTOs.RoomImage;
using HotelManagement.Models;
using Microsoft.EntityFrameworkCore;
using HotelManagement.Exceptions;

namespace HotelManagement.Services
{
    public class RoomImageService
    {
        private readonly AppDbContext _context;
        private readonly CloudinaryService _cloudinaryService;
        private readonly ILogger<RoomImageService> _logger;

        public RoomImageService(
            AppDbContext context,
            CloudinaryService cloudinaryService,
            ILogger<RoomImageService> logger)
        {
            _context = context;
            _cloudinaryService = cloudinaryService;
            _logger = logger;
        }

        private async Task TryDeleteImageAsync(string? imageUrl)
        {
            if (string.IsNullOrWhiteSpace(imageUrl))
            {
                return;
            }

            try
            {
                await _cloudinaryService.DeleteImageAsync(imageUrl);
            }
            catch (Exception exception)
            {
                _logger.LogError(
                    exception,
                    "A room image asset could not be deleted from Cloudinary.");
            }
        }
        // Chuyển RoomImage -> ResponseDto
        private RoomImageResponseDto MapToResponseDto(RoomImage roomImage)
        {
            return new RoomImageResponseDto
            {
                RoomImageId = roomImage.RoomImageId,
                RoomId = roomImage.RoomId,
                RoomName = roomImage.Room?.RoomName,
                ImageUrl = roomImage.ImageUrl,
                IsPrimary = roomImage.IsPrimary,
                DisplayOrder = roomImage.DisplayOrder,
                CreateAt = roomImage.CreateAt,
                UpdateAt = roomImage.UpdateAt
            };
        }
        // Chuyển CreateDto -> RoomImage
        private RoomImage MapToEntity(RoomImageCreateDto dto, string imageUrl)
        {
            return new RoomImage
            {
                RoomId = dto.RoomId,
                ImageUrl = imageUrl,
                IsPrimary = dto.IsPrimary,
                DisplayOrder = dto.DisplayOrder
            };
        }
        // Update RoomImage từ UpdateDto
        private void UpdateEntity(RoomImage roomImage, RoomImageUpdateDto dto, string? imageUrl = null)
        {
            // Chỉ thay ImageUrl nếu có ảnh mới
            if (!string.IsNullOrEmpty(imageUrl))
                roomImage.ImageUrl = imageUrl;
            roomImage.IsPrimary = dto.IsPrimary;
            roomImage.DisplayOrder = dto.DisplayOrder;
        }
        // Check RoomId có tồn tại trong bảng Room không?
        private async Task CheckRoomId(int  roomId)
        {
            var exists = await _context.Rooms.AnyAsync(r => r.RoomId == roomId);
            if (!exists)
                throw new NotFoundException("Mã phòng không hợp lệ!");
        }
        // Cập nhật IsPrimary
        private async Task SetPrimaryImage(int roomId, int? roomImageId = null)
        {
            var primaryImage = await _context.RoomImages
                                             .FirstOrDefaultAsync(ri => ri.RoomId == roomId 
                                                               && ri.IsPrimary == 1
                                                               && (roomImageId == null || ri.RoomImageId != roomImageId));
            if (primaryImage is not null)
                primaryImage.IsPrimary = 0;
        }
        // ===========================================================
        // =                           CRUD                          =
        // ===========================================================
        // Get all 
        public async Task<List<RoomImageResponseDto>> GetAllAsync()
        {
            var roomImages = await _context.RoomImages
                                           .OrderBy(i => i.RoomId)
                                                .ThenBy(i => i.DisplayOrder)
                                           .Select(ri => new RoomImageResponseDto
                                           {
                                                RoomImageId = ri.RoomImageId,
                                                RoomId = ri.RoomId,
                                                RoomName = ri.Room!.RoomName,
                                                ImageUrl = ri.ImageUrl,
                                                IsPrimary = ri.IsPrimary,
                                                DisplayOrder = ri.DisplayOrder,
                                                CreateAt = ri.CreateAt,
                                                UpdateAt = ri.UpdateAt
                                           })
                                           .ToListAsync();
            return roomImages;
        }

        public async Task<List<RoomImageResponseDto>> GetPublicAsync()
        {
            return await _context.RoomImages
                .Where(image => image.Room != null && image.Room.IsDeleted == HotelManagement.Enums.ActiveEnums.Active)
                .OrderBy(image => image.RoomId)
                .ThenByDescending(image => image.IsPrimary)
                .ThenBy(image => image.DisplayOrder)
                .Select(image => new RoomImageResponseDto
                {
                    RoomImageId = image.RoomImageId,
                    RoomId = image.RoomId,
                    RoomName = image.Room!.RoomName,
                    ImageUrl = image.ImageUrl,
                    IsPrimary = image.IsPrimary,
                    DisplayOrder = image.DisplayOrder,
                    CreateAt = image.CreateAt,
                    UpdateAt = image.UpdateAt
                })
                .ToListAsync();
        }

        // Get by Id
        public async Task<RoomImageResponseDto?> GetByIdAsync(int id)
        {
            var roomImage = await _context.RoomImages
                                          .Include(ri => ri.Room)
                                          .FirstOrDefaultAsync(ri => ri.RoomImageId == id);
            if (roomImage is null) return null;
            return MapToResponseDto(roomImage);
        }
        // Create
        public async Task<RoomImageResponseDto> CreateAsync(RoomImageCreateDto dto)
        {
            await CheckRoomId(dto.RoomId);
            // Nếu tạo ảnh chỉnh -> bỏ ảnh chính cũ
            if (dto.IsPrimary == 1)
                await SetPrimaryImage(dto.RoomId);
            // Upload ảnh lên Cloud
            var imageUrl = await _cloudinaryService.UploadImageAsync(dto.Image);
            var roomImage = MapToEntity(dto, imageUrl);
            try
            {
                await _context.RoomImages.AddAsync(roomImage);
                await _context.SaveChangesAsync();
            }
            catch
            {
                await TryDeleteImageAsync(imageUrl);
                throw;
            }
            return (await GetByIdAsync(roomImage.RoomImageId))!;
        }
        // Update
        public async Task<RoomImageResponseDto?> UpdateAsync(int id, RoomImageUpdateDto dto)
        {
            var roomImage = await _context.RoomImages.FindAsync(id);
            if (roomImage is null) return null;
            // Lưu URL ảnh cũ
            var oldImageUrl = roomImage.ImageUrl;
            if (dto.IsPrimary == 1)
                await SetPrimaryImage(roomImage.RoomId, id);
            // URL ảnh mới
            string? newImageUrl = null;
            if (dto.Image != null && dto.Image.Length > 0)
                newImageUrl = await _cloudinaryService.UploadImageAsync(dto.Image);
            UpdateEntity(roomImage, dto, newImageUrl);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch
            {
                await TryDeleteImageAsync(newImageUrl);
                throw;
            }
            // Xóa ảnh cũ trên Cloud
            if (!string.IsNullOrWhiteSpace(newImageUrl) && !string.IsNullOrWhiteSpace(oldImageUrl))
                await TryDeleteImageAsync(oldImageUrl);
            return await GetByIdAsync(id);
        }
        // Delete
        public async Task<bool> DeleteAsync(int id)
        {
            var roomImage = await _context.RoomImages.FindAsync(id);
            if (roomImage is null) return false;
            // Lưu Url cũ
            var imageUrl = roomImage.ImageUrl;
            _context.RoomImages.Remove(roomImage);
            await _context.SaveChangesAsync();
            // 4. Xóa ảnh trên Cloudinary
            await TryDeleteImageAsync(imageUrl);
            return true;
        }
    }
}
