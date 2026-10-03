using System.Data;
using System.Text.Json;
using HotelManagement.Data;
using HotelManagement.DTOs.Payment;
using HotelManagement.Enums;
using HotelManagement.Exceptions;
using HotelManagement.Models;
using Microsoft.EntityFrameworkCore;

namespace HotelManagement.Services
{
    public class PaymentService
    {
        private readonly AppDbContext _context;
        private readonly AuditLogService _auditLogService;
        private readonly InvoiceService _invoiceService;
        private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

        public PaymentService(AppDbContext context, AuditLogService auditLogService, InvoiceService invoiceService)
        {
            _context = context;
            _auditLogService = auditLogService;
            _invoiceService = invoiceService;
        }

        private static PaymentResponseDto MapToResponseDto(Payment p) => new()
        {
            PaymentId = p.PaymentId,
            InvoiceId = p.InvoiceId,
            BookingId = p.BookingId ?? p.Invoice?.BookingId,
            BookingStatus = p.Booking?.Status ?? p.Invoice?.Booking?.Status,
            Amount = p.Amount,
            PaymentType = p.PaymentType,
            PaymentMethod = p.PaymentMethod,
            TransactionCode = p.TransactionCode,
            Status = p.Status,
            CreatedByUserId = p.CreatedByUserId,
            PaymentDate = p.PaymentDate,
            CreateAt = p.CreateAt,
            RefundSource = p.RefundSource
        };

        private IQueryable<Payment> PaymentsForBooking(int bookingId) =>
            _context.Payments.Where(p => p.BookingId == bookingId || p.Invoice!.BookingId == bookingId);

        private async Task<decimal> GetNetPaidAsync(int bookingId) => Math.Max(0,
            await PaymentsForBooking(bookingId).Where(p => p.Status == PaymentEnums.Success)
                .Select(p => p.PaymentType == PaymentTypeEnums.Refund ? -p.Amount : p.Amount).SumAsync());

        private async Task<decimal> GetBookingAmountDueAsync(int bookingId)
        {
            var snapshot = await _invoiceService.BuildFinalSnapshotAsync(bookingId);
            return snapshot.AmountDue;
        }

        private async Task ValidateAmountAsync(Payment payment)
        {
            if (payment.BookingId is null && payment.InvoiceId is not null)
                payment.BookingId = await _context.Invoices.Where(i => i.InvoiceId == payment.InvoiceId)
                    .Select(i => (int?)i.BookingId).FirstOrDefaultAsync();
            if (payment.BookingId is null) throw new BadRequestException("Giao dịch phải gắn với hóa đơn hoặc đặt phòng!");

            var bookingId = payment.BookingId.Value;
            if (payment.PaymentType != PaymentTypeEnums.Refund &&
                await _context.Cancellations.AnyAsync(c => c.BookingId == bookingId && c.Status == CancellationEnums.Pending))
                throw new BadRequestException("Đặt phòng đang chờ xử lý yêu cầu hủy, không thể xác nhận thanh toán!");
            var netPaid = await GetNetPaidAsync(bookingId);
            if (payment.PaymentType == PaymentTypeEnums.Refund)
            {
                var otherPendingRefunds = await PaymentsForBooking(bookingId)
                    .Where(p => p.Status == PaymentEnums.Pending && p.PaymentType == PaymentTypeEnums.Refund && p.PaymentId != payment.PaymentId)
                    .SumAsync(p => (decimal?)p.Amount) ?? 0;
                if (payment.Amount > Math.Max(0, netPaid - otherPendingRefunds))
                    throw new BadRequestException("Số tiền hoàn không được vượt quá số tiền đã thu chưa hoàn!");
                return;
            }

            var amountDue = payment.InvoiceId.HasValue
                ? await _context.Invoices.Where(i => i.InvoiceId == payment.InvoiceId.Value).Select(i => (decimal?)i.AmountDue).FirstOrDefaultAsync() ?? await GetBookingAmountDueAsync(bookingId)
                : await GetBookingAmountDueAsync(bookingId);
            if (payment.Amount > Math.Max(0, amountDue - netPaid))
                throw new BadRequestException("Số tiền thanh toán vượt quá số tiền còn phải trả!");
        }

