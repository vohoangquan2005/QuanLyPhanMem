using Azure.Core;
using HotelManagement.Data;
using HotelManagement.DTOs.User;
using HotelManagement.Enums;
using HotelManagement.Exceptions;
using HotelManagement.Models;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;

namespace HotelManagement.Services
{
    public class UserService
    {
        private readonly AppDbContext _context;
        private readonly AuditLogService _auditLogService;
        private readonly CloudinaryService _cloudinaryService;
        private readonly ILogger<UserService> _logger;

        public UserService(
            AppDbContext context,
            AuditLogService auditLogService,
            CloudinaryService cloudinaryService,
            ILogger<UserService> logger)
        {
            _context = context;
            _auditLogService = auditLogService;
            _cloudinaryService = cloudinaryService;
            _logger = logger;
        }
        // Chuyển User -> UserResponseDto
        private UserResponseDto MapToResponseDto(User user)
        {
            return new UserResponseDto
            {
                UserId = user.UserId,
                FullName = user.FullName,
                Email = user.Email,
                RoleId = user.RoleId,
                RoleName = user.Role?.RoleName,
                IsLocked = user.IsLocked,
                Gender = user.Gender,
                DateOfBirth = user.DateOfBirth,
                PhoneNumber = user.PhoneNumber,
                Avatar = user.Avatar,
                IsDeleted = user.IsDeleted,
                CreateAt = user.CreateAt,
                UpdateAt = user.UpdateAt
            };
        }
        // Chuyển UserCreateDto -> User
        private User MapToEntity(UserCreateDto dto, string? avatarUrl)
        {
            return new User
            {
                FullName = dto.FullName,
                Email = dto.Email,
                HashPassword = dto.HashPassword,
                RoleId = dto.RoleId,
                Gender = dto.Gender,
                DateOfBirth = dto.DateOfBirth,
                PhoneNumber = dto.PhoneNumber,
                Avatar = avatarUrl
            };
        }
        // Cập nhật User từ UserUpdateDto
        private void UpdateEntity(User user, UserUpdateDto dto)
        {
            user.FullName = dto.FullName;
            user.Email = dto.Email;
            user.HashPassword = dto.HashPassword;
            user.RoleId = dto.RoleId;
            user.IsLocked = dto.IsLocked;
            user.Gender = dto.Gender;
            user.DateOfBirth = dto.DateOfBirth;
            user.PhoneNumber = dto.PhoneNumber;
        }
        // ===========================================================
        // =                         VALIDATE                        =
        // ===========================================================
        // Lấy Role đang Active
        private async Task<Role> GetActiveRole(int roleId)
        {
            var role = await _context.Roles
                                     .FirstOrDefaultAsync(r => r.RoleId == roleId &&
                                                               r.IsDeleted == ActiveEnums.Active);
            if (role is null)
                throw new NotFoundException("Vai trò không tồn tại hoặc đã bị xóa!");
            return role;
        }
        // Check trùng Email
        private async Task EmailExists(string email, int? userId = null)
        {
            var exists = await _context.Users.AnyAsync(u => u.Email == email
                                                    &&(userId == null || u.UserId != userId));
            // userId == null để check khi create User
            // u.userId = userId để check khi update User
            if (exists)
                throw new ConflictException("Email đã tồn tại!");
        }
        // Check trùng Số điện thoại
        private async Task PhoneNumberExists(string? phoneNumber, int? userId = null)
        {
            if (string.IsNullOrWhiteSpace(phoneNumber)) return;
            var exists = await _context.Users.AnyAsync(u => u.PhoneNumber == phoneNumber
                                                    &&(userId == null || u.UserId != userId));
            if (exists)
                throw new ConflictException("Số điện thoại đã tồn tại");
        }
        // Check ngày sinh
        private void CheckDateOfBirth(DateTime? dateOfBirth)
        {
            if (dateOfBirth.HasValue && dateOfBirth.Value.Date > DateTime.Today)
            {
                throw new BadRequestException("Ngày sinh không được lớn hơn ngày hiện tại!");
            }
        }
        // Check CCCD
        private void CheckCCCD(string? cccd)
        {
            if (cccd is not null && !cccd.All(char.IsDigit))
                throw new BadRequestException("CCCD phải là số!");
        }
        // Check trùng CCCD
        private async Task CCCDExists(string? cccd, int? customerId = null)
        {
            if (string.IsNullOrWhiteSpace(cccd)) return;
            var exists = await _context.Customers
                                       .AnyAsync(c => c.CCCD == cccd 
                                                  && (customerId == null || customerId != c.CustomerId));
            if (exists)
                throw new ConflictException("CCCD đã tồn tại!");
        }
        // ===========================================================
        // =                       CUSTOMER                          =
        // ===========================================================
        // Create Customer
        private async Task CreateCustomer(int userId, CustomerCreateDto dto)
        {
            CheckCCCD(dto.CCCD);
            await CCCDExists(dto.CCCD);
            var customer = new Customer
            {
                UserId = userId,
                CCCD = dto.CCCD,
                Nationality = dto.Nationality,
            };
            await _context.Customers.AddAsync(customer);
        }
        // Update Customer
        private async Task UpdateCustomer(int userId, CustomerUpdateDto dto)
        {
            CheckCCCD(dto.CCCD);
            var customer = await _context.Customers
                                         .FirstOrDefaultAsync(c => c.UserId == userId);
            // User chưa có Customer -> tạo mới
            if (customer is null)
            {
                await CCCDExists(dto.CCCD);
                customer = new Customer
                {
                    UserId = userId,
                    CCCD = dto.CCCD,
                    Nationality = dto.Nationality
                };
                await _context.Customers.AddAsync(customer);
                return;
            }
            // User đã có Customer -> cập nhật
            await CCCDExists(dto.CCCD, customer.CustomerId);
            customer.CCCD = dto.CCCD;
            customer.Nationality = dto.Nationality;
        }
        private async Task TryDeleteUploadedAvatarAsync(string? avatarUrl)
        {
            if (string.IsNullOrWhiteSpace(avatarUrl))
            {
                return;
            }

            try
            {
                await _cloudinaryService.DeleteImageAsync(
                    avatarUrl,
                    CloudinaryService.UserAvatarFolder);
            }
            catch (Exception exception)
            {
                _logger.LogError(
                    exception,
                    "Failed to clean up a newly uploaded user avatar after a database failure.");
            }
        }

