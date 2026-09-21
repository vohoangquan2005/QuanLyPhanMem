using HotelManagement.Data;
using HotelManagement.DTOs.CancallationPolicy;
using HotelManagement.Enums;
using HotelManagement.Exceptions;
using HotelManagement.Models;
using Microsoft.EntityFrameworkCore;

namespace HotelManagement.Services
{
    public class CancellationPolicyService
    {
        private readonly AppDbContext _context;
        private readonly AuditLogService _auditLogService;
        public CancellationPolicyService(AppDbContext context, AuditLogService auditLogService)
        {
            _context = context;
            _auditLogService = auditLogService;
        }
        // Chuyển CancellationPolicy -> ResponseDto
        private CancellationPolicyResponseDto MapToResponseDto(CancellationPolicy policy)
        {
            return new CancellationPolicyResponseDto
            {
                CancellationPolicyId = policy.CancellationPolicyId,
                PolicyName = policy.PolicyName,
                CancellationTime = policy.CancellationTime,
                RefundPercentage = policy.RefundPercentage,
                Description = policy.Description,
                IsDeleted = policy.IsDeleted,
                CreateAt = policy.CreateAt,
                UpdateAt = policy.UpdateAt
            };
        }
        // Chuyển CancellationPolicyCreateDto -> CancellationPolicy
        private CancellationPolicy MapToEntity(CancellationPolicyCreateDto dto)
        {
            return new CancellationPolicy
            {
                PolicyName = dto.PolicyName,
                CancellationTime = dto.CancellationTime,
                CancellationTimeUnit = dto.CancellationTimeUnit,
                RefundPercentage = dto.RefundPercentage,
                Description = dto.Description,
            };
        }
        // Update CancellationPolicy từ 
        private void UpdateEntity(CancellationPolicy policy, CancellationPolicyUpdateDto dto)
        {
            policy.PolicyName = dto.PolicyName;
            policy.CancellationTime = dto.CancellationTime;
            policy.CancellationTimeUnit = dto.CancellationTimeUnit;
            policy.RefundPercentage = dto.RefundPercentage;
            policy.Description = dto.Description;
        }

