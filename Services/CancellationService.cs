using HotelManagement.Data;
using HotelManagement.DTOs.Cancellation;
using HotelManagement.Enums;
using HotelManagement.Exceptions;
using HotelManagement.Models;
using Microsoft.EntityFrameworkCore;

namespace HotelManagement.Services
{
    public class CancellationService
    {
        private readonly AppDbContext _context;
        private readonly AuditLogService _auditLogService;
        private readonly PaymentService _paymentService;
        public CancellationService(AppDbContext context, AuditLogService auditLogService, PaymentService paymentService)
        {
            _context = context;
            _auditLogService = auditLogService;
            _paymentService = paymentService;
        }
        // Chuyển Cancellation -> ResponseDto
        private CancellationResponseDto MapToResponseDto(Cancellation cancellation)
        {
            return new CancellationResponseDto
            {
                CancellationId = cancellation.CancellationId,
                BookingId = cancellation.BookingId,
                CancellationPolicyId = cancellation.CancellationPolicyId,
                Reason = cancellation.Reason,
                RefundPercentage = cancellation.RefundPercentage,
                RefundAmount = cancellation.RefundAmount,
                Status = cancellation.Status,
                CreateAt = cancellation.CreateAt
            };
        }
        // Chuyển CancellationCreateDto -> Cancellation
        private Cancellation MapToEntity(CancellationCreateDto dto, CancellationPolicy policy, decimal refundAmount)
        {
            return new Cancellation
            {
                BookingId = dto.BookingId,
                // Policy được tự động xác định
                CancellationPolicyId = policy.CancellationPolicyId,
                Reason = dto.Reason,
                // Lưu phần trăm hoàn trả tại thời điểm hủy đặt phòng
                RefundPercentage = policy.RefundPercentage,
                RefundAmount = refundAmount,
                Status = CancellationEnums.Pending
            };
        }
        // ===========================================================
        // =                       VALIDATION                       =
        // ===========================================================
        // Check BookingId có tồn tại trong bảng Booking không?
        private async Task<Booking> CheckBookingId(int bookingId)
        {
            var booking = await _context.Bookings.FirstOrDefaultAsync(b => b.BookingId == bookingId
                                                                        && b.IsDeleted == ActiveEnums.Active);
            if (booking is null)
                throw new NotFoundException("Mã đặt phòng không hợp lệ!");
            return booking;
        }
        // Check Booking Status
        private void CheckBookingStatus(Booking booking)
        {
            if (booking.Status == BookingEnums.Cancelled)
                throw new BadRequestException("Đặt phòng đã được hủy!");
            if (booking.Status == BookingEnums.Completed)
                throw new BadRequestException("Đặt phòng đã hoàn thành, không thể hủy!");
            if (booking.Status == BookingEnums.CheckedIn)
                throw new BadRequestException("Đặt phòng đã nhận phòng, không thể hủy!");
            // Chỉ cho phép hủy Booking đã được xác nhận
            if (booking.Status != BookingEnums.Confirmed)
                throw new BadRequestException("Chỉ có thể hủy đặt phòng đã được xác nhận!");
        }
        // Check BookingId đã có yêu cầu hủy phòng chưa?
        private async Task CheckCancellationExists(int bookingId)
        {
            var exists = await _context.Cancellations.AnyAsync(c => c.BookingId == bookingId
                                                                 && c.Status != CancellationEnums.Rejected);
            if (exists)
                throw new ConflictException("Đặt phòng này đã có yêu cầu hủy!");
        }
        // ===========================================================
        // =                     XỬ LÝ CHÍNH SÁCH                    =
        // ===========================================================
        // Tính thời điểm kết thúc của một chính sách
        private DateTime CalculatePolicyDeadline(
            DateTime bookingDate,
            CancellationPolicy policy)
        {
            return policy.CancellationTimeUnit switch
            {
                CancellationTimeUnitEnums.Hour => bookingDate.AddHours(policy.CancellationTime),
                CancellationTimeUnitEnums.Day => bookingDate.AddDays(policy.CancellationTime),
                CancellationTimeUnitEnums.Week => bookingDate.AddDays(policy.CancellationTime * 7),
                CancellationTimeUnitEnums.Month => bookingDate.AddMonths(policy.CancellationTime),
                _ => throw new BadRequestException("Đơn vị thời gian của chính sách không hợp lệ!")
            };
        }
        // Tìm chính sách thích hợp
        private async Task<CancellationPolicy> FindCancellationPolicy( Booking booking)
        {
            var now = DateTime.Now;
            // Chỉ lấy Policy đang Active
            var policies = await _context.CancellationPolicies
                                         .Where(cp => cp.IsDeleted == ActiveEnums.Active)
                                         .ToListAsync();
            if (!policies.Any())
                throw new BadRequestException("Hiện tại không có chính sách hủy đang hoạt động!");
            // Tìm các policy mà thời gian hoàn trả còn đáp ứng
            var applicablePolicies = policies.Select(policy => new
                                             {
                                                Policy = policy,
                                                Deadline = CalculatePolicyDeadline(booking.BookingDate, policy)
                                             })
                                             .Where(x => x.Deadline >= now)
                                             .OrderBy(x => x.Deadline)
                                             .ToList();
            if (!applicablePolicies.Any())
                throw new BadRequestException("Đã quá thời gian cho phép hủy đặt phòng!");
            // Lấy chính sách đáp ứng
            // Ví dụ hoàn trả sau 5 ngày thì sẽ lấy được chính sách 1 tuần, 3 ngày và 24 giờ
            // Nhưng chỉ được áp dụng chính sách 1 tuần
            return applicablePolicies.First().Policy;
        }
        // Tính số tiền hoàn trả
        private decimal CalculateRefundAmount(decimal depositAmount, decimal refundPercentage)
        {
            return depositAmount * refundPercentage / 100;
        }
        // ===========================================================
        // =                           CRUD                          =
        // ===========================================================