        private async Task TryDeleteOldAvatarAsync(string? avatarUrl)
        {
            if (string.IsNullOrWhiteSpace(avatarUrl))
            {
                return;
            }

            try
            {
                await _cloudinaryService.DeleteImageAsync(
                    avatarUrl,
                    CloudinaryService.UserAvatarFolder);
            }
            catch (Exception exception)
            {
                _logger.LogError(
                    exception,
                    "User data was saved, but the old Cloudinary avatar could not be deleted.");
            }
        }

        // ===========================================================
        // =                           CRUD                          =
        // ===========================================================
        // Get all Active(Lấy users còn hoạt động)
        public async Task<List<UserResponseDto>> GetAllAsync()
        {
            var users = await _context.Users.Where(u => u.IsDeleted == ActiveEnums.Active)
                                            .OrderBy(u => u.Role!.RoleName)
                                                .ThenBy(u => u.FullName)
                                            .Select(u => new UserResponseDto
                                            {
                                                UserId = u.UserId,
                                                FullName = u.FullName,
                                                Email = u.Email,
                                                RoleId = u.RoleId,
                                                RoleName = u.Role!.RoleName,
                                                IsLocked = u.IsLocked,
                                                Gender = u.Gender,
                                                DateOfBirth = u.DateOfBirth,
                                                PhoneNumber = u.PhoneNumber,
                                                Avatar = u.Avatar,
                                                IsDeleted = u.IsDeleted,
                                                CreateAt = u.CreateAt,
                                                UpdateAt = u.UpdateAt
                                            }).ToListAsync();
            return users;
        }
        // Get all InActive(Lấy users không hoạt động)
        public async Task<List<UserResponseDto>> GetDeletedAsync()
        {
            var users = await _context.Users.Where(u => u.IsDeleted == ActiveEnums.Inactive)
                                            .OrderBy(u => u.Role!.RoleName)
                                                .ThenBy(u => u.FullName)
                                            .Select(u => new UserResponseDto
                                            {
                                                UserId = u.UserId,
                                                FullName = u.FullName,
                                                Email = u.Email,
                                                RoleId = u.RoleId,
                                                RoleName = u.Role!.RoleName,
                                                IsLocked = u.IsLocked,
                                                Gender = u.Gender,
                                                DateOfBirth = u.DateOfBirth,
                                                PhoneNumber = u.PhoneNumber,
                                                Avatar = u.Avatar,
                                                IsDeleted = u.IsDeleted,
                                                CreateAt = u.CreateAt,
                                                UpdateAt = u.UpdateAt
                                            }).ToListAsync();
            return users;
        }
        // Get by Id (Chỉ lấy những User còn hoạt động
        public async Task<UserResponseDto?> GetByIdAsync(int id)
        {
            var user = await _context.Users.Include(u => u.Role)
                                           .FirstOrDefaultAsync(u => u.UserId == id
                                                                  && u.IsDeleted == ActiveEnums.Active);
            if (user is null)   return null;
            return MapToResponseDto(user);
        }
        // Create
        public async Task<UserResponseDto> CreateAsync(UserCreateRequestDto request)
        {
            await using var transaction = await _context.Database.BeginTransactionAsync();
            string? uploadedAvatarUrl = null;

            try
            {
                var role = await GetActiveRole(request.User.RoleId);
                await EmailExists(request.User.Email!);
                await PhoneNumberExists(request.User.PhoneNumber);
                CheckDateOfBirth(request.User.DateOfBirth);

                if (request.Avatar is { Length: > 0 })
                {
                    uploadedAvatarUrl = await _cloudinaryService.UploadImageAsync(
                        request.Avatar,
                        CloudinaryService.UserAvatarFolder);
                }

                var user = MapToEntity(request.User, uploadedAvatarUrl);
                await _context.Users.AddAsync(user);
                await _context.SaveChangesAsync();

                if (role.RoleName == "Customer")
                {
                    if (request.Customer is null)
                    {
                        throw new BadRequestException("Vui lòng cung cấp thông tin khách hàng!");
                    }

                    await CreateCustomer(user.UserId, request.Customer);
                }

                await _context.SaveChangesAsync();
                await _auditLogService.LogAsync(
                    AuditLogEnums.Create,
                    "User",
                    user.UserId,
                    null,
                    $"FullName: {user.FullName}, "
                    + $"Email: {user.Email}, "
                    + $"RoleId: {user.RoleId}, "
                    + $"PhoneNumber: {user.PhoneNumber}"
                    + (role.RoleName == "Customer"
                        ? $", CCCD: {request.Customer?.CCCD}, "
                            + $"Nationality: {request.Customer?.Nationality}"
                        : ""));

                var response = await GetByIdAsync(user.UserId);
                await transaction.CommitAsync();
                return response!;
            }
            catch
            {
                await transaction.RollbackAsync();
                await TryDeleteUploadedAvatarAsync(uploadedAvatarUrl);
                throw;
            }
        }

