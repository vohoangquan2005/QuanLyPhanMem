using HotelManagement.DTOs.Promotion;
using HotelManagement.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HotelManagement.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class PromotionController : ControllerBase
    {
        private readonly PromotionService _promotionService;
        public PromotionController(PromotionService promotionService)
        {
            _promotionService = promotionService;
        }
        [Authorize(Roles = "Admin,Staff")]
        [HttpGet]   // Lấy danh sách promotion đang hoạt động (Active)
        public async Task<IActionResult> GetAll()
        {
            var promotions = await _promotionService.GetAllAsync();
            return Ok(promotions);
        }
        [Authorize(Roles = "Admin")]
        [HttpGet("deleted")]    // Lấy danh sách promotion đã xóa (Inactive)
        public async Task<IActionResult> GetDeleted()
        {
            var promotions = await _promotionService.GetDeletedAsync();
            return Ok(promotions);
        }
        [Authorize(Roles = "Admin,Staff")]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var promotion = await _promotionService.GetByIdAsync(id);
            if (promotion is null) return NotFound();
            return Ok(promotion);
        }
        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] PromotionCreateRequestDto request)
        {
            var createPromotion = await _promotionService.CreateAsync(request);
            return CreatedAtAction(nameof(GetById),
                                    new { id = createPromotion.PromotionId },
                                    createPromotion);
        }
        [Authorize(Roles = "Admin")]
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] PromotionUpdateRequestDto request)
        {
            var updatePromotion = await _promotionService.UpdateAsync(id, request);
            if (updatePromotion is null) return NotFound();
            return NoContent();
        }
        [Authorize(Roles = "Admin")]
        [HttpPatch("{id}/soft-delete")]
        public async Task<IActionResult> SoftDelete(int id)
        {
            var deletePromotion = await _promotionService.SoftDeleteAsync(id);
            if (deletePromotion is null)
                return NotFound();
            return NoContent();
        }
        [Authorize(Roles = "Admin")]
        [HttpPatch("{id}/restore")]
        public async Task<IActionResult> Restore(int id)
        {
            var restorePromotion = await _promotionService.RestoreAsync(id);
            if (restorePromotion is null)
                return NotFound();
            return NoContent();
        }
        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var deletePromotion = await _promotionService.DeleteAsync(id);
            if (!deletePromotion) return NotFound();
            return NoContent();
        }
    }
}
