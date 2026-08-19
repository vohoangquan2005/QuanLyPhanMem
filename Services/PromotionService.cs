using Azure.Core;
using HotelManagement.Data;
using HotelManagement.DTOs.Promotion;
using HotelManagement.Enums;
using HotelManagement.Exceptions;
using HotelManagement.Models;
using Microsoft.EntityFrameworkCore;

namespace HotelManagement.Services
{
    public class PromotionService
    {
        private readonly AppDbContext _context;
        private readonly AuditLogService _auditLogService;
        private static DateTime NormalizeUtc(DateTime value) => value.Kind switch
        {
            DateTimeKind.Utc => value,
            DateTimeKind.Local => value.ToUniversalTime(),
            _ => DateTime.SpecifyKind(value, DateTimeKind.Utc)
        };
        public PromotionService(AppDbContext context, AuditLogService auditLogService)
        {
            _context = context;
            _auditLogService = auditLogService;
        }
        private PromotionResponseDto MapToResponseDto(Promotion promotion)
        {
            return new PromotionResponseDto
            {
                PromotionId = promotion.PromotionId,
                PromotionCode= promotion.PromotionCode,
                PromotionName= promotion.PromotionName,
                DiscountType = promotion.DiscountType,
                DiscountValue= promotion.DiscountValue,
                MinAmount= promotion.MinAmount,
                MaxDisCountAmount = promotion.MaxDisCountAmount,
                UsageLimit= promotion.UsageLimit,
                UsedCount = promotion.UsedCount,
                StartDate= DateTime.SpecifyKind(promotion.StartDate, DateTimeKind.Utc),
                EndDate = DateTime.SpecifyKind(promotion.EndDate, DateTimeKind.Utc),
                IsDeleted = promotion.IsDeleted,
                CreateAt = promotion.CreateAt,
                UpdateAt = promotion.UpdateAt
            };
        }
        // Chuyển CreateDto -> Promotion
        private Promotion MapToEntity(PromotionCreateDto dto)
        {
            return new Promotion
            {
                PromotionCode = dto.PromotionCode,
                PromotionName = dto.PromotionName,
                DiscountType = dto.DiscountType,
                DiscountValue = dto.DiscountValue,
                MinAmount = dto.MinAmount,
                MaxDisCountAmount = dto.MaxDisCountAmount,
                UsageLimit = dto.UsageLimit,
                StartDate = NormalizeUtc(dto.StartDate),
                EndDate = NormalizeUtc(dto.EndDate)
            };
        }
        // Update Promotion từ PromotionUpdateDto
        private void UpdateEntity(Promotion promotion, PromotionUpdateDto dto)
        {
            promotion.PromotionCode = dto.PromotionCode;
            promotion.PromotionName = dto.PromotionName;
            promotion.DiscountType = dto.DiscountType;
            promotion.DiscountValue = dto.DiscountValue;
            promotion.MinAmount = dto.MinAmount;
            promotion.MaxDisCountAmount = dto.MaxDisCountAmount;
            promotion.UsageLimit = dto.UsageLimit;
            promotion.StartDate = NormalizeUtc(dto.StartDate);
            promotion.EndDate = NormalizeUtc(dto.EndDate);
        }
        // ===========================================================
        // =                       VALIDATION                        =
        // ===========================================================
        // Check trùng PromotionCode
        private async Task PromotionCodeExists(string promotionCode, int? promotionId = null)
        {
            var exists = await _context.Promotions.AnyAsync(p => p.PromotionCode == promotionCode
                                                  && (promotionId == null || promotionId != p.PromotionId));
            if (exists)
                throw new ConflictException("Mã khuyến mãi đã tồn tại!");
        }
        // Check trùng PromotionName
        private async Task PromotionNameExists(string promotionName, int? promotionId = null)
        {
            var exists = await _context.Promotions.AnyAsync(p => p.PromotionName == promotionName
                                                    && (promotionId == null || promotionId != p.PromotionId));
            if (exists)
                throw new ConflictException("Tên khuyến mãi đã tồn tại!");
        }
        // Check Date
        private void CheckDate(DateTime startDate, DateTime endDate, bool isCreate = false)
        {
            var startUtc = NormalizeUtc(startDate);
            var endUtc = NormalizeUtc(endDate);
            if (isCreate && startUtc <= DateTime.UtcNow)
                throw new BadRequestException("Thời gian bắt đầu khuyến mãi phải lớn hơn thời gian hiện tại!");
            if (endUtc <= startUtc)
                throw new BadRequestException("Thời gian kết thúc phải sau thời gian bắt đầu!");
        }
        // Check UsageLimit
        private void CheckUsageLimit(int? usageLimit, int usedCount)
        {
            if (usageLimit.HasValue && usageLimit.Value < usedCount)
                throw new BadRequestException("Giới hạn sử dụng không được nhỏ hơn số lần sử dụng!");
        }
        private static void CheckDiscount(PromotionDiscountTypeEnums discountType, decimal? discountValue)
        {
            if (!Enum.IsDefined(discountType))
                throw new BadRequestException("Hình thức giảm giá không hợp lệ!");
            if (discountType == PromotionDiscountTypeEnums.Percentage && discountValue is > 100)
                throw new BadRequestException("Phần trăm khuyến mãi phải từ 0 đến 100!");
        }
        // ===========================================================
        // =                      APPLY PROMOTION                    =
        // ===========================================================
        // Lấy RoomType đang Active
        private async Task<RoomType> GetActiveRoomType(int roomTypeId)
        {
            var roomType = await _context.RoomTypes
                                         .FirstOrDefaultAsync(rt => rt.RoomTypeId == roomTypeId
                                                                 && rt.IsDeleted == ActiveEnums.Active);
            if (roomType is null)
                throw new NotFoundException($"Loại phòng có Id = {roomTypeId} không tồn tại hoặc đã bị xóa!");
            return roomType;
        }
        // Check trùng RoomType trong danh sách ApplyPromotion
        private void CheckDuplicateRoomType( List<ApplyPromotionCreateDto> applyPromotions) 
        { var duplicate = applyPromotions .GroupBy(ap => ap.RoomTypeId) 
                .Any(g => g.Count() > 1); 
            if (duplicate) 
                throw new ConflictException( "Không thể áp dụng khuyến mãi trùng cho cùng một loại phòng!");
        }
        //Tạo ApplyPromotion
         private async Task CreateApplyPromotion(int promotionId, List<ApplyPromotionCreateDto> applyPromotions)
        {
            if (applyPromotions is null || applyPromotions.Count == 0)
                throw new BadRequestException("Vui lòng chọn ít nhất một loại phòng áp dụng khuyến mãi!");
            CheckDuplicateRoomType(applyPromotions);
            foreach (var dto in applyPromotions)
            {
                await GetActiveRoomType(dto.RoomTypeId);
                var applyPromotion = new ApplyPromotion
                {
                    PromotionId = promotionId,
                    RoomTypeId = dto.RoomTypeId
                };
                await _context.ApplyPromotions.AddAsync(applyPromotion);
            }
        }
        //Update ApplyPromotion
         private async Task UpdateApplyPromotion(int promotionId, List<ApplyPromotionUpdateDto> applyPromotions)
         {
             if (applyPromotions is null || applyPromotions.Count == 0)
                throw new BadRequestException("Vui lòng chọn ít nhất một loại phòng áp dụng khuyến mãi!");
             var duplicate = applyPromotions.GroupBy(ap => ap.RoomTypeId)
                                            .Any(g => g.Count() > 1);
             if (duplicate)
                throw new ConflictException("Không thể áp dụng khuyến mãi trùng cho cùng một loại phòng!");
             //Kiểm tra tất cả RoomType trước
             foreach (var dto in applyPromotions)
             { 
                 await GetActiveRoomType(dto.RoomTypeId); 
             }
             //Lấy danh sách ApplyPromotion hiện tại
             var currentApplyPromotions = await _context.ApplyPromotions
                                                        .Where(ap => ap.PromotionId == promotionId).ToListAsync();
             //Xóa danh sách cũ
             _context.ApplyPromotions.RemoveRange(currentApplyPromotions);
             //Tạo danh sách mới
             foreach (var dto in applyPromotions)
             {
             var applyPromotion = new ApplyPromotion
             { 
                 PromotionId = promotionId, 
                 RoomTypeId = dto.RoomTypeId 
             };
             await _context.ApplyPromotions.AddAsync(applyPromotion);
             }
         }
        // ===========================================================
        // =                           CRUD                          =
        // ===========================================================
        // Get all Active(Lấy promotion còn hoạt động)
        public async Task<List<PromotionResponseDto>> GetAllAsync()
        {
            var promotions = await _context.Promotions.Where(p => p.IsDeleted == ActiveEnums.Active)
                                            .OrderBy(p => p.StartDate)
                                                .ThenBy(p => p.PromotionName)
                                            .Select(p => new PromotionResponseDto
                                            {
                                                PromotionId = p.PromotionId,
                                                PromotionCode = p.PromotionCode,
                                                PromotionName = p.PromotionName,
                                                DiscountType = p.DiscountType,
                                                DiscountValue = p.DiscountValue,
                                                MinAmount = p.MinAmount,
                                                MaxDisCountAmount = p.MaxDisCountAmount,
                                                UsageLimit = p.UsageLimit,
                                                UsedCount = p.UsedCount,
                                                StartDate = p.StartDate,
                                                EndDate = p.EndDate,
                                                IsDeleted = p.IsDeleted,
                                                CreateAt = p.CreateAt,
                                                UpdateAt = p.UpdateAt
                                            }).ToListAsync();
            return promotions.Select(p =>
            {
                p.StartDate = DateTime.SpecifyKind(p.StartDate, DateTimeKind.Utc);
                p.EndDate = DateTime.SpecifyKind(p.EndDate, DateTimeKind.Utc);
                return p;
            }).ToList();
        }
        // Get all InActive(Lấy promotion không hoạt động)
        public async Task<List<PromotionResponseDto>> GetDeletedAsync()
        {
            var promotions = await _context.Promotions.Where(p => p.IsDeleted == ActiveEnums.Inactive)
                                            .OrderBy(p => p.StartDate)
                                                .ThenBy(p => p.PromotionName)
                                            .Select(p => new PromotionResponseDto
                                            {
                                                PromotionId = p.PromotionId,
                                                PromotionCode = p.PromotionCode,
                                                PromotionName = p.PromotionName,
                                                DiscountType = p.DiscountType,
                                                DiscountValue = p.DiscountValue,
                                                MinAmount = p.MinAmount,
                                                MaxDisCountAmount = p.MaxDisCountAmount,
                                                UsageLimit = p.UsageLimit,
                                                UsedCount = p.UsedCount,
                                                StartDate = p.StartDate,
                                                EndDate = p.EndDate,
                                                IsDeleted = p.IsDeleted,
                                                CreateAt = p.CreateAt,
                                                UpdateAt = p.UpdateAt
                                            }).ToListAsync();
            return promotions.Select(p =>
            {
                p.StartDate = DateTime.SpecifyKind(p.StartDate, DateTimeKind.Utc);
                p.EndDate = DateTime.SpecifyKind(p.EndDate, DateTimeKind.Utc);
                return p;
            }).ToList();
        }
        // Get by Id
        public async Task<PromotionResponseDto?> GetByIdAsync(int id)
        {
            var promotion = await _context.Promotions
                                          .FirstOrDefaultAsync(p => p.PromotionId == id 
                                                                 && p.IsDeleted == ActiveEnums.Active);
            if (promotion is null) return null;
            return MapToResponseDto(promotion);
        }
        // Create 
        public async Task<PromotionResponseDto> CreateAsync(PromotionCreateRequestDto request)
        {
            await using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // Validation Promotion
                await PromotionCodeExists(request.Promotion.PromotionCode!);
                await PromotionNameExists(request.Promotion.PromotionName!);
                CheckDate(request.Promotion.StartDate, request.Promotion.EndDate, true);
                CheckDiscount(request.Promotion.DiscountType, request.Promotion.DiscountValue);
                CheckUsageLimit(request.Promotion.UsageLimit, 0);
                // Create Promotion
                var promotion = MapToEntity(request.Promotion);
                await _context.Promotions.AddAsync(promotion);
                await _context.SaveChangesAsync();
                // Create ApplyPromotion
                await CreateApplyPromotion(promotion.PromotionId, request.ApplyPromotions);
                await _context.SaveChangesAsync();
                await _auditLogService.LogAsync(AuditLogEnums.Create,
                                                "Promotion",
                                                promotion.PromotionId,
                                                null,
                                                $"PromotionCode: {promotion.PromotionCode}, " +
                                                $"PromotionName: {promotion.PromotionName}, " +
                                                $"DiscountValue: {promotion.DiscountValue}, " +
                                                $"UsageLimit: {promotion.UsageLimit}, " +
                                                $"StartDate: {promotion.StartDate}, " +
                                                $"EndDate: {promotion.EndDate}, " +
                                                $"RoomTypeIds: {string.Join(", ", request.ApplyPromotions.Select(ap => ap.RoomTypeId))}");
                await transaction.CommitAsync();
                return (await GetByIdAsync(promotion.PromotionId))!;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
            
        }
        // Update
        public async Task<PromotionResponseDto?> UpdateAsync(int id, PromotionUpdateRequestDto request)
        {
            await using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var promotion = await _context.Promotions
                                              .FirstOrDefaultAsync(p => p.PromotionId == id
                                                                     && p.IsDeleted == ActiveEnums.Active);
                if (promotion is null) return null;
                // Validation Promotion
                await PromotionCodeExists(request.Promotion.PromotionCode!, id);
                await PromotionNameExists(request.Promotion.PromotionName!, id);
                CheckDate(request.Promotion.StartDate, request.Promotion.EndDate);
                CheckDiscount(request.Promotion.DiscountType, request.Promotion.DiscountValue);
                CheckUsageLimit(request.Promotion.UsageLimit, promotion.UsedCount ?? 0);
                var oldValue = $"PromotionCode: {promotion.PromotionCode}, " +
                               $"PromotionName: {promotion.PromotionName}, " +
                               $"DiscountValue: {promotion.DiscountValue}, " +
                               $"MinAmount: {promotion.MinAmount}, " +
                               $"MaxDisCountAmount: {promotion.MaxDisCountAmount}, " +
                               $"UsageLimit: {promotion.UsageLimit}, " +
                               $"StartDate: {promotion.StartDate}, " +
                               $"EndDate: {promotion.EndDate}";
                // Update Promotion
                UpdateEntity(promotion, request.Promotion);
                // Update ApplyPromotion
                await UpdateApplyPromotion(id, request.ApplyPromotions);
                await _context.SaveChangesAsync();
                var newValue = $"PromotionCode: {promotion.PromotionCode}, " +
                               $"PromotionName: {promotion.PromotionName}, " +
                               $"DiscountValue: {promotion.DiscountValue}, " +
                               $"MinAmount: {promotion.MinAmount}, " +
                               $"MaxDisCountAmount: {promotion.MaxDisCountAmount}, " +
                               $"UsageLimit: {promotion.UsageLimit}, " +
                               $"StartDate: {promotion.StartDate}, " +
                               $"EndDate: {promotion.EndDate}, " +
                               $"RoomTypeIds: {string.Join(", ", request.ApplyPromotions.Select(ap => ap.RoomTypeId))}";
                // Lưu nhật ký
                await _auditLogService.LogAsync(AuditLogEnums.Update,
                                                "Promotion",
                                                promotion.PromotionId,
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
        // Delete
        public async Task<bool> DeleteAsync(int id)
        {
            var promotion = await _context.Promotions
                                          .FirstOrDefaultAsync(p => p.PromotionId == id
                                                                 && p.IsDeleted == ActiveEnums.Inactive);
            if (promotion is null) return false;
            // Không cho xóa nếu Promotion còn trong Booking
            var booking = await _context.Bookings
                                        .AnyAsync(b => b.PromotionId == id);
            if (booking)
                throw new BadRequestException("Khuyến mãi đã được sử dụng trong đặt phòng. Không thể xóa!");
            _context.Promotions.Remove(promotion);
            await _context.SaveChangesAsync();
            return true;
        }
        // Xóa tạm
        public async Task<PromotionResponseDto?> SoftDeleteAsync(int id)
        {
            var promotion = await _context.Promotions
                                          .FirstOrDefaultAsync(p => p.PromotionId == id
                                                                 && p.IsDeleted == ActiveEnums.Active);
            if (promotion is null) return null;
            promotion.IsDeleted = ActiveEnums.Inactive;
            await _context.SaveChangesAsync();
            await _auditLogService.LogAsync(AuditLogEnums.SoftDelete,
                                            "Promotion",
                                            promotion.PromotionId,
                                            "IsDeleted: Active",
                                            "IsDeleted: Inactive");
            return MapToResponseDto(promotion);
        }
        // Khôi phục (Restore)
        public async Task<PromotionResponseDto?> RestoreAsync(int id)
        {
            var promotion = await _context.Promotions
                                          .FirstOrDefaultAsync(p => p.PromotionId == id
                                                                 && p.IsDeleted == ActiveEnums.Inactive);
            if (promotion is null) return null;
            promotion.IsDeleted = ActiveEnums.Active;
            await _context.SaveChangesAsync();
            await _auditLogService.LogAsync(AuditLogEnums.Restore,
                                            "Promotion",
                                            promotion.PromotionId,
                                            "IsDeleted: Inactive",
                                            "IsDeleted: Active");
            return MapToResponseDto(promotion);
        }
    }
}
