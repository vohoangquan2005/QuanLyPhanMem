using System.Text.Json;
using HotelManagement.Data;
using HotelManagement.DTOs.Invoice;
using HotelManagement.Enums;
using HotelManagement.Exceptions;
using HotelManagement.Models;
using Microsoft.EntityFrameworkCore;

namespace HotelManagement.Services
{
    public class InvoiceService
    {
        private readonly AppDbContext _context;
        private readonly AuditLogService _auditLogService;
        private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

        public InvoiceService(AppDbContext context, AuditLogService auditLogService)
        {
            _context = context;
            _auditLogService = auditLogService;
        }

        private static decimal CalculatePaidAmount(IEnumerable<Payment> payments)
        {
            var paid = payments.Where(p => p.Status == PaymentEnums.Success)
                .Sum(p => p.PaymentType == PaymentTypeEnums.Refund ? -p.Amount : p.Amount);
            return Math.Max(0, paid);
        }

        private async Task<InvoiceResponseDto> MapToResponseDto(Invoice invoice)
        {
            var payments = await _context.Payments
                .Where(p => p.BookingId == invoice.BookingId || p.InvoiceId == invoice.InvoiceId)
                .ToListAsync();
            var paid = CalculatePaidAmount(payments);
            var lines = string.IsNullOrWhiteSpace(invoice.LineItemsJson)
                ? new List<InvoiceLineItemDto>()
                : JsonSerializer.Deserialize<List<InvoiceLineItemDto>>(invoice.LineItemsJson, JsonOptions) ?? new();
            return new InvoiceResponseDto
            {
                InvoiceId = invoice.InvoiceId,
                BookingId = invoice.BookingId,
                CustomerName = invoice.CustomerFullName ?? invoice.Booking?.Customer?.User?.FullName,
                CustomerCCCD = invoice.CustomerCCCD ?? invoice.Booking?.Customer?.CCCD,
                CustomerNationality = invoice.CustomerNationality ?? invoice.Booking?.Customer?.Nationality,
                ExpectedCheckIn = invoice.ExpectedCheckIn ?? invoice.Booking?.ExpectedCheckIn,
                ExpectedCheckOut = invoice.ExpectedCheckOut ?? invoice.Booking?.ExpectedCheckOut,
                ActualCheckIn = invoice.ActualCheckIn,
                ActualCheckOut = invoice.ActualCheckOut,
                LineItems = lines,
                SubTotal = invoice.SubTotal,
                TaxAmount = invoice.TaxAmount,
                DiscountAmount = invoice.DiscountAmount,
                AmountDue = invoice.AmountDue,
                PaidAmount = paid,
                RemainingAmount = Math.Max(0, invoice.AmountDue - paid),
                Status = invoice.Status,
                CreatedByUserId = invoice.CreatedByUserId,
                CreateAt = invoice.CreateAt
            };
        }

