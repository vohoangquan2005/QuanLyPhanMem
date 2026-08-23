using HotelManagement.DTOs.Room;
using HotelManagement.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HotelManagement.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class RoomController : ControllerBase
    {
        private readonly RoomService _roomService;
        public RoomController(RoomService roomService)
        {
            _roomService = roomService;
        }

        [AllowAnonymous]
        [HttpGet]   // Lấy danh sách room đang hoạt động (Active)
        public async Task<IActionResult> GetAll()
        {
            var rooms = await _roomService.GetAllAsync();
            return Ok(rooms);
        }
        [Authorize(Roles = "Admin")]
        [HttpGet("deleted")]    // Lấy danh sách room đã xóa (Inactive)
        public async Task<IActionResult> GetDeleted()
        {
            var rooms = await _roomService.GetDeletedAsync();
            return Ok(rooms);
        }
        [AllowAnonymous]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var room = await _roomService.GetByIdAsync(id);
            if (room is null) return NotFound();
            return Ok(room);
        }
        [AllowAnonymous]
        [HttpGet("filter")]
        public async Task<IActionResult> Filter([FromQuery] RoomFilterDto filter)
        {
            var rooms = await _roomService.FilterAsync(filter);
            return Ok(rooms);
        }
        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> Create(RoomCreateDto dto)
        {
            var createRoom = await _roomService.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById),
                                    new { id = createRoom.RoomId },
                                    createRoom);
        }
        [Authorize(Roles = "Admin")]
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, RoomUpdateDto dto)
        {
            var updateRoom = await _roomService.UpdateAsync(id, dto);
            if (updateRoom is null) return NotFound();
            return NoContent();
        }
        [Authorize(Roles = "Admin")]
        [HttpPatch("{id}/soft-delete")]
        public async Task<IActionResult> SoftDelete(int id)
        {
            var deleteRoom = await _roomService.SoftDeleteAsync(id);
            if (deleteRoom is null) return NotFound();
            return NoContent();
        }
        [Authorize(Roles = "Admin")]
        [HttpPatch("{id}/restore")]
        public async Task<IActionResult> Restore(int id)
        {
            var restoreRoom = await _roomService.RestoreAsync(id);
            if (restoreRoom is null)
                return NotFound();
            return NoContent();
        }
        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var deleteRoom = await _roomService.DeleteAsync(id);
            if (!deleteRoom) return NotFound();
            return NoContent();
        }
    }
}
