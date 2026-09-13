using HotelManagement.DTOs.Guest;
using HotelManagement.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HotelManagement.Controllers
{
    [Authorize(Roles = "Admin,Staff")]
    [ApiController]
    [Route("api/[controller]")]
    public class GuestController : ControllerBase
    {
        private readonly GuestService _guestService;
        public GuestController(GuestService guestService)
        {
            _guestService = guestService;
        }
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var guests = await _guestService.GetAllAsync();
            return Ok(guests);
        }
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var guest = await _guestService.GetByIdAsync(id);
            if (guest is null) return NotFound();
            return Ok(guest);
        }
        [HttpPost]
        public async Task<IActionResult> Create(GuestCreateDto dto)
        {
            var createGuest = await _guestService.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById),
                                    new { id = createGuest.GuestId },
                                    createGuest);
        }
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, GuestUpdateDto dto)
        {
            var updateGuest = await _guestService.UpdateAsync(id, dto);
            if (updateGuest is null) return NotFound();
            return NoContent();
        }
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var deleteGuest = await _guestService.DeleteAsync(id);
            if (!deleteGuest) return NotFound();
            return NoContent();
        }
    }
}
