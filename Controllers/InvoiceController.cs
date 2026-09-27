using HotelManagement.DTOs.Invoice;
using HotelManagement.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace HotelManagement.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class InvoiceController : ControllerBase
    {
        private readonly InvoiceService _invoiceService;
        public InvoiceController(InvoiceService invoiceService)
        {
            _invoiceService = invoiceService;
        }
        private int? CurrentCustomerId => int.TryParse(User.FindFirst("CustomerId")?.Value, out var id) ? id : null;
        private int? CurrentUserId => int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : null;
        [Authorize(Roles = "Admin,Staff")]
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var invoices = await _invoiceService.GetAllAsync();
            return Ok(invoices);
        }
        [Authorize(Roles = "Customer")]
        [HttpGet("my-invoices")]
        public async Task<IActionResult> GetMyInvoices()
        {
            var customerId = CurrentCustomerId;
            return customerId is null ? Unauthorized() : Ok(await _invoiceService.GetMyInvoicesAsync(customerId.Value));
        }
        [Authorize(Roles = "Customer")]
        [HttpGet("my-invoices/{id:int}")]
        public async Task<IActionResult> GetMyInvoiceById(int id)
        {
            var customerId = CurrentCustomerId;
            if (customerId is null) return Unauthorized();
            var invoice = await _invoiceService.GetMyInvoiceByIdAsync(id, customerId.Value);
            return invoice is null ? NotFound() : Ok(invoice);
        }
        [Authorize(Roles = "Admin,Staff")]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var invoice = await _invoiceService.GetByIdAsync(id);
            if (invoice is null) return NotFound();
            return Ok(invoice);
        }
        [Authorize(Roles = "Admin,Staff")]
        [HttpPost]
        public async Task<IActionResult> Create(InvoiceCreateDto dto)
        {
            var createInvoice = await _invoiceService.CreateAsync(dto, CurrentUserId);
            return CreatedAtAction(nameof(GetById),
                                   new { id = createInvoice.InvoiceId },
                                   createInvoice);
        }
        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var deleteInvoice = await _invoiceService.DeleteAsync(id);
            if (!deleteInvoice) return NotFound();
            return NoContent();
        }
    }
}
