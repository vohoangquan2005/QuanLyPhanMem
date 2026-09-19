using HotelManagementSystem.Data;
using HotelManagementSystem.Models;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
        // Update
        public async Task<BookingResponseDto?> UpdateAsync(int id, BookingUpdateDto dto)
        {
            await using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var booking = await _context.Bookings
                                            .Include(b => b.BookingDetails)
                                            .FirstOrDefaultAsync(b => b.BookingId == id
                                                                   && b.IsDeleted == ActiveEnums.Active);
                if (booking is null) return null;
                // Chỉ cho phép sửa các Booking đang Pending
                if (booking.Status == BookingEnums.Pending) // BUG 9: Điều kiện trạng thái bị đảo ngược 
                    throw new BadRequestException("Chỉ có thể cập nhật Booking đang ở trạng thái Pending!"); 
                if (await _context.Payments.AnyAsync(p => p.Status == PaymentEnums.Success &&
                    (p.BookingId == booking.BookingId || p.Invoice!.BookingId == booking.BookingId)))
                    throw new BadRequestException("Không thể thay đổi chi phí đặt phòng sau khi đã thu tiền cọc!");
                // Không cho update Booking đã hủy đặt phòng
                if (booking.Status == BookingEnums.Cancelled)
                    throw new BadRequestException("Không thể cập nhật Booking đã hủy!");
                // Lưu thông tin khuyến mãi và gói dịch vụ cũ
                var oldPromotionId = booking.PromotionId;
                // Validate
                await CheckPromotionId(dto.PromotionId);
                await CheckServicePackageId(dto.ServicePackageId);
                CheckDate(dto.ExpectedCheckIn, dto.ExpectedCheckOut);
                // Kiểm tra số lượng phòng
                var currentDetailCount = booking.BookingDetails.Count;
                if (dto.RoomCount != currentDetailCount) 
                    throw new BadRequestException("Không thể thay đổi số lượng phòng khi cập nhật Booking. " + "Hãy cập nhật BookingDetail tương ứng!");
                // Kiểm tra tổng số khách
                var currentGuestCount = booking.BookingDetails.Sum(bd => bd.GuestCount);
                if (dto.GuestCount != currentGuestCount)
                    throw new BadRequestException("Số lượng khách không khớp với chi tiết đặt phòng!");
                // Check cho tất cả phòng còn trống
                foreach (var detail in booking.BookingDetails)
                {
                    await CheckRoomAvailable(detail.RoomId, dto.ExpectedCheckIn, dto.ExpectedCheckOut, booking.BookingId);
                }
                // Tính lại các khoản tiền
                // Tính RoomAmount
                var listBookingDetails = await _context.BookingDetails
                                                       .Where(bd => bd.BookingId == booking.BookingId)
                                                       .Include(bd => bd.Room)
                                                       .ToListAsync();
                var roomAmount = CalculateRoomAmount(listBookingDetails, dto.ExpectedCheckIn, dto.ExpectedCheckOut);
                var (newPromotion, discountAmount) = await SelectPromotionAsync(
                    dto.PromotionId, listBookingDetails, dto.ExpectedCheckIn, dto.ExpectedCheckOut, oldPromotionId);
                // Tính tiền ServicePackage
                var packageAmount = await CalculatePackageAmount(dto.ServicePackageId);
                // Tiền dịch vụ đặt thêm ban đầu = 0
                var additionalServiceAmount = booking.AdditionalServiceAmount;
                // Tính tổng tiền
                var totalAmount = CalculateTotalAmount(roomAmount, packageAmount, additionalServiceAmount, discountAmount);
                // Gán lại các khoản tiền
                booking.RoomAmount = roomAmount;
                booking.PackageAmount = packageAmount;
                booking.DiscountAmount = discountAmount;
                booking.AdditionalServiceAmount = additionalServiceAmount;
                booking.TotalAmount = totalAmount;
                var depositPercentage = _configuration.GetValue<decimal>("BookingSettings:DepositPercentage", 20m);
                if (depositPercentage <= 0 || depositPercentage > 100)
                    throw new BadRequestException("BookingSettings:DepositPercentage phải nằm trong khoảng lớn hơn 0 đến 100!");
                booking.DepositAmount = Math.Round(totalAmount * depositPercentage / 100m, 2, MidpointRounding.AwayFromZero);
                // Xử lý UsedCount của Promotion
                booking.PromotionId = newPromotion?.PromotionId;
                booking.PromotionNameSnapshot = newPromotion?.PromotionName;
                booking.PromotionDiscountTypeSnapshot = newPromotion?.DiscountType;
                booking.PromotionDiscountValueSnapshot = newPromotion?.DiscountValue;
                if (oldPromotionId != booking.PromotionId)
                {
                    if (oldPromotionId.HasValue)
                    {
                        var oldPromotion = await _context.Promotions
                                                         .FirstOrDefaultAsync(p => p.PromotionId == oldPromotionId.Value);
                        if (oldPromotion is not null && oldPromotion.UsedCount.HasValue)
                        {
                            oldPromotion.UsedCount--;
                            if (oldPromotion.UsedCount < 0)
                                oldPromotion.UsedCount = 0;
                        }
                    }
                    if (newPromotion is not null)
                        IncreasePromotionUsedCount(newPromotion);
                }
                var oldValue = $"ExpectedCheckIn: {booking.ExpectedCheckIn}, " +
                               $"ExpectedCheckOut: {booking.ExpectedCheckOut}, " +
                               $"GuestCount: {booking.GuestCount}, " +
                               $"RoomCount: {booking.RoomCount}, " +
                               $"PromotionId: {booking.PromotionId}, " +
                               $"ServicePackageId: {booking.ServicePackageId}, " +
                               $"TotalAmount: {booking.TotalAmount}";
                UpdateEntity(booking, dto);
                booking.PromotionId = newPromotion?.PromotionId;
                booking.PromotionNameSnapshot = newPromotion?.PromotionName;
                booking.PromotionDiscountTypeSnapshot = newPromotion?.DiscountType;
                booking.PromotionDiscountValueSnapshot = newPromotion?.DiscountValue;
                await _context.SaveChangesAsync();
                await _paymentService.RefreshPendingDepositAsync(booking.BookingId, booking.DepositAmount);
                var newValue = $"ExpectedCheckIn: {booking.ExpectedCheckIn}, " +
                               $"ExpectedCheckOut: {booking.ExpectedCheckOut}, " +
                               $"GuestCount: {booking.GuestCount}, " +
                               $"RoomCount: {booking.RoomCount}, " +
                               $"PromotionId: {booking.PromotionId}, " +
                               $"ServicePackageId: {booking.ServicePackageId}, " +
                               $"TotalAmount: {booking.TotalAmount}";

                await _auditLogService.LogAsync(AuditLogEnums.Update,
                                                "Booking",
                                                booking.BookingId,
                                                oldValue,
                                                newValue);
                await transaction.CommitAsync();
                return await GetByIdAsync(id);
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }