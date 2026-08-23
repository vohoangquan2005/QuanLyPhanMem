using HotelManagement.DTOs.ServicePackage;
using HotelManagement.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HotelManagement.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class ServicePackageController : ControllerBase
    {
        private readonly ServicePackageService _servicePackageService;
        public ServicePackageController(ServicePackageService servicePackageService) 
        {
            _servicePackageService = servicePackageService;
        }
        [Authorize(Roles = "Admin,Staff,Customer")]
        [HttpGet]   // Lấy danh sách service package đang hoạt động (Active)
        public async Task<IActionResult> GetAll()
        {
            var package = await _servicePackageService.GetAllAsync();
            return Ok(package);
        }
        [Authorize(Roles = "Admin")]
        [HttpGet("deleted")]    // Lấy danh sách service package đã xóa (Inactive)
        public async Task<IActionResult> GetDeleted()
        {
            var package = await _servicePackageService.GetDeletedAsync();
            return Ok(package);
        }
        [Authorize(Roles = "Admin,Staff")]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var package = await _servicePackageService.GetByIdAsync(id);
            if (package is null) return NotFound();
            return Ok(package);
        }
        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> Create(ServicePackageCreateDto dto)
        {
            var createPackage = await _servicePackageService.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById),
                                    new { id = createPackage.ServicePackageId },
                                    createPackage);
        }
        [Authorize(Roles = "Admin")]
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, ServicePackageUpdateDto dto)
        {
            var updatePackage = await _servicePackageService.UpdateAsync(id, dto);
            if (updatePackage is null) return NotFound();
            return NoContent();
        }
        [Authorize(Roles = "Admin")]
        [HttpPatch("{id}/soft-delete")]
        public async Task<IActionResult> SoftDelete(int id)
        {
            var deletePackage = await _servicePackageService.SoftDeleteAsync(id);

            if (deletePackage is null)
                return NotFound();
            return NoContent();
        }
        [Authorize(Roles = "Admin")]
        [HttpPatch("{id}/restore")]
        public async Task<IActionResult> Restore(int id)
        {
            var restorePackage = await _servicePackageService.RestoreAsync(id);
            if (restorePackage is null)
                return NotFound();
            return NoContent();
        }
        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var deletePackage = await _servicePackageService.DeleteAsync(id);
            if (!deletePackage) return NotFound();
            return NoContent();
        }
    }
}
