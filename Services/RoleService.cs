using HotelManagement.Data;
using HotelManagement.DTOs.Role;
using HotelManagement.Enums;
using HotelManagement.Exceptions;
using HotelManagement.Models;
using Microsoft.EntityFrameworkCore;

namespace HotelManagement.Services
{
    public class RoleService
    {
        private readonly AppDbContext _context;
        public RoleService(AppDbContext context)
        {
            _context = context;
        }
        // Hàm chuyển Role -> RoleResponseDto
        private RoleResponseDto MapToResponseDto(Role role)
        {
            return new RoleResponseDto
            {
                RoleId = role.RoleId,
                RoleName = role.RoleName,
                Description = role.Description,
                IsDeleted = role.IsDeleted,
                CreateAt = role.CreateAt,
                UpdateAt = role.UpdateAt
            };
        }
        // Check trùng RoleName
        private async Task RoleNameExists(string roleName, int? roleId = null)
        {
            var exists = await _context.Roles
                                             .AnyAsync(r => r.RoleName == roleName
                                                   && (roleId == null || roleId != r.RoleId));
            if (exists)
                throw new ConflictException("Tên vai trò đã tồn tại!");
        }
        // ===========================================================
        // =                           CRUD                          =
        // ===========================================================
        // Get all Active(Lấy roles còn hoạt động)
        public async Task<List<RoleResponseDto>> GetAllAsync()
        {
            var roles = await _context.Roles.Where(r => r.IsDeleted == ActiveEnums.Active)
                                        .OrderBy(r => r.RoleName)
                                        .Select(r => new RoleResponseDto
                                        {
                                            RoleId = r.RoleId,
                                            RoleName = r.RoleName,
                                            Description = r.Description,
                                            IsDeleted = r.IsDeleted,
                                            CreateAt = r.CreateAt,
                                            UpdateAt = r.UpdateAt
                                        }).ToListAsync();
            return roles;
        }
        // Get all InActive(Lấy roles không hoạt động)
        public async Task<List<RoleResponseDto>> GetDeletedAsync()
        {
            var roles = await _context.Roles.Where(r => r.IsDeleted == ActiveEnums.Inactive)
                                        .OrderBy(r => r.RoleName)
                                        .Select(r => new RoleResponseDto
                                        {
                                            RoleId = r.RoleId,
                                            RoleName = r.RoleName,
                                            Description = r.Description,
                                            IsDeleted = r.IsDeleted,
                                            CreateAt = r.CreateAt,
                                            UpdateAt = r.UpdateAt
                                        }).ToListAsync();
            return roles;
        }
        // Get by Id
        public async Task<RoleResponseDto?> GetByIdAsync(int id)
        {
            var role = await _context.Roles.FindAsync(id);
            if (role is null)   return null;
            return MapToResponseDto(role);
        }
        // Create
        public async Task<RoleResponseDto> CreateAsync(RoleCreateDto dto)
        {
            await RoleNameExists(dto.RoleName!);
            var role = new Role
            {
                RoleName = dto.RoleName,
                Description = dto.Description
            };
            await _context.Roles.AddAsync(role);
            await _context.SaveChangesAsync();
            return MapToResponseDto(role);
        }
        // Update
        public async Task<RoleResponseDto?> UpdateAsync(int id, RoleUpdateDto dto)
        {
            var role = await _context.Roles
                                     .FirstOrDefaultAsync(r => r.RoleId == id  
                                                           && (r.IsDeleted == ActiveEnums.Active));
            if (role is null) return null;
            await RoleNameExists(dto.RoleName!, id);
            role.RoleName = dto.RoleName;
            role.Description = dto.Description;
            await _context.SaveChangesAsync();
            return MapToResponseDto(role);
        }
        // Delete
        public async Task<bool> DeleteAsync(int id)
        {
            var role = await _context.Roles
                                     .FirstOrDefaultAsync(r => r.RoleId == id
                                                           && (r.IsDeleted == ActiveEnums.Inactive));
            if (role is null) return false;
            var user = await _context.Users.AnyAsync(u => u.RoleId == id
                                                      && (u.IsDeleted == ActiveEnums.Active));
            if (user)
                throw new BadRequestException("Vai trò đang được sử dụng. Không thể xóa!");
            _context.Roles.Remove(role);
            await _context.SaveChangesAsync();
            return true;
        }
        // Soft delete
        public async Task<RoleResponseDto?> SoftDeleteAsync(int id)
        {
            // Lấy role 
            var role = await _context.Roles
                                     .FirstOrDefaultAsync(r => r.RoleId == id  
                                                            && r.IsDeleted == ActiveEnums.Active);
            if (role is null) return null;
            // Check RoleId có được sử dụng không?
            var user = await _context.Users.AnyAsync(u => u.RoleId == id
                                                      && (u.IsDeleted == ActiveEnums.Active));
            if (user)
                throw new BadRequestException("Vai trò đang được sử dụng. Không thể xóa!");
            role.IsDeleted = ActiveEnums.Inactive;
            await _context.SaveChangesAsync();
            return MapToResponseDto(role);
        }
        // Khôi phục (Restore)
        public async Task<RoleResponseDto?> RestoreAsync(int id)
        {
            var role = await _context.Roles
                                     .FirstOrDefaultAsync(r => r.RoleId == id
                                                            && r.IsDeleted == ActiveEnums.Inactive);
            if (role is null) return null;
            role.IsDeleted = ActiveEnums.Active;
            await _context.SaveChangesAsync();
            return MapToResponseDto(role);
        }
    }
}