        public async Task<InvoiceSnapshotDto> BuildFinalSnapshotAsync(int bookingId)
        {
            var booking = await _context.Bookings
                .Include(b => b.Customer).ThenInclude(c => c!.User)
                .Include(b => b.BookingDetails).ThenInclude(d => d.Room)
                .Include(b => b.BookingServices).ThenInclude(s => s.Service)
                .Include(b => b.ServicePackage)
                .FirstOrDefaultAsync(b => b.BookingId == bookingId && b.IsDeleted == ActiveEnums.Active);
            if (booking is null) throw new NotFoundException("Không tìm thấy đặt phòng để lập hóa đơn!");

            var lines = new List<InvoiceLineItemDto>();
            foreach (var detail in booking.BookingDetails.OrderBy(d => d.BookingDetailId))
            {
                var nights = detail.ActualCheckIn.HasValue && detail.ActualCheckOut.HasValue
                    ? Math.Max(1, (detail.ActualCheckOut.Value.Date - detail.ActualCheckIn.Value.Date).Days)
                    : 0;
                lines.Add(new InvoiceLineItemDto
                {
                    Category = "Room",
                    Description = detail.Room?.RoomName ?? $"Phòng #{detail.RoomId}",
                    Quantity = nights,
                    UnitPrice = detail.PricePerNight,
                    Amount = detail.TotalAmount
                });
            }
            if (booking.PackageAmount > 0)
                lines.Add(new InvoiceLineItemDto
                {
                    Category = "Package",
                    Description = booking.ServicePackage?.PackageName ?? "Gói dịch vụ",
                    Amount = booking.PackageAmount,
                    UnitPrice = booking.PackageAmount
                });
            foreach (var service in booking.BookingServices.OrderBy(s => s.BookingServiceId))
                lines.Add(new InvoiceLineItemDto
                {
                    Category = "Service",
                    Description = service.Service?.ServiceName ?? $"Dịch vụ #{service.ServiceId}",
                    Quantity = service.Quantity,
                    UnitPrice = service.UnitPrice,
                    Amount = service.TotalAmount
                });

            var subTotal = booking.RoomAmount + booking.PackageAmount + booking.AdditionalServiceAmount;
            var discount = Math.Clamp(booking.DiscountAmount, 0, subTotal);
            var taxable = Math.Max(0, subTotal - discount);
            var tax = Math.Round(taxable * 0.10m, 2, MidpointRounding.AwayFromZero);
            if (discount > 0)
                lines.Add(new InvoiceLineItemDto { Category = "Discount", Description = booking.PromotionNameSnapshot ?? "Khuyến mãi", Amount = -discount, UnitPrice = -discount });
            lines.Add(new InvoiceLineItemDto { Category = "Tax", Description = "Thuế VAT (10%)", Amount = tax, UnitPrice = tax });

            return new InvoiceSnapshotDto
            {
                BookingId = booking.BookingId,
                CustomerFullName = booking.Customer?.User?.FullName,
                CustomerCCCD = booking.Customer?.CCCD,
                CustomerNationality = booking.Customer?.Nationality,
                ExpectedCheckIn = booking.ExpectedCheckIn,
                ExpectedCheckOut = booking.ExpectedCheckOut,
                ActualCheckIn = booking.BookingDetails.Where(d => d.ActualCheckIn.HasValue).Select(d => d.ActualCheckIn).Min(),
                ActualCheckOut = booking.BookingDetails.Where(d => d.ActualCheckOut.HasValue).Select(d => d.ActualCheckOut).Max(),
                SubTotal = subTotal,
                TaxAmount = tax,
                DiscountAmount = discount,
                AmountDue = taxable + tax,
                LineItems = lines
            };
        }

        public async Task<InvoiceResponseDto> CreateFromSnapshotAsync(int bookingId, string snapshotJson, int? createdByUserId = null)
        {
            var existing = await _context.Invoices.FirstOrDefaultAsync(i => i.BookingId == bookingId);
            if (existing is not null)
                return (await GetByIdAsync(existing.InvoiceId))!;

            var snapshot = JsonSerializer.Deserialize<InvoiceSnapshotDto>(snapshotJson, JsonOptions)
                ?? throw new BadRequestException("Dữ liệu chốt hóa đơn không hợp lệ!");
            var payments = await _context.Payments.Where(p => p.BookingId == bookingId).ToListAsync();
            var paid = CalculatePaidAmount(payments);
            var invoice = new Invoice
            {
                BookingId = bookingId,
                CustomerFullName = snapshot.CustomerFullName,
                CustomerCCCD = snapshot.CustomerCCCD,
                CustomerNationality = snapshot.CustomerNationality,
                ExpectedCheckIn = snapshot.ExpectedCheckIn,
                ExpectedCheckOut = snapshot.ExpectedCheckOut,
                ActualCheckIn = snapshot.ActualCheckIn,
                ActualCheckOut = snapshot.ActualCheckOut,
                LineItemsJson = JsonSerializer.Serialize(snapshot.LineItems, JsonOptions),
                SubTotal = snapshot.SubTotal,
                TaxAmount = snapshot.TaxAmount,
                DiscountAmount = snapshot.DiscountAmount,
                AmountDue = snapshot.AmountDue,
                Status = paid <= 0 ? InvoiceEnums.Unpaid : paid < snapshot.AmountDue ? InvoiceEnums.PartiallyPaid : InvoiceEnums.Paid,
                CreatedByUserId = createdByUserId
            };
            _context.Invoices.Add(invoice);
            await _context.SaveChangesAsync();
            foreach (var payment in payments) payment.InvoiceId = invoice.InvoiceId;
            await _context.SaveChangesAsync();
            await _auditLogService.LogAsync(AuditLogEnums.Create, "Invoices", invoice.InvoiceId, null,
                $"BookingId: {bookingId}, AmountDue: {invoice.AmountDue}, Status: {invoice.Status}");
            return (await GetByIdAsync(invoice.InvoiceId))!;
        }