        private async Task CheckCreateByUserId(int? userId)
        {
            if (userId is null) return;
            var roleName = await _context.Users.Where(u => u.UserId == userId).Select(u => u.Role!.RoleName).FirstOrDefaultAsync();
            if (roleName is null) throw new NotFoundException("Mã nhân viên tạo thanh toán không hợp lệ!");
            if (roleName is not ("Admin" or "Staff")) throw new BadRequestException("Người tạo thanh toán phải là nhân viên hoặc quản trị viên!");
        }

        public async Task<List<PaymentResponseDto>> GetAllAsync() =>
            (await _context.Payments.Include(p => p.Invoice).OrderByDescending(p => p.CreateAt).ToListAsync()).Select(MapToResponseDto).ToList();

        public async Task<List<PaymentResponseDto>> GetMyPaymentsAsync(int customerId) =>
            (await _context.Payments.Include(p => p.Invoice).ThenInclude(i => i!.Booking).Include(p => p.Booking)
                .Where(p => (p.BookingId != null && p.Booking!.CustomerId == customerId) ||
                            (p.BookingId == null && p.Invoice!.Booking!.CustomerId == customerId))
                .OrderByDescending(p => p.CreateAt).ToListAsync()).Select(MapToResponseDto).ToList();

        public async Task<PaymentResponseDto?> CreateCustomerBalancePaymentAsync(int invoiceId, int customerId)
        {
            var invoice = await _context.Invoices.Include(i => i.Booking)
                .FirstOrDefaultAsync(i => i.InvoiceId == invoiceId && i.Booking!.CustomerId == customerId);
            if (invoice is null) return null;
            var netPaid = await GetNetPaidAsync(invoice.BookingId);
            var remaining = Math.Max(0, invoice.AmountDue - netPaid);
            if (remaining <= 0) throw new BadRequestException("Hóa đơn này đã được thanh toán đầy đủ!");
            var requestKey = $"invoice:{invoiceId}:balance";
            var pending = await _context.Payments.FirstOrDefaultAsync(p => p.RequestKey == requestKey && p.Status == PaymentEnums.Pending);
            if (pending is not null) return MapToResponseDto(pending);
            var retry = await _context.Payments.FirstOrDefaultAsync(p => p.RequestKey == requestKey && p.Status == PaymentEnums.Failed);
            if (retry is not null)
            {
                retry.Status = PaymentEnums.Pending;
                retry.Amount = remaining;
                retry.PaymentMethod = null;
                retry.TransactionCode = null;
                await _context.SaveChangesAsync();
                return MapToResponseDto(retry);
            }
            var payment = new Payment
            {
                BookingId = invoice.BookingId,
                InvoiceId = invoice.InvoiceId,
                Amount = remaining,
                PaymentType = PaymentTypeEnums.Payment,
                Status = PaymentEnums.Pending,
                RequestKey = requestKey
            };
            _context.Payments.Add(payment);
            await _context.SaveChangesAsync();
            await _auditLogService.LogAsync(AuditLogEnums.Create, "Payment", payment.PaymentId, null,
                $"BookingId: {invoice.BookingId}, InvoiceId: {invoice.InvoiceId}, Amount: {remaining}, Type: Payment");
            return MapToResponseDto(payment);
        }

        public async Task<PaymentResponseDto?> GetByIdAsync(int id)
        {
            var p = await _context.Payments.Include(x => x.Invoice).FirstOrDefaultAsync(x => x.PaymentId == id);
            return p is null ? null : MapToResponseDto(p);
        }

