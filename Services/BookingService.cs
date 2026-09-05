using HotelManagement.Data;
using HotelManagement.DTOs.Booking;
using HotelManagement.DTOs.BookingDetail;
using HotelManagement.DTOs.BookingService;
using HotelManagement.Enums;
using HotelManagement.Exceptions;
using HotelManagement.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace HotelManagement.Services
{
    public class BookingService
    {
        private readonly AppDbContext _context;
        private readonly AuditLogService _auditLogService;
        private readonly PaymentService _paymentService;
        private readonly IConfiguration _configuration;
        public BookingService(AppDbContext context, AuditLogService auditLogService, PaymentService paymentService, IConfiguration configuration)
        {
            _context = context;
            _auditLogService = auditLogService;
            _paymentService = paymentService;
            _configuration = configuration;
        }
        // ===========================================================
        // =                        MAPPING                          =
        // ===========================================================
        // Chuyển Booking -> ResponseDto
        private BookingResponseDto MapToResponseDto(Booking booking)
        {
            return new BookingResponseDto
            {
                BookingId = booking.BookingId,
                // Thông tin Customer
                CustomerId = booking.CustomerId,
                FullName = booking.Customer?.User?.FullName,
                Email = booking.Customer?.User?.Email,
                PhoneNumber = booking.Customer?.User?.PhoneNumber,
                // Thông tin Booking
                BookingDate = booking.BookingDate,
                ExpectedCheckIn = booking.ExpectedCheckIn,
                ExpectedCheckOut = booking.ExpectedCheckOut,
                GuestCount = booking.GuestCount,
                RoomCount = booking.RoomCount,
                // Thông tin Promotion
                PromotionId = booking.PromotionId,
                PromotionCode = booking.Promotion?.PromotionCode,
                PromotionName = booking.PromotionNameSnapshot ?? booking.Promotion?.PromotionName,
                DiscountType = booking.PromotionDiscountTypeSnapshot ?? booking.Promotion?.DiscountType,
                DiscountValue = booking.PromotionDiscountValueSnapshot ?? booking.Promotion?.DiscountValue,
                // Thông tin ServicePackage
                ServicePackageId = booking.ServicePackageId,
                PackageName = booking.ServicePackage?.PackageName,
                // Thông tin tiền
                DepositAmount = booking.DepositAmount,
                RoomAmount = booking.RoomAmount,
                PackageAmount = booking.PackageAmount,
                DiscountAmount = booking.DiscountAmount,
                AdditionalServiceAmount = booking.AdditionalServiceAmount,
                TotalAmount = booking.TotalAmount,
                Note = booking.Note,
                Status = booking.Status,
                IsDeleted = booking.IsDeleted,
                CreateAt = booking.CreateAt,
                UpdateAt = booking.UpdateAt,
                // Booking detail
                BookingDetails = booking.BookingDetails
                                        .Select(bd => new BookingDetailResponseDto
                                        {
                                            BookingDetailId = bd.BookingDetailId,
                                            BookingId = bd.BookingId,
                                            RoomId = bd.RoomId,
                                            RoomName = bd.Room?.RoomName,
                                            RoomTypeName = bd.Room?.RoomType?.RoomTypeName,
                                            ActualCheckIn = bd.ActualCheckIn,
                                            ActualCheckOut = bd.ActualCheckOut,
                                            PricePerNight = bd.PricePerNight,
                                            TotalAmount = bd.TotalAmount,
                                            GuestCount = bd.GuestCount,
                                            CreateAt = bd.CreateAt
                                        })
                                        .ToList(),
                // Booking service
                BookingServices = booking.BookingServices
                                         .Select(bs => new BookingServiceResponseDto
                                         {
                                            BookingServiceId = bs.BookingServiceId,
                                            BookingId = bs.BookingId,
                                            ServiceId = bs.ServiceId,
                                            ServiceName = bs.Service?.ServiceName,
                                            Quantity = bs.Quantity,
                                            UnitPrice = bs.UnitPrice,
                                            TotalAmount = bs.TotalAmount,
                                            CreateAt = bs.CreateAt
                                         })
                                         .ToList()
            };
        }
        // Chuyển BookingCreateDto -> Booking
        private Booking MapToEntity(BookingCreateDto dto)
        {
            return new Booking
            {
                CustomerId = dto.CustomerId,
                BookingDate = DateTime.Now,
                ExpectedCheckIn = dto.ExpectedCheckIn,
                ExpectedCheckOut = dto.ExpectedCheckOut,
                GuestCount = dto.GuestCount,
                RoomCount = dto.RoomCount,
                PromotionId = dto.PromotionId,
                ServicePackageId = dto.ServicePackageId,
                Note = dto.Note,
            };
        }
        // Chuyển BookingCreateOnlineDto -> Booking
        private Booking MapToEntityOnline(BookingCreateOnlineDto dto, int customerId)
        {
            return new Booking
            {
                CustomerId = customerId,
                BookingDate = DateTime.Now,
                ExpectedCheckIn = dto.ExpectedCheckIn,
                ExpectedCheckOut = dto.ExpectedCheckOut,
                GuestCount = dto.GuestCount,
                RoomCount = dto.RoomCount,
                PromotionId = dto.PromotionId,
                ServicePackageId = dto.ServicePackageId,
                Note = dto.Note,
            };
        }
        // Update Booking từ BookingUpdateDto
        private void UpdateEntity(Booking booking, BookingUpdateDto dto)
        {
            booking.ExpectedCheckIn = dto.ExpectedCheckIn;
            booking.ExpectedCheckOut = dto.ExpectedCheckOut;
            booking.GuestCount = dto.GuestCount;
            booking.RoomCount = dto.RoomCount;
            booking.PromotionId = dto.PromotionId;
            booking.ServicePackageId = dto.ServicePackageId;
            booking.Note = dto.Note;
        }
        // ===========================================================
        // =                       VALIDATION                        =
        // ===========================================================
        // Check CustomerId có tồn tại trong bảng Customer không?
        private async Task CheckCustomerId(int customerId)
        {
            var exists = await _context.Customers.AnyAsync(c => c.CustomerId == customerId);
            if (!exists)
                throw new NotFoundException("Mã khách hàng không tồn tại!");
        }
        // Check PromotionId có tồn tại trong bảng Promotion không?
        private async Task CheckPromotionId(int? promotionId)
        {
            if (promotionId is null) return;
            var exists = await _context.Promotions.AnyAsync(p => p.PromotionId == promotionId
                                                              && p.IsDeleted == ActiveEnums.Active);
            if (!exists)
                throw new NotFoundException("Mã khuyến mãi không tồn tại!");
        }
        // Check ServicePackageId có tồn tại trong bảng ServicePackage không?
        private async Task CheckServicePackageId(int? servicePackageId)
        {
            if (servicePackageId is null) return;
            var exists = await _context.ServicePackages.AnyAsync(sp => sp.ServicePackageId == servicePackageId
                                                                    &&(sp.IsDeleted == ActiveEnums.Active));
            if (!exists)
                throw new NotFoundException("Gói dịch vụ không tồn tại!");
        }
        // Check Date
        private void CheckDate(DateTime expectedCheckIn, DateTime expectedCheckOut)
        {
            if (expectedCheckIn >= expectedCheckOut)
                throw new BadRequestException("Ngày trả phòng phải sau ngày nhận phòng!");
            if (expectedCheckIn < DateTime.UtcNow)
                throw new BadRequestException("Ngày nhận phòng không được ở trong quá khứ!");
        }
        // ===========================================================
        // =                  CHECK BOOKING DETAILS                  =
        // ===========================================================
        // Check danh sách phòng trong BookingDetail
        private async Task CheckBookingDetails(List<BookingDetailCreateDto> bookingDetails, int roomCount,int guestCount)
        {
            // Phải có ít nhất một phòng
            if (bookingDetails is null || !bookingDetails.Any())
                throw new BadRequestException("Booking phải có ít nhất một phòng!");
            // Số lượng phòng phải khớp với số BookingDetail
            if (bookingDetails.Count != roomCount)
                throw new BadRequestException(
                    "Số lượng phòng không khớp với danh sách phòng!");
            // Không cho phép cùng một phòng xuất hiện nhiều lần
            var duplicateRoom = bookingDetails.GroupBy(bd => bd.RoomId).Any(g => g.Count() > 1);
            if (duplicateRoom)
                throw new ConflictException("Không được chọn cùng một phòng nhiều lần!");
            // Tổng số khách trong các phòng
            var totalGuestCount = bookingDetails.Sum(bd => bd.GuestCount);
            if (totalGuestCount != guestCount)
                throw new BadRequestException("Tổng số khách trong các phòng không khớp với số lượng khách của Booking!");
            // Kiểm tra từng phòng
            foreach (var detail in bookingDetails)
            {
                var room = await _context.Rooms.FirstOrDefaultAsync(r => r.RoomId == detail.RoomId);
                if (room is null)
                    throw new NotFoundException($"Phòng {detail.RoomId} không tồn tại!");
                if (detail.GuestCount > room.Capacity) 
                    throw new BadRequestException($"Phòng {detail.RoomId} chỉ có sức chứa {room.Capacity} khách!"); 
            }
        }
        // ===========================================================
        // =                    CHECK ROOM AVAILABLE                 =
        // ===========================================================
        // Check phòng có được yêu cầu bởi 1 Booking khác trong khoảng thời gian ExpectedCheckIn -> ExpectedCheckOut hay không?
        private async Task CheckRoomAvailable(int roomId, DateTime expectedCheckIn, DateTime expectedCheckOut, int? bookingId = null)
        {
            var query = _context.BookingDetails
                                              .Where(bd => bd.RoomId == roomId
                                                        && bd.Booking!.IsDeleted == ActiveEnums.Active
                                                        && bd.Booking!.Status != BookingEnums.Cancelled
                                                        && bd.Booking.Status != BookingEnums.Rejected
                                                        && bd.Booking.ExpectedCheckIn < expectedCheckOut
                                                        && bd.Booking.ExpectedCheckOut > expectedCheckIn);
            // Nếu Update thì bỏ qua Booking hiện tại
            if (bookingId.HasValue)
                query = query.Where(bd => bd.BookingId != bookingId.Value);
            var bookingExists = await query.AnyAsync();
            if (bookingExists)
                throw new ConflictException($"Phòng {roomId} đã được đặt trong khoảng " + $"{expectedCheckIn:dd/MM/yyyy HH:mm} đến " + $"{expectedCheckOut:dd/MM/yyyy HH:mm}!");                                 
        }
        // ===========================================================
        // =                 CHECK VALID PROMOTION                   =
        // ===========================================================
        // Check Promotion hợp lệ hay không?
        private async Task<(Promotion? Promotion, decimal DiscountAmount)> SelectPromotionAsync(
            int? requestedPromotionId,
            IReadOnlyCollection<BookingDetail> bookingDetails,
            DateTime expectedCheckIn,
            DateTime expectedCheckOut,
            int? existingPromotionId = null)
        {
            var nowUtc = DateTime.UtcNow;
            var candidates = await _context.Promotions
                .Include(p => p.ApplyPromotions)
                .Where(p => p.IsDeleted == ActiveEnums.Active
                    && p.StartDate <= nowUtc && nowUtc < p.EndDate
                    && (!p.UsageLimit.HasValue || (p.UsedCount ?? 0) < p.UsageLimit.Value
                        || p.PromotionId == existingPromotionId)
                    && (!requestedPromotionId.HasValue || p.PromotionId == requestedPromotionId.Value))
                .ToListAsync();

            if (requestedPromotionId.HasValue && candidates.Count == 0)
                throw new BadRequestException("Khuyến mãi không tồn tại, chưa bắt đầu, đã hết hạn hoặc đã hết lượt sử dụng!");

            var nightCount = (expectedCheckOut.Date - expectedCheckIn.Date).Days;
            var applicable = candidates
                .Select(promotion =>
                {
                    var roomTypeIds = promotion.ApplyPromotions.Select(ap => ap.RoomTypeId).ToHashSet();
                    var eligibleRoomAmount = bookingDetails
                        .Where(detail => detail.Room?.RoomTypeId is int roomTypeId && roomTypeIds.Contains(roomTypeId))
                        .Sum(detail => detail.PricePerNight * nightCount);
                    var discount = CalculateDiscountAmount(promotion, eligibleRoomAmount);
                    return (Promotion: promotion, DiscountAmount: discount);
                })
                .Where(candidate => candidate.DiscountAmount > 0)
                .OrderByDescending(candidate => candidate.DiscountAmount)
                .ThenBy(candidate => candidate.Promotion.PromotionId)
                .ToList();

            if (requestedPromotionId.HasValue)
            {
                if (applicable.Count == 0)
                    throw new BadRequestException("Khuyến mãi không áp dụng cho loại phòng đã chọn hoặc chưa đạt giá trị tối thiểu!");
                return applicable[0];
            }

            return applicable.FirstOrDefault();
        }
        private void IncreasePromotionUsedCount( Promotion promotion)
        { 
            promotion.UsedCount = (promotion.UsedCount ?? 0) + 1; 
        }
        // ===========================================================
        // =                CREATE BOOKING DETAILS                   =
        // ===========================================================
        // Tạo chi tiết đặt phòng (BookingDetail)
        private async Task CreateBookingDetails(int bookingId, List<BookingDetailCreateDto> details)
        {
            foreach (var detail in details) 
            {
                var room = await _context.Rooms.FindAsync(detail.RoomId);
                if (room is null)
                    throw new NotFoundException($"Phòng có mã {detail.RoomId} không tồn tại!");
                var bookingDetail = new BookingDetail
                {
                    BookingId = bookingId,
                    RoomId = detail.RoomId,
                    // Lưu giá tại thời điểm đặt phòng
                    PricePerNight = room.PricePerNight,
                    GuestCount = detail.GuestCount,
                    ActualCheckIn = null,
                    ActualCheckOut = null
                };
                await _context.BookingDetails.AddAsync(bookingDetail);
            } 
        }
        // ===========================================================
        // =                    CALCULATE MONEY                      =
        // ===========================================================
        // Tính RoomAmount
        private decimal CalculateRoomAmount(IEnumerable<BookingDetail> bookingDetails, DateTime expectedCheckIn, DateTime expectedCheckOut)
        {
            var nightNumber =(expectedCheckOut.Date - expectedCheckIn.Date).Days;
            if (nightNumber <= 0)
                throw new BadRequestException("Số đêm lưu trú phải lớn hơn 0!");

            return bookingDetails.Sum(detail => detail.PricePerNight * nightNumber);
        }
        // Tính PackageAmount
        private async Task<decimal> CalculatePackageAmount(int? servicePackageId)
        {
            if (servicePackageId is null) return 0;
            var package = await _context.ServicePackages.FindAsync(servicePackageId);
            if (package is null)
                throw new NotFoundException("Gói dịch vụ không tồn tại!");
            return package.PackagePrice;
        }
        // Tính DiscountAmount
        private decimal CalculateDiscountAmount(Promotion? promotion, decimal eligibleRoomAmount)
        {
            if (promotion is null || promotion.DiscountValue is null) return 0;
            // Check giá trị tối thiểu được giảm
            if (promotion.MinAmount.HasValue && eligibleRoomAmount < promotion.MinAmount.Value)
                return 0;
            // Tính tiền giảm
            var discountValue = promotion.DiscountValue.Value;
            var amountAfterDiscount = promotion.DiscountType == PromotionDiscountTypeEnums.FixedAmount
                ? discountValue
                : eligibleRoomAmount * discountValue / 100;
            // Check tiền giảm vượt giới hạn giảm không?
            if (promotion.MaxDisCountAmount.HasValue && amountAfterDiscount > promotion.MaxDisCountAmount.Value)
                amountAfterDiscount = promotion.MaxDisCountAmount.Value;
            return Math.Round(Math.Clamp(amountAfterDiscount, 0, eligibleRoomAmount), 2, MidpointRounding.AwayFromZero);
        }
        // Tính TotalAmount
        private decimal CalculateTotalAmount(decimal roomAmount, decimal packageAmount, decimal additionalServiceAmount, decimal discountAmount)
        {
            var totalAmount = roomAmount + packageAmount + additionalServiceAmount - discountAmount;
            if(totalAmount < 0) return 0;
            return totalAmount;
        }
        // ===========================================================
        // =                           CRUD                          =
        // ===========================================================
        // Get all (Lấy danh sách booking đang hoạt động)

        public async Task<List<BookingResponseDto>> GetAllAsync()
        {
            var bookings = await _context.Bookings
                .Where(b => b.IsDeleted == ActiveEnums.Active)
                .OrderByDescending(b => b.BookingDate)
                .Select(b => new BookingResponseDto
                {
                    BookingId = b.BookingId,
                    // Thông tin Customer
                    CustomerId = b.CustomerId,
                    FullName = b.Customer!.User!.FullName,
                    Email = b.Customer!.User!.Email,
                    PhoneNumber = b.Customer!.User!.PhoneNumber,
                    // Thông tin Booking
                    BookingDate = b.BookingDate,
                    ExpectedCheckIn = b.ExpectedCheckIn,
                    ExpectedCheckOut = b.ExpectedCheckOut,
                    GuestCount = b.GuestCount,
                    RoomCount = b.RoomCount,
                    // Thông tin Promotion
                    PromotionId = b.PromotionId,
                    PromotionCode = b.Promotion != null
                        ? b.Promotion.PromotionCode
                        : null,
                    PromotionName = b.PromotionNameSnapshot ?? (b.Promotion != null ? b.Promotion.PromotionName : null),
                    DiscountType = b.PromotionDiscountTypeSnapshot ?? (b.Promotion != null ? b.Promotion.DiscountType : null),
                    DiscountValue = b.PromotionDiscountValueSnapshot ?? (b.Promotion != null ? b.Promotion.DiscountValue : null),
                    // Thông tin ServicePackage
                    ServicePackageId = b.ServicePackageId,
                    PackageName = b.ServicePackage != null
                        ? b.ServicePackage.PackageName
                        : null,
                    // Thông tin tiền
                    DepositAmount = b.DepositAmount,
                    RoomAmount = b.RoomAmount,
                    PackageAmount = b.PackageAmount,
                    DiscountAmount = b.DiscountAmount,
                    AdditionalServiceAmount = b.AdditionalServiceAmount,
                    TotalAmount = b.TotalAmount,
                    Note = b.Note,
                    Status = b.Status,
                    IsDeleted = b.IsDeleted,
                    CreateAt = b.CreateAt,
                    UpdateAt = b.UpdateAt,
                    // Booking Details
                    BookingDetails = b.BookingDetails
                        .Select(bd => new BookingDetailResponseDto
                        {
                            BookingDetailId = bd.BookingDetailId,
                            BookingId = bd.BookingId,
                            RoomId = bd.RoomId,
                            RoomName = bd.Room != null
                                ? bd.Room.RoomName
                                : null,
                            RoomTypeName = bd.Room != null && bd.Room.RoomType != null
                                ? bd.Room.RoomType.RoomTypeName
                                : null,
                            ActualCheckIn = bd.ActualCheckIn,
                            ActualCheckOut = bd.ActualCheckOut,
                            PricePerNight = bd.PricePerNight,
                            TotalAmount = bd.TotalAmount,
                            GuestCount = bd.GuestCount,
                            CreateAt = bd.CreateAt
                        })
                        .ToList()
                })
                .ToListAsync();

            return bookings;
        }

        // Get deleted (Lấy danh sách booking không hoạt động)
        public async Task<List<BookingResponseDto>> GetDeletedAsync()
        {
            var bookings = await _context.Bookings.Where(b => b.IsDeleted == ActiveEnums.Inactive)
                                        .OrderByDescending(b => b.BookingDate)
                                        .Select(b => new BookingResponseDto
                                        {
                                            BookingId = b.BookingId,
                                            // Thông tin Customer
                                            CustomerId = b.CustomerId,
                                            FullName = b.Customer!.User!.FullName,
                                            Email = b.Customer!.User!.Email,
                                            PhoneNumber = b.Customer!.User!.PhoneNumber,
                                            // Thông tin Booking
                                            BookingDate = b.BookingDate,
                                            ExpectedCheckIn = b.ExpectedCheckIn,
                                            ExpectedCheckOut = b.ExpectedCheckOut,
                                            GuestCount = b.GuestCount,
                                            RoomCount = b.RoomCount,
                                            // Thông tin Promotion
                                            PromotionId = b.PromotionId,
                                            PromotionCode = b.Promotion != null ? b.Promotion.PromotionCode : null,
                                            PromotionName = b.PromotionNameSnapshot ?? (b.Promotion != null ? b.Promotion.PromotionName : null),
                                            DiscountType = b.PromotionDiscountTypeSnapshot ?? (b.Promotion != null ? b.Promotion.DiscountType : null),
                                            DiscountValue = b.PromotionDiscountValueSnapshot ?? (b.Promotion != null ? b.Promotion.DiscountValue : null),

                                            // Thông tin ServicePackage
                                            ServicePackageId = b.ServicePackageId,
                                            PackageName = b.ServicePackage != null ? b.ServicePackage.PackageName : null,
                                            // Thông tin tiền
                                            DepositAmount = b.DepositAmount,
                                            RoomAmount = b.RoomAmount,
                                            PackageAmount = b.PackageAmount,
                                            DiscountAmount = b.DiscountAmount,
                                            AdditionalServiceAmount = b.AdditionalServiceAmount,
                                            TotalAmount = b.TotalAmount,
                                            Note = b.Note,
                                            Status = b.Status,
                                            IsDeleted = b.IsDeleted,
                                            CreateAt = b.CreateAt,
                                            UpdateAt = b.UpdateAt
                                        }).ToListAsync();
            return bookings;
        }
        // Get by Id
        public async Task<BookingResponseDto?> GetByIdAsync(int id)
        {
            var booking = await _context.Bookings.Include(b => b.Customer).ThenInclude(c => c!.User)
                                                 .Include(b => b.Promotion)
                                                 .Include(b => b.ServicePackage)
                                                 .Include(b => b.BookingDetails)
                                                    .ThenInclude(bd => bd.Room)
                                                        .ThenInclude(room => room!.RoomType)
                                                 .Include(b => b.BookingServices)
                                                    .ThenInclude(bs => bs.Service)
                                                 .FirstOrDefaultAsync(b => b.BookingId == id
                                                                        && b.IsDeleted == ActiveEnums.Active);
            if (booking is null) return null;
            return MapToResponseDto(booking);
        }
        // Lấy danh sách booking của khách hàng đã đăng nhập
        public async Task<List<BookingResponseDto>> GetMyBookingsAsync(int customerId)
        {
            var bookings = await _context.Bookings
                                         .Where(b => b.CustomerId == customerId 
                                                  && b.IsDeleted == ActiveEnums.Active)
                                         .Include(b => b.Customer)
                                            .ThenInclude(c => c!.User)
                                         .Include(b => b.Promotion)
                                         .Include(b => b.ServicePackage)
                                         .Include(b => b.BookingDetails)
                                            .ThenInclude(bd => bd.Room)
                                                .ThenInclude(room => room!.RoomType)
                                         .Include(b => b.BookingServices)
                                            .ThenInclude(bs => bs.Service)
                                         .OrderByDescending(b => b.BookingDate)
                                         .ToListAsync();
            return bookings.Select(MapToResponseDto).ToList();
        }
        // Lấy danh sách booking detail của khách hàng đang đăng nhập
        public async Task<BookingResponseDto?> GetMyBookingDetailAsync(int customerId, int bookingId)
        {
            var booking = await _context.Bookings
                                        .Where(b =>b.BookingId == bookingId
                                                && b.CustomerId == customerId 
                                                && b.IsDeleted == ActiveEnums.Active)
                                        .Include(b => b.Customer)
                                            .ThenInclude(c => c!.User)
                                        .Include(b => b.Promotion)
                                        .Include(b => b.ServicePackage)
                                        .Include(b => b.BookingDetails)
                                            .ThenInclude(bd => bd.Room)
                                                .ThenInclude(room => room!.RoomType)
                                        .Include(b => b.BookingServices)
                                            .ThenInclude(bs => bs.Service)
                                        .FirstOrDefaultAsync();
            if (booking is null)  return null;
            return MapToResponseDto(booking);
        }
        // Create 
        public async Task<BookingResponseDto> CreateAsync(BookingCreateDto dto)
        {
            // Dùng Transaction
            await using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // Check đầu vào
                await CheckCustomerId(dto.CustomerId);
                await CheckServicePackageId(dto.ServicePackageId);
                CheckDate(dto.ExpectedCheckIn, dto.ExpectedCheckOut);
                await CheckBookingDetails(dto.BookingDetails, dto.RoomCount, dto.GuestCount);
                // Check tất cả phòng còn trống không?
                foreach (var detail in dto.BookingDetails)
                {
                    await CheckRoomAvailable(detail.RoomId, dto.ExpectedCheckIn, dto.ExpectedCheckOut);
                }
                // Tạo booking
                var booking = MapToEntity(dto);
                // Các khoản tiền ban đầu
                booking.DepositAmount = 0;
                booking.RoomAmount = 0;
                booking.PackageAmount = 0;
                booking.DiscountAmount = 0;
                booking.AdditionalServiceAmount = 0;
                booking.TotalAmount = 0;
                await _context.Bookings.AddAsync(booking);
                await _context.SaveChangesAsync();
                // Tạo booking detail và tính tiền phòng
                await CreateBookingDetails(booking.BookingId, dto.BookingDetails);
                await _context.SaveChangesAsync();
                var listBookingDetails = await _context.BookingDetails
                                                       .Where(bd => bd.BookingId == booking.BookingId)
                                                       .Include(bd => bd.Room)
                                                       .ToListAsync();
                var roomAmount = CalculateRoomAmount(listBookingDetails, dto.ExpectedCheckIn, dto.ExpectedCheckOut);
                var (promotion, discountAmount) = await SelectPromotionAsync(
                    dto.PromotionId, listBookingDetails, dto.ExpectedCheckIn, dto.ExpectedCheckOut);
                booking.PromotionId = promotion?.PromotionId;
                booking.PromotionNameSnapshot = promotion?.PromotionName;
                booking.PromotionDiscountTypeSnapshot = promotion?.DiscountType;
                booking.PromotionDiscountValueSnapshot = promotion?.DiscountValue;
                // Tính tiền ServicePackage
                var packageAmount = await CalculatePackageAmount(dto.ServicePackageId);
                // Tiền dịch vụ đặt thêm ban đầu = 0
                var additionalServiceAmount = 0m;
                // Tính tổng tiền
                var totalAmount = CalculateTotalAmount(roomAmount, packageAmount, additionalServiceAmount, discountAmount);
                // Gán các khoản tiền vào booking
                booking.RoomAmount = roomAmount;
                booking.PackageAmount = packageAmount;
                booking.DiscountAmount = discountAmount;
                booking.AdditionalServiceAmount = additionalServiceAmount;
                booking.TotalAmount = totalAmount;
                // Tăng UsedCount
                if (promotion is not null)
                    IncreasePromotionUsedCount(promotion);
                await _context.SaveChangesAsync();
                // Lưu nhật ký
                await _auditLogService.LogAsync(AuditLogEnums.Create,
                                                "Booking",
                                                booking.BookingId,
                                                null,
                                                $"CustomerId: {booking.CustomerId}, TotalAmount: {booking.TotalAmount}");
                // Commit
                await transaction.CommitAsync();
                return (await GetByIdAsync(booking.BookingId))!;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
        // Create online
        public async Task<BookingResponseDto> CreateOnlineAsync(BookingCreateOnlineDto dto, int customerId)
        {
            // Dùng Transaction
            await using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // Check đầu vào
                await CheckServicePackageId(dto.ServicePackageId);
                CheckDate(dto.ExpectedCheckIn, dto.ExpectedCheckOut);
                await CheckBookingDetails(dto.BookingDetails, dto.RoomCount, dto.GuestCount);
                // Check tất cả phòng còn trống không?
                foreach (var detail in dto.BookingDetails)
                {
                    await CheckRoomAvailable(detail.RoomId, dto.ExpectedCheckIn, dto.ExpectedCheckOut);
                }
                // Tạo booking
                var booking = MapToEntityOnline(dto, customerId);
                // Các khoản tiền ban đầu
                booking.DepositAmount = 0;
                booking.RoomAmount = 0;
                booking.PackageAmount = 0;
                booking.DiscountAmount = 0;
                booking.AdditionalServiceAmount = 0;
                booking.TotalAmount = 0;
                await _context.Bookings.AddAsync(booking);
                await _context.SaveChangesAsync();
                // Tạo booking detail và tính tiền phòng
                await CreateBookingDetails(booking.BookingId, dto.BookingDetails);
                await _context.SaveChangesAsync();
                var listBookingDetails = await _context.BookingDetails
                                                       .Where(bd => bd.BookingId == booking.BookingId)
                                                       .Include(bd => bd.Room)
                                                       .ToListAsync();
                var roomAmount = CalculateRoomAmount(listBookingDetails, dto.ExpectedCheckIn, dto.ExpectedCheckOut);
                var (promotion, discountAmount) = await SelectPromotionAsync(
                    dto.PromotionId, listBookingDetails, dto.ExpectedCheckIn, dto.ExpectedCheckOut);
                booking.PromotionId = promotion?.PromotionId;
                booking.PromotionNameSnapshot = promotion?.PromotionName;
                booking.PromotionDiscountTypeSnapshot = promotion?.DiscountType;
                booking.PromotionDiscountValueSnapshot = promotion?.DiscountValue;
                // Tính tiền ServicePackage
                var packageAmount = await CalculatePackageAmount(dto.ServicePackageId);
                // Tiền dịch vụ đặt thêm ban đầu = 0
                var additionalServiceAmount = 0m;
                // Tính tổng tiền
                var totalAmount = CalculateTotalAmount(roomAmount, packageAmount, additionalServiceAmount, discountAmount);
                // Gán các khoản tiền vào booking
                booking.RoomAmount = roomAmount;
                booking.PackageAmount = packageAmount;
                booking.DiscountAmount = discountAmount;
                booking.AdditionalServiceAmount = additionalServiceAmount;
                booking.TotalAmount = totalAmount;
                var depositPercentage = _configuration.GetValue<decimal>("BookingSettings:DepositPercentage", 20m);
                if (depositPercentage <= 0 || depositPercentage > 100)
                    throw new BadRequestException("BookingSettings:DepositPercentage phải nằm trong khoảng lớn hơn 0 đến 100!");
                booking.DepositAmount = Math.Round(totalAmount * depositPercentage / 100m, 2, MidpointRounding.AwayFromZero);
                // Tăng UsedCount
                if (promotion is not null)
                    IncreasePromotionUsedCount(promotion);
                await _context.SaveChangesAsync();
                if (booking.DepositAmount > 0)
                    await _paymentService.CreateDepositAsync(booking, booking.DepositAmount);
                await _auditLogService.LogAsync(AuditLogEnums.Create,
                                                "Booking",
                                                booking.BookingId,
                                                null,
                                                $"CustomerId: {booking.CustomerId}, TotalAmount: {booking.TotalAmount}");
                // Commit
                await transaction.CommitAsync();
                return (await GetByIdAsync(booking.BookingId))!;
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
                if (booking.Status != BookingEnums.Pending) 
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
        // Soft delete
        public async Task<bool> SoftDeleteAsync(int id)
        {
            var booking = await _context.Bookings
                                        .FirstOrDefaultAsync(b => b.BookingId == id
                                                               && b.IsDeleted == ActiveEnums.Active);

            if (booking is null) return false;
            booking.IsDeleted = ActiveEnums.Inactive;
            booking.UpdateAt = DateTime.Now;
            await _context.SaveChangesAsync();
            await _auditLogService.LogAsync(AuditLogEnums.SoftDelete,
                                            "Booking",
                                            booking.BookingId,
                                            "IsDeleted: Active",
                                            "IsDeleted: Inactive");
            return true;
        }
        // Delete
        public async Task<bool> DeleteAsync(int id)
        {
            await using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // Chỉ cho phép delete Booking đã soft delete
                var booking = await _context.Bookings
                                            .FirstOrDefaultAsync(b => b.BookingId == id
                                                                   && b.IsDeleted == ActiveEnums.Inactive);
                if (booking is null) return false;
                // Không cho xóa nếu Booking đã có Invoice
                var hasInvoice = await _context.Invoices.AnyAsync(i => i.BookingId == id);
                if (hasInvoice)
                    throw new BadRequestException("Không thể xóa Booking vì Booking đã phát sinh hóa đơn!");
                var hasPayments = await _context.Payments.AnyAsync(p => p.BookingId == id || p.Invoice!.BookingId == id);
                if (hasPayments)
                    throw new BadRequestException("Không thể xóa Booking vì Booking đã có lịch sử thanh toán hoặc hoàn tiền!");
                // Không cho xóa nếu Booking đã có Cancellation
                var hasCancellation = await _context.Cancellations.AnyAsync(c => c.BookingId == id);
                if (hasCancellation)
                    throw new BadRequestException("Không thể xóa Booking vì Booking đã có lịch sử hủy!");
                // Không cho xóa nếu Booking đã có Review
                var hasReview = await _context.Reviews.AnyAsync(r => r.BookingId == id);
                if (hasReview)
                    throw new BadRequestException("Không thể xóa Booking vì Booking đã có đánh giá!");
                // BookingDetails và Guests sẽ được xóa thông qua Cascade Delete
                // BookingDetails -> Guests
                // BookingServices cũng sẽ được xóa thông qua Cascade Delete
                _context.Bookings.Remove(booking);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                return true;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
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
            booking.Status = BookingEnums.Confirmed;
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
        }
    }
}
