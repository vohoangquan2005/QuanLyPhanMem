using HotelManagement.DTOs.Payment;
using HotelManagement.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace HotelManagement.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class PaymentController : ControllerBase
    {
        private readonly PaymentService _paymentService;

        public PaymentController(PaymentService paymentService)
        {
            _paymentService = paymentService;
        }
        private int? CurrentUserId => int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : null;
        private int? CurrentCustomerId => int.TryParse(User.FindFirst("CustomerId")?.Value, out var id) ? id : null;
        [Authorize(Roles = "Admin,Staff")]
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var payments = await _paymentService.GetAllAsync();
            return Ok(payments);
        }
        [Authorize(Roles = "Customer")]
        [HttpGet("my-payments")]
        public async Task<IActionResult> GetMyPayments()
        {
            var customerId = CurrentCustomerId;
            return customerId is null ? Unauthorized() : Ok(await _paymentService.GetMyPaymentsAsync(customerId.Value));
        }
        [Authorize(Roles = "Customer")]
        [HttpPut("{id}/submit")]
        public async Task<IActionResult> SubmitForPayment(int id, PaymentSubmitDto dto)
        {
            var customerId = CurrentCustomerId;
            if (customerId is null) return Unauthorized();
            var payment = await _paymentService.SubmitForPaymentAsync(id, customerId.Value, dto);
            return payment is null ? NotFound() : Ok(payment);
        }
        [Authorize(Roles = "Customer")]
        [HttpPost("my-invoices/{invoiceId:int}/request")]
        public async Task<IActionResult> RequestInvoiceBalancePayment(int invoiceId)
        {
            var customerId = CurrentCustomerId;
            if (customerId is null) return Unauthorized();
            var payment = await _paymentService.CreateCustomerBalancePaymentAsync(invoiceId, customerId.Value);
            return payment is null ? NotFound() : Ok(payment);
        }
        [Authorize(Roles = "Admin,Staff")]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var payment = await _paymentService.GetByIdAsync(id);
            if (payment is null) return NotFound();
            return Ok(payment);
        }
        [Authorize(Roles = "Admin,Staff")]
        [HttpPost]
        public async Task<IActionResult> Create(PaymentCreateDto dto)
        {
            var payment = await _paymentService.CreateAsync(dto, CurrentUserId);
            return CreatedAtAction(nameof(GetById),
                                   new { id = payment.PaymentId },
                                   payment);
        }
        [Authorize(Roles = "Admin,Staff")]
        [HttpPut("{id}/confirm")]
        public async Task<IActionResult> Confirm(int id)
        {
            var result = await _paymentService.ConfirmAsync(id, CurrentUserId);
            if (result == false) return NotFound();
            return NoContent();
        }
        [Authorize(Roles = "Admin,Staff")]
        [HttpPut("{id}/reject")]
        public async Task<IActionResult> Reject(int id)
        {
            var result = await _paymentService.RejectAsync(id);
            if (result == false) return NotFound();
            return NoContent();
        }
        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _paymentService.DeleteAsync(id);
            if (!result) return NotFound();
            return NoContent();
        }
    }
}
