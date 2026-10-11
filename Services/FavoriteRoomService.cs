using HotelManagement.Data;
using HotelManagement.DTOs.FavoriteRoom;
using HotelManagement.Exceptions;
using HotelManagement.Models;
using Microsoft.EntityFrameworkCore;

namespace HotelManagement.Services
{
    public class FavoriteRoomService
    {
        private readonly AppDbContext _context;
        public FavoriteRoomService(AppDbContext context)
        {
            _context = context;
        }
        // Chuyển FavoriteRoom -> ResponseDto
        private FavoriteRoomResponseDto MapToResponseDto(FavoriteRoom favoriteRoom)
        {
            return new FavoriteRoomResponseDto
            {
                FavoriteRoomId = favoriteRoom.FavoriteRoomId,
                CustomerId = favoriteRoom.CustomerId,
                CustomerName = favoriteRoom.Customer?.User?.FullName,
                RoomId = favoriteRoom.RoomId,
                RoomName = favoriteRoom.Room?.RoomName,
                CreateAt = favoriteRoom.CreateAt
            };
        }
        // Chuyển CreateDto -> FavoriteRoom
        private FavoriteRoom MapToEntity(FavoriteRoomCreateDto dto, int customerId)
        {
            return new FavoriteRoom
            {
                CustomerId = customerId,
                RoomId = dto.RoomId
            };
        }
        // Check CustomerId có tồn tại trong bảng Customer không?
        private async Task CheckCustomerId(int customerId)
        {
            var exists = await _context.Customers
                .AnyAsync(c => c.CustomerId == customerId);

            if (!exists)
                throw new NotFoundException("Mã khách hàng không hợp lệ!");
        }
        // Check RoomId có tồn tại trong bảng Room không?
        private async Task CheckRoomId(int roomId)
        {
            var exists = await _context.Rooms.AnyAsync(r => r.RoomId == roomId);
            if (!exists)
                throw new NotFoundException("Mã phòng không hợp lệ!");
        }
        // Check CustomerId đã đưa RoomId và FavoriteRoom chưa? 
        private async Task CheckFavoriteExists(int customerId, int roomId)
        {
            var exists = await _context.FavoriteRooms.AnyAsync(fr  => fr.CustomerId == customerId
                                                                   && fr.RoomId == roomId);
            if (exists)
                throw new ConflictException("Khách hàng đã yêu thích phòng này!");
        }
        // ===========================================================
        // =                           CRUD                          =
        // ===========================================================
        // Get all
        public async Task<List<FavoriteRoomResponseDto>> GetMyFavoritesAsync(int customerId)
        {
            var favoriteRooms = await _context.FavoriteRooms
                                              .Where(fr => fr.CustomerId == customerId)
                                              .Include(fr => fr.Customer)
                                                  .ThenInclude(c => c!.User)
                                              .Include(fr => fr.Room)
                                              .Select(fr => new FavoriteRoomResponseDto
                                              {
                                                    FavoriteRoomId = fr.FavoriteRoomId,
                                                    CustomerId = fr.CustomerId,
                                                    CustomerName = fr.Customer!.User!.FullName,
                                                    RoomId = fr.RoomId,
                                                    RoomName = fr.Room!.RoomName,
                                                    CreateAt = fr.CreateAt
                                              })
                                              .ToListAsync();
            return favoriteRooms;
        }
        // Create
        public async Task<FavoriteRoomResponseDto> CreateAsync(FavoriteRoomCreateDto dto, int customerId)
        {
            await CheckCustomerId(customerId);
            await CheckRoomId(dto.RoomId);
            await CheckFavoriteExists(customerId, dto.RoomId);
            var favoriteRoom = MapToEntity(dto, customerId);
            await _context.FavoriteRooms.AddAsync(favoriteRoom);
            await _context.SaveChangesAsync();
            return MapToResponseDto(favoriteRoom);
        }
        // Delete
        public async Task<bool> DeleteAsync(int id, int customerId)
        {
            var favoriteRoom = await _context.FavoriteRooms
                                             .FirstOrDefaultAsync(fr => fr.FavoriteRoomId == id
                                                                     && fr.CustomerId == customerId);
            if (favoriteRoom is null) return false;
            _context.FavoriteRooms.Remove(favoriteRoom);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
