using HotelManagement.DTOs.Service;
using HotelManagement.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HotelManagement.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class ServiceController : ControllerBase
    {
        private readonly ServiceService _serviceService;
        public ServiceController(ServiceService serviceService)
        {
            _serviceService = serviceService;
        }
        [Authorize(Roles = "Admin,Staff,Customer")]
        [HttpGet]   // Lấy danh sách service đang hoạt động (Active)
        public async Task<IActionResult> GetAll()
        {
            var services = await _serviceService.GetAllAsync();
            return Ok(services);
        }
        [Authorize(Roles = "Admin")]
        [HttpGet("deleted")]    // Lấy danh sách service đã xóa (Inactive)
        public async Task<IActionResult> GetDeleted()
        {
            var services = await _serviceService.GetDeletedAsync();
            return Ok(services);
        }
        [Authorize(Roles = "Admin,Staff")]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var service = await _serviceService.GetByIdAsync(id);
            if (service is null) return NotFound();
            return Ok(service);
        }
        [Authorize(Roles = "Admin,Staff,Customer")]
        [HttpGet("search")]
        public async Task<IActionResult> Search([FromQuery] ServiceSearchDto request)
        {
            var services = await _serviceService.SearchAsync(request);
            return Ok(services);
        }
        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> Create(ServiceCreateDto dto)
        {
            var createService = await _serviceService.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById),
                                    new { id = createService.ServiceId },
                                    createService);
        }
        [Authorize(Roles = "Admin")]
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, ServiceUpdateDto dto)
        {
            var updateService = await _serviceService.UpdateAsync(id, dto);
            if (updateService is null) return NotFound();
            return NoContent();
        }
        [Authorize(Roles = "Admin")]
        [HttpPatch("{id}/soft-delete")]
        public async Task<IActionResult> SoftDelete(int id)
        {
            var deleteService = await _serviceService.SoftDeleteAsync(id);

            if (deleteService is null)
                return NotFound();
            return NoContent();
        }
        [Authorize(Roles = "Admin")]
        [HttpPatch("{id}/restore")]
        public async Task<IActionResult> Restore(int id)
        {
            var restoreService = await _serviceService.RestoreAsync(id);
            if (restoreService is null)
                return NotFound();
            return NoContent();
        }
        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var deleteService = await _serviceService.DeleteAsync(id);
            if (!deleteService) return NotFound();
            return NoContent();
        }
    }
}
