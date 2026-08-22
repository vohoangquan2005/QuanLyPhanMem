using HotelManagement.Data;
using HotelManagement.DTOs.RoomType;
using HotelManagement.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HotelManagement.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class RoomTypeController : ControllerBase
    {
        private readonly RoomTypeService _roomTypeService;
        public RoomTypeController(RoomTypeService roomTypeService)
        {
            _roomTypeService = roomTypeService;
        }
        [Authorize(Roles = "Admin,Staff,Customer")]
        [HttpGet]   // Lấy danh sách roomtype đang hoạt động (Active)
        public async Task<IActionResult> GetAll()
        {
            var roomTypes = await _roomTypeService.GetAllAsync();
            return Ok(roomTypes);
        }
        [Authorize(Roles = "Admin")]
        [HttpGet("deleted")]    // Lấy danh sách roomtype đã xóa (Inactive)
        public async Task<IActionResult> GetDeleted()
        {
            var roomTypes = await _roomTypeService.GetDeletedAsync();
            return Ok(roomTypes);
        }
        [Authorize(Roles = "Admin,Staff")]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var roomType = await _roomTypeService.GetByIdAsync(id);
            if (roomType is null) return NotFound();
            return Ok(roomType);
        }
        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> Create(RoomTypeCreateDto dto)
        {
            var createRoomType = await _roomTypeService.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById),
                                    new { id = createRoomType.RoomTypeId },
                                    createRoomType);
        }
        [Authorize(Roles = "Admin")]
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, RoomTypeUpdateDto dto)
        {
            var updateRoomType = await _roomTypeService.UpdateAsync(id, dto);
            if (updateRoomType is null) return NotFound();
            return NoContent();
        }
        [Authorize(Roles = "Admin")]
        [HttpPatch("{id}/soft-delete")]
        public async Task<IActionResult> SoftDelete(int id)
        {
            var deleteRoomType = await _roomTypeService.SoftDeleteAsync(id);

            if (deleteRoomType is null)
                return NotFound();
            return NoContent();
        }
        [Authorize(Roles = "Admin")]
        [HttpPatch("{id}/restore")]
        public async Task<IActionResult> Restore(int id)
        {
            var restoreRoomType = await _roomTypeService.RestoreAsync(id);
            if (restoreRoomType is null)
                return NotFound();
            return NoContent();
        }
        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var deleteRoomType = await _roomTypeService.DeleteAsync(id);
            if (!deleteRoomType) return NotFound();
            return NoContent();
        }
    }
}
