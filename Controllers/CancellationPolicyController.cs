using HotelManagement.DTOs.CancallationPolicy;
using HotelManagement.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HotelManagement.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CancellationPolicyController : ControllerBase
    {
        private readonly CancellationPolicyService _cancellationPolicyService;
        public CancellationPolicyController(CancellationPolicyService cancellationPolicyService)
        {
            _cancellationPolicyService = cancellationPolicyService;
        }
        [Authorize(Roles = "Admin,Staff")]
        [HttpGet]   // Lấy danh sách user đang hoạt động (Active)
        public async Task<IActionResult> GetAll()
        {
            var policies = await _cancellationPolicyService.GetAllAsync();
            return Ok(policies);
        }
        [Authorize(Roles = "Admin")]
        [HttpGet("deleted")]    // Lấy danh sách user đã xóa (Inactive)
        public async Task<IActionResult> GetDeleted()
        {
            var policies = await _cancellationPolicyService.GetDeletedAsync();
            return Ok(policies);
        }
        [Authorize(Roles = "Admin,Staff")]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var policy = await _cancellationPolicyService.GetByIdAsync(id);
            if (policy is null) return NotFound();
            return Ok(policy);
        }
        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> Create(CancellationPolicyCreateDto dto)
        {
            var createPolicy = await _cancellationPolicyService.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById),
                                    new { id = createPolicy.CancellationPolicyId },
                                    createPolicy);
        }
        [Authorize(Roles = "Admin")]
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, CancellationPolicyUpdateDto dto)
        {
            var updatePolicy = await _cancellationPolicyService.UpdateAsync(id, dto);
            if (updatePolicy is null) return NotFound();
            return NoContent();
        }
        [Authorize(Roles = "Admin")]
        [HttpPatch("{id}/soft-delete")]
        public async Task<IActionResult> SoftDelete(int id)
        {
            var deletePolicy = await _cancellationPolicyService.SoftDeleteAsync(id);
            if (deletePolicy is null)
                return NotFound();
            return NoContent();
        }
        [Authorize(Roles = "Admin")]
        [HttpPatch("{id}/restore")]
        public async Task<IActionResult> Restore(int id)
        {
            var restorePolicy = await _cancellationPolicyService.RestoreAsync(id);
            if (restorePolicy is null)
                return NotFound();
            return NoContent();
        }
        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var deletePolicy = await _cancellationPolicyService.DeleteAsync(id);
            if (!deletePolicy) return NotFound();
            return NoContent();
        }
    }
}
