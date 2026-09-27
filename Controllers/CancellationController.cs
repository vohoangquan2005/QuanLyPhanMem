using HotelManagement.DTOs.Cancellation;
using HotelManagement.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace HotelManagement.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CancellationController : ControllerBase
    {
        private readonly CancellationService _cancellationService;

        public CancellationController(CancellationService cancellationService)
        {
            _cancellationService = cancellationService;
        }
        [Authorize(Roles = "Customer")]
        [HttpPost("my")]
        public async Task<IActionResult> CreateForCurrentCustomer(CancellationCreateDto dto)
        {
            if (!int.TryParse(User.FindFirst("CustomerId")?.Value, out var customerId)) return Unauthorized();
            var cancellation = await _cancellationService.CreateForCustomerAsync(dto, customerId);
            return CreatedAtAction(nameof(GetById), new { id = cancellation.CancellationId }, cancellation);
        }
        [Authorize(Roles = "Admin,Staff")]
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var cancellations = await _cancellationService.GetAllAsync();
            return Ok(cancellations);
        }
        [Authorize(Roles = "Admin,Staff")]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var cancellation = await _cancellationService.GetByIdAsync(id);
            if (cancellation is null) return NotFound();
            return Ok(cancellation);
        }
        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> Create(CancellationCreateDto dto)
        {
            var cancellation = await _cancellationService.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById),
                                  new { id = cancellation.CancellationId },
                                  cancellation);
        }
        [Authorize(Roles = "Admin, Staff")]
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, CancellationUpdateDto dto)
        {
            var cancellation = await _cancellationService.UpdateAsync(id, dto);
            if (cancellation is null) return NotFound();
            return NoContent();
        }
        [Authorize(Roles = "Admin,Staff")]
        [HttpPut("{id}/confirm")]
        public async Task<IActionResult> Confirm(int id)
        {
            var result = await _cancellationService.ConfirmAsync(id);
            if (!result) return NotFound();
            return NoContent();
        }
        [Authorize(Roles = "Admin,Staff")]
        [HttpPut("{id}/reject")]
        public async Task<IActionResult> Reject(int id)
        {
            var result = await _cancellationService.RejectAsync(id);
            if (!result) return NotFound();
            return NoContent();
        }
        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _cancellationService.DeleteAsync(id);
            if (!result) return NotFound();
            return NoContent();
        }
    }
}