        // Update
        public async Task<UserResponseDto?> UpdateAsync(
            int id,
            UserUpdateRequestDto request)
        {
            await using var transaction = await _context.Database.BeginTransactionAsync();
            string? uploadedAvatarUrl = null;
            string? oldAvatarUrl = null;

            try
            {
                var user = await _context.Users
                    .FirstOrDefaultAsync(
                        current => current.UserId == id
                            && current.IsDeleted == ActiveEnums.Active);

                if (user is null)
                {
                    return null;
                }

                await EmailExists(request.User.Email!, id);
                await PhoneNumberExists(request.User.PhoneNumber, id);
                CheckDateOfBirth(request.User.DateOfBirth);

                var newRole = await GetActiveRole(request.User.RoleId);
                if (newRole.RoleName == "Customer")
                {
                    if (request.Customer is null)
                    {
                        throw new BadRequestException("Vui lòng cung cấp thông tin khách hàng!");
                    }

                    await UpdateCustomer(id, request.Customer);
                }

                var oldValue = $"FullName: {user.FullName}, "
                    + $"Email: {user.Email}, "
                    + $"RoleId: {user.RoleId}, "
                    + $"IsLocked: {user.IsLocked}, "
                    + $"Gender: {user.Gender}, "
                    + $"DateOfBirth: {user.DateOfBirth}, "
                    + $"PhoneNumber: {user.PhoneNumber}, "
                    + $"Avatar: {user.Avatar}";

                oldAvatarUrl = user.Avatar;
                if (request.Avatar is { Length: > 0 })
                {
                    uploadedAvatarUrl = await _cloudinaryService.UploadImageAsync(
                        request.Avatar,
                        CloudinaryService.UserAvatarFolder);
                }

                UpdateEntity(user, request.User);
                if (uploadedAvatarUrl is not null)
                {
                    user.Avatar = uploadedAvatarUrl;
                }

                await _context.SaveChangesAsync();

                var newValue = $"FullName: {user.FullName}, "
                    + $"Email: {user.Email}, "
                    + $"RoleId: {user.RoleId}, "
                    + $"IsLocked: {user.IsLocked}, "
                    + $"Gender: {user.Gender}, "
                    + $"DateOfBirth: {user.DateOfBirth}, "
                    + $"PhoneNumber: {user.PhoneNumber}, "
                    + $"Avatar: {user.Avatar}";

                await _auditLogService.LogAsync(
                    AuditLogEnums.Update,
                    "User",
                    user.UserId,
                    oldValue,
                    newValue);

                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                await TryDeleteUploadedAvatarAsync(uploadedAvatarUrl);
                throw;
            }

            var response = await GetByIdAsync(id);
            if (uploadedAvatarUrl is not null
                && !string.Equals(oldAvatarUrl, uploadedAvatarUrl, StringComparison.Ordinal))
            {
                await TryDeleteOldAvatarAsync(oldAvatarUrl);
            }

            return response;
        }