        private async Task CheckCreateByUserId(int? userId)
        {
            if (userId is null) return;
            var roleName = await _context.Users.Where(u => u.UserId == userId)
                .Select(u => u.Role!.RoleName).FirstOrDefaultAsync();
            if (roleName is null) throw new NotFoundException("Mã nhân viên tạo hóa đơn không hợp lệ!");
            if (roleName is not ("Admin" or "Staff"))
                throw new BadRequestException("Người tạo hóa đơn phải là nhân viên hoặc quản trị viên!");
        }

        public async Task<List<InvoiceResponseDto>> GetAllAsync()
        {
            var invoices = await _context.Invoices.Include(i => i.Booking).ThenInclude(b => b!.Customer).ThenInclude(c => c!.User).ToListAsync();
            var result = new List<InvoiceResponseDto>();
            foreach (var invoice in invoices) result.Add(await MapToResponseDto(invoice));
            return result;
        }

        public async Task<List<InvoiceResponseDto>> GetMyInvoicesAsync(int customerId)
        {
            var invoices = await _context.Invoices.Where(i => i.Booking!.CustomerId == customerId)
                .Include(i => i.Booking).ThenInclude(b => b!.Customer).ThenInclude(c => c!.User).ToListAsync();
            var result = new List<InvoiceResponseDto>();
            foreach (var invoice in invoices) result.Add(await MapToResponseDto(invoice));
            return result;
        }

        public async Task<InvoiceResponseDto?> GetByIdAsync(int id)
        {
            var invoice = await _context.Invoices.Include(i => i.Booking).ThenInclude(b => b!.Customer).ThenInclude(c => c!.User)
                .FirstOrDefaultAsync(i => i.InvoiceId == id);
            return invoice is null ? null : await MapToResponseDto(invoice);
        }

        public async Task<InvoiceResponseDto?> GetMyInvoiceByIdAsync(int id, int customerId)
        {
            var invoice = await _context.Invoices.Where(i => i.InvoiceId == id && i.Booking!.CustomerId == customerId)
                .Include(i => i.Booking).ThenInclude(b => b!.Customer).ThenInclude(c => c!.User).FirstOrDefaultAsync();
            return invoice is null ? null : await MapToResponseDto(invoice);
        }

        public async Task<InvoiceResponseDto> CreateAsync(InvoiceCreateDto dto, int? createdByUserId)
        {
            var booking = await _context.Bookings.Include(b => b.Customer).ThenInclude(c => c!.User)
                .FirstOrDefaultAsync(b => b.BookingId == dto.BookingId && b.IsDeleted == ActiveEnums.Active);
            if (booking is null) throw new NotFoundException("Mã đặt phòng không hợp lệ!");
            if (booking.Status != BookingEnums.Completed)
                throw new BadRequestException("Chỉ có thể lập hóa đơn cuối sau khi tất cả phòng đã checkout!");
            await CheckCreateByUserId(createdByUserId);
            if (await _context.Invoices.AnyAsync(i => i.BookingId == dto.BookingId))
                throw new ConflictException("Đặt phòng này đã có hóa đơn!");
            var snapshot = await BuildFinalSnapshotAsync(dto.BookingId);
            var paid = CalculatePaidAmount(await _context.Payments.Where(p => p.BookingId == dto.BookingId).ToListAsync());
            if (paid < snapshot.AmountDue)
                throw new BadRequestException("Chỉ lập hóa đơn cuối sau khi tổng chi phí đã được thanh toán!");
            return await CreateFromSnapshotAsync(dto.BookingId, JsonSerializer.Serialize(snapshot, JsonOptions), createdByUserId);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var invoice = await _context.Invoices.Include(i => i.Payments).FirstOrDefaultAsync(i => i.InvoiceId == id);
            if (invoice is null) return false;
            if (invoice.Payments.Any()) throw new BadRequestException("Không thể xóa hóa đơn đã có giao dịch thanh toán!");
            _context.Invoices.Remove(invoice);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
