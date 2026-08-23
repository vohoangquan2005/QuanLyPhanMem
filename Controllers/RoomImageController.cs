using HotelManagement.DTOs.RoomImage;
using HotelManagement.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HotelManagement.Controllers
{
    [Authorize(Roles = "Admin")]
    [ApiController]
    [Route("api/[controller]")]
    public class RoomImageController : ControllerBase
    {
        private readonly RoomImageService _roomImageService;
        public RoomImageController(RoomImageService roomImageService)
        {
            _roomImageService = roomImageService;
        }
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var roomImages = await _roomImageService.GetAllAsync();
            return Ok(roomImages);
        }
        [AllowAnonymous]
        [HttpGet("public")]
        public async Task<IActionResult> GetPublic()
        {
            var roomImages = await _roomImageService.GetPublicAsync();
            return Ok(roomImages);
        }
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var roomImage = await _roomImageService.GetByIdAsync(id);
            if (roomImage is null)
                return NotFound();
            return Ok(roomImage);
        }
        [HttpPost]
        public async Task<IActionResult> Create([FromForm] RoomImageCreateDto dto)
        {
            var createRoomImage = await _roomImageService.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById),
                                         new { id = createRoomImage.RoomImageId },
                                         createRoomImage);
        }
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromForm]  RoomImageUpdateDto dto)
        {
            var updateRoomImage = await _roomImageService.UpdateAsync(id, dto);
            if (updateRoomImage is null) return NotFound();
            return NoContent();
        }
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var deleteRoomImage = await _roomImageService.DeleteAsync(id);
            if (!deleteRoomImage) return NotFound();
            return NoContent();
        }
    }
}