        // Check trùng tên chính sách
        private async Task PolicyNameExists(string policyName, int? cancellationPolicyId = null)
        {
            var exists = await _context.CancellationPolicies.AnyAsync(cp => cp.PolicyName == policyName
                                                && (cancellationPolicyId == null || cancellationPolicyId != cp.CancellationPolicyId));
            if (exists)
                throw new ConflictException("Tên chính sách đã tồn tại!");
        }
        // Check mốc CancellationHours đã tồn tại chưa
        private async Task CancellationHoursExists(int cancellationTime, CancellationTimeUnitEnums cancellationTimeUnit, int? cancellationPolicyId = null)
        {
            var exists = await _context.CancellationPolicies
                .AnyAsync(cp => cp.CancellationTime == cancellationTime
                             && cp.CancellationTimeUnit == cancellationTimeUnit
                             && (cancellationPolicyId == null || cp.CancellationPolicyId != cancellationPolicyId));
            if (exists)
                throw new ConflictException("Đã tồn tại chính sách với mốc thời gian này!");
        }
        // ===========================================================
        // =                           CRUD                          =
        // ===========================================================
        // Get all Active(Lấy cancellation policy còn hoạt động)
        public async Task<List<CancellationPolicyResponseDto>> GetAllAsync()
        {
            var policies = await _context.CancellationPolicies.Where(cp => cp.IsDeleted == ActiveEnums.Active)
                                            .Select(cp => new CancellationPolicyResponseDto
                                            {
                                                CancellationPolicyId = cp.CancellationPolicyId,
                                                PolicyName = cp.PolicyName,
                                                CancellationTime = cp.CancellationTime,
                                                RefundPercentage = cp.RefundPercentage,
                                                Description = cp.Description,
                                                IsDeleted = cp.IsDeleted,
                                                CreateAt = cp.CreateAt,
                                                UpdateAt = cp.UpdateAt
                                            }).ToListAsync();
            return policies;
        }
        // Get all InActive(Lấy cancellation policy không hoạt động)
        public async Task<List<CancellationPolicyResponseDto>> GetDeletedAsync()
        {
            var policies = await _context.CancellationPolicies.Where(cp => cp.IsDeleted == ActiveEnums.Inactive)
                                            .Select(cp => new CancellationPolicyResponseDto
                                            {
                                                CancellationPolicyId = cp.CancellationPolicyId,
                                                PolicyName = cp.PolicyName,
                                                CancellationTime = cp.CancellationTime,
                                                RefundPercentage = cp.RefundPercentage,
                                                Description = cp.Description,
                                                IsDeleted = cp.IsDeleted,
                                                CreateAt = cp.CreateAt,
                                                UpdateAt = cp.UpdateAt
                                            }).ToListAsync();
            return policies;
        }
        // Get by Id
        public async Task<CancellationPolicyResponseDto?> GetByIdAsync(int id)
        {
            var policy = await _context.CancellationPolicies
                                       .FirstOrDefaultAsync(cp => cp.CancellationPolicyId == id
                                                               && cp.IsDeleted == ActiveEnums.Active);
            if (policy is null) return null;
            return MapToResponseDto(policy);
        }
        // Create
        public async Task<CancellationPolicyResponseDto> CreateAsync(CancellationPolicyCreateDto dto)
        {
            await PolicyNameExists(dto.PolicyName!);
            await CancellationHoursExists(dto.CancellationTime, dto.CancellationTimeUnit);
            var policy = MapToEntity(dto);
            await _context.CancellationPolicies.AddAsync(policy);
            await _context.SaveChangesAsync();
            await _auditLogService.LogAsync(AuditLogEnums.Create,
                                            "CancellationPolicy",
                                            policy.CancellationPolicyId,
                                            null,
                                            $"PolicyName: {policy.PolicyName}, " +
                                            $"CancellationTime: {policy.CancellationTime} {policy.CancellationTimeUnit}, " +
                                            $"RefundPercentage: {policy.RefundPercentage}%");
            return MapToResponseDto(policy);
        }
        // Update
        public async Task<CancellationPolicyResponseDto?> UpdateAsync(int id, CancellationPolicyUpdateDto dto)
        {
            var policy = await _context.CancellationPolicies
                                       .FirstOrDefaultAsync(cp => cp.CancellationPolicyId == id
                                                               && cp.IsDeleted == ActiveEnums.Active);
            if (policy is null) return null;
            await PolicyNameExists(dto.PolicyName!, id);
            await CancellationHoursExists(dto.CancellationTime, dto.CancellationTimeUnit, id);
            var oldValue = $"PolicyName: {policy.PolicyName}, " +
                           $"CancellationTime: {policy.CancellationTime} {policy.CancellationTimeUnit}, " +
                           $"RefundPercentage: {policy.RefundPercentage}%, " +
                           $"Description: {policy.Description}";
            UpdateEntity(policy, dto);
            var newValue = $"PolicyName: {policy.PolicyName}, " +
                           $"CancellationTime: {policy.CancellationTime} {policy.CancellationTimeUnit}, " +
                           $"RefundPercentage: {policy.RefundPercentage}%, " +
                           $"Description: {policy.Description}";
            await _auditLogService.LogAsync(AuditLogEnums.Update,
                                            "CancellationPolicy",
                                            policy.CancellationPolicyId,
                                            oldValue,
                                            newValue);
            await _context.SaveChangesAsync();
            return MapToResponseDto(policy);
        }
        // Delete
        public async Task<bool> DeleteAsync(int id)
        {
            var policy = await _context.CancellationPolicies
                                       .FirstOrDefaultAsync(cp => cp.CancellationPolicyId == id
                                                               && cp.IsDeleted == ActiveEnums.Inactive);
            if (policy is null) return false;
            var cancellation = await _context.Cancellations
                                             .AnyAsync(c => c.CancellationPolicyId == id);
            if (cancellation)
                throw new BadRequestException("Chính sách đã được sử dụng. Không thể xóa!");
            _context.CancellationPolicies.Remove(policy);
            await _context.SaveChangesAsync();
            return true;
        }
        // Xóa tạm
        public async Task<CancellationPolicyResponseDto?> SoftDeleteAsync(int id)
        {
            var policy = await _context.CancellationPolicies
                                       .FirstOrDefaultAsync(cp => cp.CancellationPolicyId == id
                                                               && cp.IsDeleted == ActiveEnums.Active);
            if (policy is null) return null;
            policy.IsDeleted = ActiveEnums.Inactive;
            await _context.SaveChangesAsync();
            await _auditLogService.LogAsync(AuditLogEnums.SoftDelete,
                                            "CancellationPolicy",
                                            policy.CancellationPolicyId,
                                            "IsDeleted: Active",
                                            "IsDeleted: Inactive");
            return MapToResponseDto(policy);
        }
        // Khôi phục (Restore)
        public async Task<CancellationPolicyResponseDto?> RestoreAsync(int id)
        {
            var policy = await _context.CancellationPolicies
                                       .FirstOrDefaultAsync(cp => cp.CancellationPolicyId == id
                                                               && cp.IsDeleted == ActiveEnums.Inactive);
            if (policy is null) return null;
            policy.IsDeleted = ActiveEnums.Active;
            await _context.SaveChangesAsync();
            await _auditLogService.LogAsync(AuditLogEnums.Restore,
                                            "CancellationPolicy",
                                            policy.CancellationPolicyId,
                                            "IsDeleted: Inactive",
                                            "IsDeleted: Active");
            return MapToResponseDto(policy);
        }
    }
}