        // Get all
        public async Task<List<CancellationResponseDto>> GetAllAsync()
        {
            var cancellations = await _context.Cancellations
                                              .Select(c => new CancellationResponseDto
                                              {
                                                    CancellationId = c.CancellationId,
                                                    BookingId = c.BookingId,
                                                    CancellationPolicyId = c.CancellationPolicyId,
                                                    Reason = c.Reason,
                                                    RefundPercentage = c.RefundPercentage,
                                                    RefundAmount = c.RefundAmount,
                                                    Status = c.Status,
                                                    CreateAt = c.CreateAt
                                              })
                                              .ToListAsync();
            return cancellations;
        }
        // Get by Id
        public async Task<CancellationResponseDto?> GetByIdAsync(int id)
        {
            var cancellation = await _context.Cancellations.FirstOrDefaultAsync(c => c.CancellationId == id);
            if (cancellation is null) return null;
            return MapToResponseDto(cancellation);
        }
        // Create
        public Task<CancellationResponseDto> CreateAsync(CancellationCreateDto dto) => CreateAsync(dto, null);

        public async Task<CancellationResponseDto> CreateForCustomerAsync(CancellationCreateDto dto, int customerId) =>
            await CreateAsync(dto, customerId);

        private async Task<CancellationResponseDto> CreateAsync(CancellationCreateDto dto, int? ownerCustomerId)
        {
            // Validate
            var booking = await CheckBookingId(dto.BookingId);
            if (ownerCustomerId.HasValue && booking.CustomerId != ownerCustomerId.Value)
                throw new NotFoundException("Không tìm thấy đặt phòng!");
            CheckBookingStatus(booking);
            await CheckCancellationExists(dto.BookingId);
            // Tự động tìm policy phù hợp
            var policy = await FindCancellationPolicy(booking);
            // Tính tiền hoàn trả
            var refundAmount = CalculateRefundAmount(booking.DepositAmount, policy.RefundPercentage);
            // Tạo Cancellation
            var cancellation = MapToEntity(dto, policy, refundAmount);
            await _context.Cancellations.AddAsync(cancellation);
            await _context.SaveChangesAsync();
            await _auditLogService.LogAsync(AuditLogEnums.Create,
                                            "Cancellation",
                                            cancellation.CancellationId,
                                            null,
                                            $"BookingId: {cancellation.BookingId}, " +
                                            $"CancellationPolicyId: {cancellation.CancellationPolicyId}, " +
                                            $"RefundPercentage: {cancellation.RefundPercentage}%, " +
                                            $"RefundAmount: {cancellation.RefundAmount}, " +
                                            $"Status: {cancellation.Status}");
            return MapToResponseDto(cancellation);
        }
        // Update
        public async Task<CancellationResponseDto?> UpdateAsync(int id, CancellationUpdateDto dto)
        {
            var cancellation = await _context.Cancellations.FirstOrDefaultAsync(c => c.CancellationId == id);
            if (cancellation is null) return null;
            if (cancellation.Status != CancellationEnums.Pending)
                throw new BadRequestException("Chỉ có thể cập nhật yêu cầu hủy đang chờ xử lý!");
            var oldValue = $"Reason: {cancellation.Reason}";
            cancellation.Reason = dto.Reason;
            await _context.SaveChangesAsync();
            await _auditLogService.LogAsync(
                AuditLogEnums.Update,
                "Cancellation",
                cancellation.CancellationId,
                oldValue,
                $"Reason: {cancellation.Reason}");
            return MapToResponseDto(cancellation);
        }
        // Delete
        public async Task<bool> DeleteAsync(int id)
        {
            var cancellation = await _context.Cancellations.FirstOrDefaultAsync(c => c.CancellationId == id);
            if (cancellation is null) return false;
            if (cancellation.Status != CancellationEnums.Pending)
                throw new BadRequestException("Chỉ có thể xóa yêu cầu hủy đang chờ xử lý!");
            _context.Cancellations.Remove(cancellation);
            await _context.SaveChangesAsync();
            return true;
        }
        // Xác nhận hủy đặt phòng
        public async Task<bool> ConfirmAsync(int id)
        {
            await using var transaction = await _context.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable);
            try
            {
                // Lấy Cancellation + Booking
                var cancellation = await _context.Cancellations
                                                 .Include(c => c.Booking)
                                                 .FirstOrDefaultAsync(c => c.CancellationId == id);    
                if (cancellation is null) return false;
                // Chỉ xác nhận những Cancellation đang Pending
                if (cancellation.Status != CancellationEnums.Pending)
                    throw new BadRequestException("Chỉ có thể xác nhận yêu cầu hủy đặt phòng đang chờ xử lý!");
                if (cancellation.Booking is null)
                    throw new NotFoundException("Không tìm thấy đặt phòng của yêu cầu hủy!");
                var booking = cancellation.Booking;
                var oldCancellationStatus = cancellation.Status;
                CheckBookingStatus(booking);
                // Xác nhận hủy đặt phòng
                booking.Status = BookingEnums.Cancelled;
                cancellation.Status = CancellationEnums.Approved;
                await _context.SaveChangesAsync();
                await _paymentService.CancelPendingPaymentsAsync(booking.BookingId);
                await _paymentService.CreateRefundAsync(booking.BookingId, cancellation.RefundAmount,
                    $"cancellation:{cancellation.CancellationId}:refund", "CancellationPolicy");
                await _auditLogService.LogAsync(AuditLogEnums.Confirm,
                                                "Cancellation",
                                                cancellation.CancellationId,
                                                $"Status: {oldCancellationStatus}",
                                                $"Status: {cancellation.Status}");
                await transaction.CommitAsync();
                return true;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
        // Từ chối hủy đặt phòng
        public async Task<bool> RejectAsync(int id)
        {
            var cancellation = await _context.Cancellations
                                                 .FirstOrDefaultAsync(c => c.CancellationId == id);
            if (cancellation is null) return false;
            // Chỉ xác nhận những Cancellation đang Pending
            if (cancellation.Status != CancellationEnums.Pending)
                throw new BadRequestException("Chỉ có thể từ chối yêu cầu hủy đặt phòng đang chờ xử lý!");
            var oldStatus = cancellation.Status;
            cancellation.Status = CancellationEnums.Rejected;
            await _context.SaveChangesAsync();
            await _auditLogService.LogAsync(AuditLogEnums.Update,
                                            "Cancellation",
                                            cancellation.CancellationId,
                                            $"Status: {oldStatus}",
                                            $"Status: {cancellation.Status}");
            return true;
        }
    }
}