        public async Task<PaymentResponseDto> CreateAsync(PaymentCreateDto dto, int? staffUserId)
        {
            if (dto.InvoiceId is null && dto.BookingId is null) throw new BadRequestException("Vui lòng gắn giao dịch với hóa đơn hoặc đặt phòng!");
            if (dto.InvoiceId.HasValue && dto.BookingId.HasValue) throw new BadRequestException("Chỉ gửi một trong hai mã hóa đơn hoặc đặt phòng!");
            await CheckCreateByUserId(staffUserId);
            if (dto.InvoiceId.HasValue && !await _context.Invoices.AnyAsync(i => i.InvoiceId == dto.InvoiceId.Value))
                throw new NotFoundException("Mã hóa đơn không hợp lệ!");
            if (dto.BookingId.HasValue && !await _context.Bookings.AnyAsync(b => b.BookingId == dto.BookingId.Value))
                throw new NotFoundException("Mã đặt phòng không hợp lệ!");
            var payment = new Payment
            {
                BookingId = dto.BookingId,
                InvoiceId = dto.InvoiceId,
                Amount = dto.Amount,
                PaymentType = dto.PaymentType,
                PaymentMethod = dto.PaymentMethod,
                TransactionCode = dto.TransactionCode,
                CreatedByUserId = staffUserId,
                PaymentDate = null,
                Status = PaymentEnums.Pending
            };
            await ValidateAmountAsync(payment);
            _context.Payments.Add(payment);
            await _context.SaveChangesAsync();
            await _auditLogService.LogAsync(AuditLogEnums.Create, "Payment", payment.PaymentId, null,
                $"BookingId: {payment.BookingId}, InvoiceId: {payment.InvoiceId}, Amount: {payment.Amount}, Type: {payment.PaymentType}");
            return MapToResponseDto(payment);
        }

        public async Task<PaymentResponseDto> CreateDepositAsync(Booking booking, decimal depositAmount)
        {
            var requestKey = $"booking:{booking.BookingId}:deposit";
            var existing = await _context.Payments.FirstOrDefaultAsync(p => p.RequestKey == requestKey && p.Status != PaymentEnums.Failed);
            if (existing is not null) return MapToResponseDto(existing);
            var payment = new Payment
            {
                BookingId = booking.BookingId,
                Amount = depositAmount,
                PaymentType = PaymentTypeEnums.Deposit,
                Status = PaymentEnums.Pending,
                RequestKey = requestKey
            };
            _context.Payments.Add(payment);
            await _context.SaveChangesAsync();
            await _auditLogService.LogAsync(AuditLogEnums.Create, "Payment", payment.PaymentId, null,
                $"BookingId: {booking.BookingId}, Amount: {payment.Amount}, Type: Deposit, Status: Pending");
            return MapToResponseDto(payment);
        }

        public async Task RefreshPendingDepositAsync(int bookingId, decimal amount)
        {
            var payment = await _context.Payments.FirstOrDefaultAsync(p => p.RequestKey == $"booking:{bookingId}:deposit");
            if (payment is null || payment.Status == PaymentEnums.Success) return;
            if (amount <= 0)
            {
                payment.Status = PaymentEnums.Failed;
            }
            else
            {
                payment.Amount = amount;
                payment.Status = PaymentEnums.Pending;
                payment.PaymentMethod = null;
                payment.TransactionCode = null;
            }
            await _context.SaveChangesAsync();
        }

        public async Task CreateRefundAsync(int bookingId, decimal requestedAmount, string requestKey, string source)
        {
            if (requestedAmount <= 0) return;
            var prior = await _context.Payments.FirstOrDefaultAsync(p => p.RequestKey == requestKey && p.Status != PaymentEnums.Failed);
            if (prior is not null) return;
            var netPaid = await GetNetPaidAsync(bookingId);
            var pendingRefund = await PaymentsForBooking(bookingId).Where(p => p.Status == PaymentEnums.Pending && p.PaymentType == PaymentTypeEnums.Refund)
                .SumAsync(p => (decimal?)p.Amount) ?? 0;
            var amount = Math.Min(requestedAmount, Math.Max(0, netPaid - pendingRefund));
            if (amount <= 0) return;
            var invoiceId = await _context.Invoices.Where(i => i.BookingId == bookingId).Select(i => (int?)i.InvoiceId).FirstOrDefaultAsync();
            _context.Payments.Add(new Payment
            {
                BookingId = bookingId,
                InvoiceId = invoiceId,
                Amount = amount,
                PaymentType = PaymentTypeEnums.Refund,
                Status = PaymentEnums.Pending,
                RequestKey = requestKey,
                RefundSource = source
            });
            await _context.SaveChangesAsync();
            var created = await _context.Payments.FirstAsync(p => p.RequestKey == requestKey && p.Status == PaymentEnums.Pending);
            await _auditLogService.LogAsync(AuditLogEnums.Create, "Payment", created.PaymentId, null,
                $"BookingId: {bookingId}, Amount: {amount}, Type: Refund, Source: {source}, Status: Pending");
        }

