using Model;
using System;

// Auto delete (Xóa tất cả những booking không hoạt động
        public async Task<int> AutoSoftDeleteExpiredAsync()
        {
            var now = DateTime.Now;
            var bookings = await _context.Bookings
                                         .Where(b => b.IsDeleted == ActiveEnums.Active
                                                     && b.ExpectedCheckOut < now
                                                     && b.Status != BookingEnums.Cancelled
                                                     && b.Status != BookingEnums.Rejected)
                                         .ToListAsync();
            if (!bookings.Any()) return 0;
            foreach (var booking in bookings)
            {
                booking.Status = BookingEnums.Completed;
                booking.IsDeleted = ActiveEnums.Inactive;
            }
            await _context.SaveChangesAsync();
            return bookings.Count;
        }
        // Restore
        public async Task<bool> RestoreAsync(int id)
        {
            await using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var booking = await _context.Bookings
                                            .Include(b => b.BookingDetails)
                                            .FirstOrDefaultAsync(b => b.BookingId == id
                                                                   && b.IsDeleted == ActiveEnums.Inactive);
                if (booking is null) return false;
                // Không khôi phục Booking đã bị hủy
                if (booking.Status == BookingEnums.Cancelled)
                    throw new BadRequestException("Không thể khôi phục Booking đã bị hủy!");
                // Kiểm tra tất cả phòng còn trống
                foreach (var detail in booking.BookingDetails)
                {
                    await CheckRoomAvailable(
                        detail.RoomId,
                        booking.ExpectedCheckIn,
                        booking.ExpectedCheckOut,
                        booking.BookingId);
                    await _auditLogService.LogAsync(AuditLogEnums.SoftDelete,
                                                    "Booking",
                                                    booking.BookingId,
                                                    "Status: Active, IsDeleted: Active",
                                                    "Status: Completed, IsDeleted: Inactive");
                }
                // Khôi phục Booking
                booking.IsDeleted = ActiveEnums.Active;
                booking.UpdateAt = DateTime.Now;
                await _context.SaveChangesAsync();
                await _auditLogService.LogAsync(AuditLogEnums.Restore,
                                                "Booking",
                                                booking.BookingId,
                                                "IsDeleted: Inactive",
                                                "IsDeleted: Active");
                await transaction.CommitAsync();
                return true;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
        // Xác nhận đặt phòng
        public async Task<BookingResponseDto?> ConfirmAsync(int id)
        {
            var booking = await _context.Bookings
                                        .FirstOrDefaultAsync(b => b.BookingId == id
                                                               && b.IsDeleted == ActiveEnums.Active);
            if (booking is null) return null;
            if (booking.Status != BookingEnums.Pending)
                throw new BadRequestException("Chỉ có thể xác nhận đặt phòng đang chờ xử lý!");
            booking.Status = BookingEnums.Rejected; // BUG 8: Xác nhận nhưng lại chuyển sang từ chối
            booking.UpdateAt = DateTime.Now;
            await _context.SaveChangesAsync();
            await _auditLogService.LogAsync(AuditLogEnums.Confirm,
                                            "Booking",
                                            booking.BookingId,
                                            "Status: Pending",
                                            "Status: Confirmed");
            return await GetByIdAsync(id);
        }

        public async Task<bool> RejectAsync(int id)
        {
            await using var transaction = await _context.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable);
            var booking = await _context.Bookings.FirstOrDefaultAsync(b => b.BookingId == id && b.IsDeleted == ActiveEnums.Active);
            if (booking is null) return false;
            if (booking.Status != BookingEnums.Pending)
                throw new BadRequestException("Chỉ có thể từ chối đặt phòng đang chờ xử lý!");
            booking.Status = BookingEnums.Rejected;
            booking.UpdateAt = DateTime.Now;
            await _context.SaveChangesAsync();
            await _paymentService.CancelPendingPaymentsAsync(booking.BookingId);
            await _paymentService.CreateRefundAsync(booking.BookingId, booking.DepositAmount,
                $"booking:{booking.BookingId}:rejection-refund", "BookingRejected");
            await _auditLogService.LogAsync(AuditLogEnums.Update, "Booking", booking.BookingId,
                "Status: Pending", "Status: Rejected");
            await transaction.CommitAsync();
            return true;

            // Auto delete (Xóa tất cả những booking không hoạt động
        public async Task<int> AutoSoftDeleteExpiredAsync()
        {
            var now = DateTime.Now;
            var bookings = await _context.Bookings
                                         .Where(b => b.IsDeleted == ActiveEnums.Active
                                                     && b.ExpectedCheckOut < now
                                                     && b.Status != BookingEnums.Cancelled
                                                     && b.Status != BookingEnums.Rejected)
                                         .ToListAsync();
            if (!bookings.Any()) return 0;
            foreach (var booking in bookings)
            {
                booking.Status = BookingEnums.Completed;
                booking.IsDeleted = ActiveEnums.Inactive;
            }
            await _context.SaveChangesAsync();
            return bookings.Count;
        }
        // Restore
        public async Task<bool> RestoreAsync(int id)
        {
            await using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var booking = await _context.Bookings
                                            .Include(b => b.BookingDetails)
                                            .FirstOrDefaultAsync(b => b.BookingId == id
                                                                   && b.IsDeleted == ActiveEnums.Inactive);
                if (booking is null) return false;
                // Không khôi phục Booking đã bị hủy
                if (booking.Status == BookingEnums.Cancelled)
                    throw new BadRequestException("Không thể khôi phục Booking đã bị hủy!");
                // Kiểm tra tất cả phòng còn trống
                foreach (var detail in booking.BookingDetails)
                {
                    await CheckRoomAvailable(
                        detail.RoomId,
                        booking.ExpectedCheckIn,
                        booking.ExpectedCheckOut,
                        booking.BookingId);
                    await _auditLogService.LogAsync(AuditLogEnums.SoftDelete,
                                                    "Booking",
                                                    booking.BookingId,
                                                    "Status: Active, IsDeleted: Active",
                                                    "Status: Completed, IsDeleted: Inactive");
                }
                // Khôi phục Booking
                booking.IsDeleted = ActiveEnums.Active;
                booking.UpdateAt = DateTime.Now;
                await _context.SaveChangesAsync();
                await _auditLogService.LogAsync(AuditLogEnums.Restore,
                                                "Booking",
                                                booking.BookingId,
                                                "IsDeleted: Inactive",
                                                "IsDeleted: Active");
                await transaction.CommitAsync();
                return true;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
        // Xác nhận đặt phòng
        public async Task<BookingResponseDto?> ConfirmAsync(int id)
        {
            var booking = await _context.Bookings
                                        .FirstOrDefaultAsync(b => b.BookingId == id
                                                               && b.IsDeleted == ActiveEnums.Active);
            if (booking is null) return null;
            if (booking.Status != BookingEnums.Pending)
                throw new BadRequestException("Chỉ có thể xác nhận đặt phòng đang chờ xử lý!");
            booking.Status = BookingEnums.Rejected; // BUG 8: Xác nhận nhưng lại chuyển sang từ chối
            booking.UpdateAt = DateTime.Now;
            await _context.SaveChangesAsync();
            await _auditLogService.LogAsync(AuditLogEnums.Confirm,
                                            "Booking",
                                            booking.BookingId,
                                            "Status: Pending",
                                            "Status: Confirmed");
            return await GetByIdAsync(id);
        }

        public async Task<bool> RejectAsync(int id)
        {
            await using var transaction = await _context.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable);
            var booking = await _context.Bookings.FirstOrDefaultAsync(b => b.BookingId == id && b.IsDeleted == ActiveEnums.Active);
            if (booking is null) return false;
            if (booking.Status != BookingEnums.Pending)
                throw new BadRequestException("Chỉ có thể từ chối đặt phòng đang chờ xử lý!");
            booking.Status = BookingEnums.Rejected;
            booking.UpdateAt = DateTime.Now;
            await _context.SaveChangesAsync();
            await _paymentService.CancelPendingPaymentsAsync(booking.BookingId);
            await _paymentService.CreateRefundAsync(booking.BookingId, booking.DepositAmount,
                $"booking:{booking.BookingId}:rejection-refund", "BookingRejected");
            await _auditLogService.LogAsync(AuditLogEnums.Update, "Booking", booking.BookingId,
                "Status: Pending", "Status: Rejected");
            await transaction.CommitAsync();
            return true;