        // Delete
        public async Task<bool> DeleteAsync(int id)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(
                    current => current.UserId == id
                        && current.IsDeleted == ActiveEnums.Inactive);

            if (user is null)
            {
                return false;
            }

            var customer = await _context.Customers.AnyAsync(item => item.UserId == id);
            if (customer)
            {
                throw new BadRequestException(
                    "Người dùng đang có thông tin khách hàng. Không thể xóa!");
            }

            var avatarUrl = user.Avatar;
            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            await TryDeleteOldAvatarAsync(avatarUrl);
            return true;
        }

        // Xóa tạm
        public async Task<UserResponseDto?> SoftDeleteAsync(int id)
        {
            var user = await _context.Users
                                     .Include(u => u.Role)
                                     .FirstOrDefaultAsync(u => u.UserId == id
                                                             && u.IsDeleted == ActiveEnums.Active);
            if (user is null) return null;
            user.IsDeleted = ActiveEnums.Inactive;
            await _context.SaveChangesAsync();
            await _auditLogService.LogAsync(AuditLogEnums.SoftDelete,
                                            "User",
                                            user.UserId,
                                            "IsDeleted: Active",
                                            "IsDeleted: Inactive");
            return MapToResponseDto(user);
        }
        // Khôi phục (Restore)
        public async Task<UserResponseDto?> RestoreAsync(int id)
        {
            var user = await _context.Users
                                     .FirstOrDefaultAsync(u => u.UserId == id
                                                             && u.IsDeleted == ActiveEnums.Inactive);
            if (user is null) return null;
            user.IsDeleted = ActiveEnums.Active;
            await _context.SaveChangesAsync();
            await _auditLogService.LogAsync(AuditLogEnums.Restore,
                                            "User",
                                            user.UserId,
                                            "IsDeleted: Inactive",
                                            "IsDeleted: Active");
            return await GetByIdAsync(id);
        }
    }
}