        public async Task CancelPendingPaymentsAsync(int bookingId)
        {
            var pending = await PaymentsForBooking(bookingId)
                .Where(p => p.Status == PaymentEnums.Pending && p.PaymentType != PaymentTypeEnums.Refund)
                .ToListAsync();
            foreach (var payment in pending) payment.Status = PaymentEnums.Failed;
            if (pending.Count > 0) await _context.SaveChangesAsync();
        }

        public async Task<PaymentResponseDto?> CreateFinalPaymentAsync(int bookingId)
        {
            await CancelPendingPaymentsAsync(bookingId);
            var existingInvoice = await _context.Invoices.FirstOrDefaultAsync(i => i.BookingId == bookingId);
            var snapshot = existingInvoice is null ? await _invoiceService.BuildFinalSnapshotAsync(bookingId) : null;
            var snapshotJson = snapshot is null ? null : JsonSerializer.Serialize(snapshot, JsonOptions);
            var netPaid = await GetNetPaidAsync(bookingId);
            var amountDue = existingInvoice?.AmountDue ?? snapshot!.AmountDue;
            var amount = Math.Max(0, amountDue - netPaid);
            if (amount <= 0)
            {
                if (snapshotJson is not null) await _invoiceService.CreateFromSnapshotAsync(bookingId, snapshotJson);
                return null;
            }
            var requestKey = $"booking:{bookingId}:final";
            var existing = await _context.Payments.FirstOrDefaultAsync(p => p.RequestKey == requestKey && p.Status != PaymentEnums.Failed);
            if (existing is not null) return MapToResponseDto(existing);
            var failed = await _context.Payments.FirstOrDefaultAsync(p => p.RequestKey == requestKey && p.Status == PaymentEnums.Failed);
            if (failed is not null)
            {
                failed.Status = PaymentEnums.Pending;
                failed.Amount = amount;
                failed.InvoiceSnapshotJson = snapshotJson;
                failed.InvoiceId = existingInvoice?.InvoiceId;
                failed.TransactionCode = null;
                failed.PaymentMethod = null;
                await _context.SaveChangesAsync();
                return MapToResponseDto(failed);
            }
            var payment = new Payment
            {
                BookingId = bookingId,
                InvoiceId = existingInvoice?.InvoiceId,
                Amount = amount,
                PaymentType = PaymentTypeEnums.Payment,
                Status = PaymentEnums.Pending,
                RequestKey = requestKey,
                InvoiceSnapshotJson = snapshotJson
            };
            _context.Payments.Add(payment);
            await _context.SaveChangesAsync();
            await _auditLogService.LogAsync(AuditLogEnums.Create, "Payment", payment.PaymentId, null,
                $"BookingId: {bookingId}, Amount: {amount}, Type: Payment, Status: Pending");
            return MapToResponseDto(payment);
        }

