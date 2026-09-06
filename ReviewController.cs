using HotelManagement.DTOs.Review;
using HotelManagement.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HotelManagement.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class ReviewController : ControllerBase
    {
        private readonly ReviewService _reviewService;
        public ReviewController(ReviewService reviewService)
        {
            _reviewService = reviewService;
        }
        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var includeModeration = User.IsInRole("Admin") || User.IsInRole("Staff");
            var reviews = await _reviewService.GetAllAsync(includeModeration);
            return Ok(reviews);
        }
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            ReviewResponseDto? review;
            if (User.IsInRole("Customer"))
            {
                var customerIdClaim = User.FindFirst("CustomerId");
                if (customerIdClaim is null || !int.TryParse(customerIdClaim.Value, out var customerId))
                    return Unauthorized("Không phải tài khoản khách hàng hợp lệ!");
                review = await _reviewService.GetForCustomerByIdAsync(id, customerId);
            }
            else
            {
                review = await _reviewService.GetByIdAsync(id);
            }
            if (review is null) return NotFound();
            return Ok(review);
        }
        [Authorize(Roles = "Customer")]
        [HttpGet("my-bookings/{bookingId:int}")]
        public async Task<IActionResult> GetMyBookingReviews(int bookingId)
        {
            var customerIdClaim = User.FindFirst("CustomerId");
            if (customerIdClaim is null || !int.TryParse(customerIdClaim.Value, out var customerId))
                return Unauthorized("Không phải tài khoản khách hàng hợp lệ!");

            return Ok(await _reviewService.GetForCustomerBookingAsync(bookingId, customerId));
        }
        [Authorize(Roles = "Customer")]
        [HttpPost]
        public async Task<IActionResult> Create(ReviewCreateDto dto)
        {
            var customerIdClaim = User.FindFirst("CustomerId");
            if (customerIdClaim is null || !int.TryParse(customerIdClaim.Value, out var customerId))
                return Unauthorized("Không phải tài khoản khách hàng hợp lệ!");

            var review = await _reviewService.CreateAsync(dto, customerId);
            return CreatedAtAction(nameof(GetById),
                                   new { id = review.ReviewId },
                                   review);
        }
        [Authorize(Roles = "Admin,Staff")]
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, ReviewUpdateDto dto)
        {
            var review = await _reviewService.UpdateAsync(id, dto);
            if (review is null) return NotFound();
            return NoContent();
        }
        [Authorize(Roles = "Admin,Staff")]
        [HttpPut("{id}/confirm")]
        public async Task<IActionResult> Confirm(int id)
        {
            var review = await _reviewService.ConfirmAsync(id);
            if (!review) return NotFound();
            return NoContent();
        }
        [Authorize(Roles = "Admin,Staff")]
        [HttpPut("{id}/reject")]
        public async Task<IActionResult> Reject(int id)
        {
            var review = await _reviewService.RejectAsync(id);
            if (!review) return NotFound();
            return NoContent();
        }
        [Authorize(Roles = "Admin,Staff")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _reviewService.DeleteAsync(id);
            if (!result) return NotFound();
            return NoContent();
        }
    }
}