        public async Task<PaymentResponseDto?> SubmitForPaymentAsync(int paymentId, int customerId, PaymentSubmitDto dto)
        {
            var payment = await _context.Payments.Include(p => p.Invoice).ThenInclude(i => i!.Booking).Include(p => p.Booking)
                .FirstOrDefaultAsync(p => p.PaymentId == paymentId &&
                    (p.Booking != null ? p.Booking.CustomerId == customerId : p.Invoice!.Booking!.CustomerId == customerId));
            if (payment is null) return null;
            if (payment.PaymentType == PaymentTypeEnums.Refund) throw new BadRequestException("Khách hàng không thể tự gửi hoặc xác nhận khoản hoàn tiền!");
            if (payment.Status is not (PaymentEnums.Pending or PaymentEnums.Failed)) throw new BadRequestException("Giao dịch không còn chờ thanh toán!");
            var bookingStatus = payment.Booking?.Status ?? payment.Invoice?.Booking?.Status;
            var bookingId = payment.BookingId ?? payment.Invoice?.BookingId;
            if (bookingId.HasValue && await _context.Cancellations.AnyAsync(c => c.BookingId == bookingId.Value && c.Status == CancellationEnums.Pending))
                throw new BadRequestException("Đặt phòng đang chờ xử lý yêu cầu hủy, vui lòng đợi kết quả trước khi thanh toán!");
            if (bookingStatus is BookingEnums.Cancelled or BookingEnums.Rejected)
                throw new BadRequestException("Đặt phòng đã hủy hoặc bị từ chối, không thể thanh toán!");
            if (payment.PaymentType == PaymentTypeEnums.Deposit && bookingStatus is not (BookingEnums.Pending or BookingEnums.Confirmed))
                throw new BadRequestException("Tiền cọc chỉ có thể thanh toán khi đặt phòng đang chờ hoặc đã xác nhận!");
            if (payment.PaymentType == PaymentTypeEnums.Payment && bookingStatus != BookingEnums.Completed && !payment.InvoiceId.HasValue)
                throw new BadRequestException("Chỉ có thể thanh toán cuối sau khi hoàn tất checkout!");
            if (payment.Status == PaymentEnums.Failed) payment.Status = PaymentEnums.Pending;
            payment.PaymentMethod = dto.PaymentMethod.Trim();
            payment.TransactionCode = dto.TransactionCode?.Trim();
            await _context.SaveChangesAsync();
            return MapToResponseDto(payment);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var payment = await _context.Payments.FindAsync(id);
            if (payment is null) return false;
            if (payment.Status == PaymentEnums.Success) throw new BadRequestException("Không thể xóa giao dịch thanh toán đã thành công!");
            _context.Payments.Remove(payment);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool?> ConfirmAsync(int id, int? staffUserId)
        {
            await using var tx = await _context.Database.BeginTransactionAsync(IsolationLevel.Serializable);
            var payment = await _context.Payments.Include(p => p.Invoice).FirstOrDefaultAsync(p => p.PaymentId == id);
            if (payment is null) return false;
            if (payment.Status != PaymentEnums.Pending) throw new BadRequestException("Chỉ được xác nhận giao dịch đang chờ xử lý!");
            if (payment.PaymentType != PaymentTypeEnums.Refund) await ValidateAmountAsync(payment);
            else await ValidateAmountAsync(payment);
            var oldStatus = payment.Status;
            payment.Status = PaymentEnums.Success;
            payment.PaymentDate = DateTime.Now;
            if (payment.CreatedByUserId is null) payment.CreatedByUserId = staffUserId;
            await _context.SaveChangesAsync();

            var bookingId = payment.BookingId ?? payment.Invoice?.BookingId;
            if (bookingId.HasValue && payment.InvoiceSnapshotJson is not null && payment.PaymentType == PaymentTypeEnums.Payment)
                await _invoiceService.CreateFromSnapshotAsync(bookingId.Value, payment.InvoiceSnapshotJson, staffUserId);
            else if (payment.InvoiceId.HasValue)
                await UpdateInvoiceStatusAsync(payment.InvoiceId.Value);

            await _auditLogService.LogAsync(AuditLogEnums.Confirm, "Payment", payment.PaymentId,
                $"Status: {oldStatus}", $"Status: {payment.Status}");
            await tx.CommitAsync();
            return true;
        }

        private async Task UpdateInvoiceStatusAsync(int invoiceId)
        {
            var invoice = await _context.Invoices.FirstOrDefaultAsync(i => i.InvoiceId == invoiceId);
            if (invoice is null) return;
            var paid = await _context.Payments.Where(p => p.InvoiceId == invoiceId && p.Status == PaymentEnums.Success)
                .Select(p => p.PaymentType == PaymentTypeEnums.Refund ? -p.Amount : p.Amount).SumAsync();
            invoice.Status = paid <= 0 ? InvoiceEnums.Unpaid : paid < invoice.AmountDue ? InvoiceEnums.PartiallyPaid : InvoiceEnums.Paid;
            await _context.SaveChangesAsync();
        }

        public async Task<bool?> RejectAsync(int id)
        {
            var payment = await _context.Payments.FindAsync(id);
            if (payment is null) return false;
            if (payment.Status != PaymentEnums.Pending) throw new BadRequestException("Chỉ được từ chối giao dịch đang chờ xử lý!");
            payment.Status = PaymentEnums.Failed;
            await _context.SaveChangesAsync();
            await _auditLogService.LogAsync(AuditLogEnums.Update, "Payment", payment.PaymentId, "Status: Pending", "Status: Failed");
            return true;
        }
    }
}
