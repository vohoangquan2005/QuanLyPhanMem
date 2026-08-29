using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.RegularExpressions;
using HotelManagement.Data;
using HotelManagement.DTOs.Auth;
using HotelManagement.Enums;
using HotelManagement.Exceptions;
using HotelManagement.Models;
using HotelManagement.Settings;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace HotelManagement.Services
{
    public class AuthService : IAuthService
    {
        private readonly AppDbContext _context;
        private readonly PasswordHasher<User> _passwordHasher;
        private readonly JwtSettings _jwtSettings;
        private readonly IConfiguration _configuration;
        private readonly AuditLogService _auditLogService;
        private readonly CloudinaryService _cloudinaryService;
        private readonly ILogger<AuthService> _logger;
        public AuthService(AppDbContext context,
                            PasswordHasher<User> passwordHasher,
                            IConfiguration configuration,
                            AuditLogService auditLogService,
                            CloudinaryService cloudinaryService,
                            ILogger<AuthService> logger,
                            IOptions<JwtSettings>? jwtOptions = null)
        {
            _context = context;
            _passwordHasher = passwordHasher;
            _configuration = configuration;
            _jwtSettings = jwtOptions?.Value ?? _configuration.GetSection("JwtSettings").Get<JwtSettings>() 
                                             ?? new JwtSettings();
            _auditLogService = auditLogService;
            _cloudinaryService = cloudinaryService;
            _logger = logger;
        }
        // ===========================================================
        // =                      TOKEN GENERATION                   =
        // ===========================================================
        private LoginResponseDto GenerateToken(User user)
        {
            if (string.IsNullOrWhiteSpace(_jwtSettings.SecretKey))
            {
                var secretFromConfig = _configuration["JwtSettings:SecretKey"];
                if (string.IsNullOrWhiteSpace(secretFromConfig))
                    throw new InvalidOperationException("Jwt SecretKey chưa được cấu hình!");
                _jwtSettings.SecretKey = secretFromConfig;
            }
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.SecretKey));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var expireMinutes = _jwtSettings.ExpireMinutes > 0 ? _jwtSettings.ExpireMinutes : 60;
            var expiredAt = DateTime.UtcNow.AddMinutes(expireMinutes);
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                new Claim(JwtRegisteredClaimNames.Sub, user.UserId.ToString()),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.Name, user.FullName ?? string.Empty),
                new Claim(ClaimTypes.Email, user.Email ?? string.Empty),
                new Claim(ClaimTypes.Role, user.Role?.RoleName ?? string.Empty)
                
            };
            if (user.Customer != null)
                claims.Add(new Claim("CustomerId", user.Customer.CustomerId.ToString()));
            var token = new JwtSecurityToken(issuer: _jwtSettings.Issuer,
                                             audience: _jwtSettings.Audience,
                                             claims: claims,
                                             expires: expiredAt,
                                             signingCredentials: credentials);
            var tokenString = new JwtSecurityTokenHandler().WriteToken(token);
            return new LoginResponseDto
            {
                Token = tokenString,
                ExpiredAt = expiredAt,
                UserId = user.UserId,
                CustomerId = user.Customer?.CustomerId,
                FullName = user.FullName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                RoleName = user.Role?.RoleName,
                Avatar = user.Avatar
            };
        }
        // ===========================================================
        // =                         REGISTER                        =
        // ===========================================================
        public async Task<LoginResponseDto> RegisterAsync(RegisterDto dto)
        {
            // Chuẩn hóa dữ liệu đầu vào
            var fullName = dto.FullName?.Trim() ?? string.Empty;
            var email = dto.Email?.Trim().ToLower() ?? string.Empty;
            var phoneNumber = dto.PhoneNumber?.Trim() ?? string.Empty;
            var cccd = dto.CCCD?.Trim();

            // Validate mật khẩu xác nhận
            if (dto.Password != dto.ConfirmPassword)
                throw new BadRequestException("Mật khẩu xác nhận không khớp!");
            // Validate định dạng số điện thoại (10 chữ số, bắt đầu bằng 0)
            if (!Regex.IsMatch(phoneNumber, @"^0\d{9}$"))
                throw new BadRequestException("Số điện thoại không hợp lệ (phải gồm 10 chữ số và bắt đầu bằng số 0)!");
            // Kiểm tra trùng Email
            var emailExists = await _context.Users.AnyAsync(u => u.Email!.ToLower() == email);
            if (emailExists)
                throw new ConflictException("Email đã tồn tại trong hệ thống!");
            // Kiểm tra trùng Số điện thoại
            var phoneExists = await _context.Users.AnyAsync(u => u.PhoneNumber == phoneNumber);
            if (phoneExists)
                throw new ConflictException("Số điện thoại đã tồn tại trong hệ thống!");
            // Kiểm tra CCCD nếu có cung cấp
            if (!string.IsNullOrWhiteSpace(cccd))
            {
                if (!Regex.IsMatch(cccd, @"^\d{9}(\d{3})?$"))
                    throw new BadRequestException("CCCD/CMND phải gồm 9 hoặc 12 chữ số!");
                var cccdExists = await _context.Customers.AnyAsync(c => c.CCCD == cccd);
                if (cccdExists)
                    throw new ConflictException("CCCD/CMND đã tồn tại trong hệ thống!");
            }
            // Tìm vai trò Customer
            var customerRole = await _context.Roles
                .FirstOrDefaultAsync(r => r.RoleName == "Customer" && r.IsDeleted == ActiveEnums.Active);
            if (customerRole is null)
                throw new NotFoundException("Không tìm thấy vai trò Customer trong hệ thống!");
            // Thực hiện giao dịch tạo User và Customer (nếu có CCCD)
            await using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var user = new User
                {
                    FullName = fullName,
                    Email = email,
                    PhoneNumber = phoneNumber,
                    RoleId = customerRole.RoleId,
                    Role = customerRole, // Gán Navigation Property để tạo JWT đầy đủ Role claim
                    IsLocked = ActiveEnums.Active,
                    IsDeleted = ActiveEnums.Active,
                    CreateAt = DateTime.Now
                };
                // Hash password an toàn
                user.HashPassword = _passwordHasher.HashPassword(user, dto.Password);
                await _context.Users.AddAsync(user);
                await _context.SaveChangesAsync();
                // Tạo Customer tương ứng nếu có CCCD
                if (!string.IsNullOrWhiteSpace(cccd))
                {
                    var customer = new Customer
                    {
                        UserId = user.UserId,
                        CCCD = cccd,
                        Nationality = !string.IsNullOrWhiteSpace(dto.Nationality) ? dto.Nationality.Trim() : "Việt Nam",
                        CreateAt = DateTime.Now
                    };
                    await _context.Customers.AddAsync(customer);
                    await _context.SaveChangesAsync();
                    user.Customer = customer;
                }
                await transaction.CommitAsync();
                await _auditLogService.LogAsync(AuditLogEnums.Create,
                                                "Users",
                                                user.UserId,
                                                null,
                                                $"UserId: {user.UserId}, FullName: {user.FullName}, Email: {user.Email}");
                // Tự động đăng nhập và sinh JWT Token
                return GenerateToken(user);
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
        // ===========================================================
        // =                          LOGIN                          =
        // ===========================================================
        public async Task<LoginResponseDto> LoginAsync(LoginDto dto)
        {
            var email = dto.Email?.Trim().ToLower() ?? string.Empty;
            var user = await _context.Users
                                     .Include(u => u.Role)
                                     .Include(u => u.Customer)
                                     .FirstOrDefaultAsync(u => u.Email!.ToLower() == email && u.IsDeleted == ActiveEnums.Active);
            if (user is null)
                throw new BadRequestException("Email hoặc mật khẩu không chính xác!");
            // Kiểm tra tài khoản bị khóa
            if (user.IsLocked == ActiveEnums.Inactive)
                throw new BadRequestException("Tài khoản đã bị khóa. Vui lòng liên hệ quản trị viên!");
            // Kiểm tra vai trò của tài khoản
            if (user.Role == null || user.Role.IsDeleted == ActiveEnums.Inactive)
                throw new BadRequestException("Vai trò của tài khoản không hợp lệ hoặc đã bị vô hiệu hóa!");
            // Kiểm tra mật khẩu
            if (string.IsNullOrWhiteSpace(user.HashPassword))
                throw new BadRequestException("Tài khoản chưa được thiết lập mật khẩu!");
            PasswordVerificationResult verificationResult;
            try
            {
                verificationResult = _passwordHasher.VerifyHashedPassword(user, user.HashPassword, dto.Password);
            }
            catch (FormatException)
            {
                // Xử lý dữ liệu seed cũ hoặc chưa được hash chuẩn
                if (user.HashPassword == dto.Password)
                    verificationResult = PasswordVerificationResult.SuccessRehashNeeded;
                else
                    verificationResult = PasswordVerificationResult.Failed;
            }
            if (verificationResult == PasswordVerificationResult.Failed)
                throw new BadRequestException("Email hoặc mật khẩu không chính xác!");
            // Nâng cấp mã hóa mật khẩu nếu cần thiết
            if (verificationResult == PasswordVerificationResult.SuccessRehashNeeded)
            {
                user.HashPassword = _passwordHasher.HashPassword(user, dto.Password);
                user.UpdateAt = DateTime.Now;
                await _context.SaveChangesAsync();
            }
            var loginResponse = GenerateToken(user);
            await _auditLogService.LogAsync(AuditLogEnums.Login,
                                            "Users",
                                            user.UserId,
                                            null,
                                            $"UserId: {user.UserId}, Email: {user.Email}");
            return loginResponse;
        }

        // ===========================================================
        // =                     CHANGE PASSWORD                     =
        // ===========================================================
        public async Task<bool> ChangePasswordAsync(int userId, ChangePasswordDto dto)
        {
            if (dto.NewPassword != dto.ConfirmNewPassword)
                throw new BadRequestException("Mật khẩu xác nhận không khớp!");
            if (dto.CurrentPassword == dto.NewPassword)
                throw new BadRequestException("Mật khẩu mới không được trùng với mật khẩu hiện tại!");
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.UserId == userId && u.IsDeleted == ActiveEnums.Active);
            if (user is null)
                throw new NotFoundException("Người dùng không tồn tại hoặc đã bị vô hiệu hóa!");
            if (user.IsLocked == ActiveEnums.Inactive)
                throw new BadRequestException("Tài khoản đã bị khóa!");
            if (string.IsNullOrWhiteSpace(user.HashPassword))
                throw new BadRequestException("Tài khoản chưa được thiết lập mật khẩu!");
            PasswordVerificationResult verifyResult;
            try
            {
                verifyResult = _passwordHasher.VerifyHashedPassword(user, user.HashPassword, dto.CurrentPassword);
            }
            catch (FormatException)
            {
                verifyResult = (user.HashPassword == dto.CurrentPassword)
                    ? PasswordVerificationResult.Success
                    : PasswordVerificationResult.Failed;
            }
            if (verifyResult == PasswordVerificationResult.Failed)
                throw new BadRequestException("Mật khẩu hiện tại không chính xác!");
            user.HashPassword = _passwordHasher.HashPassword(user, dto.NewPassword);
            user.UpdateAt = DateTime.Now;
            await _context.SaveChangesAsync();
            await _auditLogService.LogAsync(AuditLogEnums.Update,
                                            "Users",
                                            user.UserId,
                                            null,
                                            "Thay đổi mật khẩu");
            return true;
        }
        // ===========================================================
        // =                       USER PROFILE                      =
        // ===========================================================
        public async Task<UserProfileResponseDto> GetProfileAsync(int userId)
        {
            var user = await _context.Users
                .Include(u => u.Role)
                .Include(u => u.Customer)
                .FirstOrDefaultAsync(u => u.UserId == userId && u.IsDeleted == ActiveEnums.Active);

            if (user is null)
                throw new NotFoundException("Người dùng không tồn tại hoặc đã bị vô hiệu hóa!");

            return new UserProfileResponseDto
            {
                UserId = user.UserId,
                FullName = user.FullName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                Gender = user.Gender,
                DateOfBirth = user.DateOfBirth,
                Avatar = user.Avatar,
                RoleId = user.RoleId,
                RoleName = user.Role?.RoleName,
                CustomerId = user.Customer?.CustomerId,
                CCCD = user.Customer?.CCCD,
                Nationality = user.Customer?.Nationality,
                CreateAt = user.CreateAt,
                UpdateAt = user.UpdateAt
            };
        }
        // Update Profile
        public async Task<UserProfileResponseDto> UpdateProfileAsync(int userId, UpdateProfileDto dto)
        {
            var user = await _context.Users.Include(u => u.Role)
                                           .Include(u => u.Customer)
                                           .FirstOrDefaultAsync(u => u.UserId == userId && u.IsDeleted == ActiveEnums.Active);
            if (user is null)
                throw new NotFoundException("Người dùng không tồn tại hoặc đã bị xóa!");
            // Lưu thông tin User cũ
            var oldValue = $"FullName: {user.FullName}, " +
                           $"PhoneNumber: {user.PhoneNumber}, " +
                           $"Gender: {user.Gender}, " +
                           $"DateOfBirth: {user.DateOfBirth}, " +
                           $"CCCD: {user.Customer?.CCCD}, " +
                           $"Nationality: {user.Customer?.Nationality}";
            if (user.IsLocked == ActiveEnums.Inactive)
                throw new BadRequestException("Tài khoản đã bị khóa!");
            var phone = dto.PhoneNumber.Trim();
            if (!Regex.IsMatch(phone, @"^0\d{9}$"))
                throw new BadRequestException("Số điện thoại không hợp lệ (phải gồm 10 chữ số và bắt đầu bằng số 0)!");
            // Kiểm tra số điện thoại không trùng với user khác
            var phoneExists = await _context.Users
                .AnyAsync(u => u.PhoneNumber == phone && u.UserId != userId);
            if (phoneExists)
                throw new ConflictException("Số điện thoại đã được sử dụng bởi tài khoản khác!");
            // Kiểm tra ngày sinh hợp lệ
            if (dto.DateOfBirth.HasValue && dto.DateOfBirth.Value.Date > DateTime.Today)
                throw new BadRequestException("Ngày sinh không được lớn hơn ngày hiện tại!");
            await using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                user.FullName = dto.FullName.Trim();
                user.PhoneNumber = phone;
                user.Gender = dto.Gender;
                user.DateOfBirth = dto.DateOfBirth;
                if (!string.IsNullOrWhiteSpace(dto.Avatar))
                    user.Avatar = dto.Avatar;
                user.UpdateAt = DateTime.Now;
                // Xử lý thông tin CCCD nếu có
                if (!string.IsNullOrWhiteSpace(dto.CCCD))
                {
                    var cccd = dto.CCCD.Trim();
                    if (!Regex.IsMatch(cccd, @"^\d{9}(\d{3})?$"))
                        throw new BadRequestException("CCCD/CMND phải gồm 9 hoặc 12 chữ số!");
                    var cccdExists = await _context.Customers
                        .AnyAsync(c => c.CCCD == cccd && c.UserId != userId);
                    if (cccdExists)
                        throw new ConflictException("CCCD/CMND đã được sử dụng bởi khách hàng khác!");
                    if (user.Customer == null)
                    {
                        var customer = new Customer
                        {
                            UserId = user.UserId,
                            CCCD = cccd,
                            Nationality = !string.IsNullOrWhiteSpace(dto.Nationality) ? dto.Nationality.Trim() : "Việt Nam",
                            CreateAt = DateTime.Now
                        };
                        await _context.Customers.AddAsync(customer);
                        user.Customer = customer;
                    }
                    else
                    {
                        user.Customer.CCCD = cccd;
                        if (!string.IsNullOrWhiteSpace(dto.Nationality))
                            user.Customer.Nationality = dto.Nationality.Trim();
                        user.Customer.UpdateAt = DateTime.Now;
                    }
                }
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                await _auditLogService.LogAsync(AuditLogEnums.Update,
                                                "Users",
                                                user.UserId,
                                                oldValue,
                                                $"FullName: {user.FullName}, " +
                                                $"PhoneNumber: {user.PhoneNumber}, " +
                                                $"Gender: {user.Gender}, " +
                                                $"DateOfBirth: {user.DateOfBirth}, " +
                                                $"CCCD: {user.Customer?.CCCD}, " +
                                                $"Nationality: {user.Customer?.Nationality}");
                return await GetProfileAsync(userId);
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<UserProfileResponseDto> UpdateMyProfileAsync(int userId, UpdateMyProfileDto dto)
        {
            var user = await _context.Users
                .Include(u => u.Role)
                .Include(u => u.Customer)
                .FirstOrDefaultAsync(u => u.UserId == userId && u.IsDeleted == ActiveEnums.Active);

            if (user is null)
                throw new NotFoundException("Người dùng không tồn tại hoặc đã bị vô hiệu hóa!");
            if (user.IsLocked == ActiveEnums.Inactive)
                throw new BadRequestException("Tài khoản đã bị khóa!");

            var fullName = dto.FullName.Trim();
            var email = dto.Email.Trim().ToLowerInvariant();
            var phoneNumber = dto.PhoneNumber.Trim();

            if (string.IsNullOrWhiteSpace(fullName))
                throw new BadRequestException("Họ tên không được để trống!");
            if (!Regex.IsMatch(phoneNumber, @"^0\d{9}$"))
                throw new BadRequestException("Số điện thoại phải gồm 10 chữ số và bắt đầu bằng số 0!");
            if (dto.DateOfBirth.HasValue && dto.DateOfBirth.Value.Date > DateTime.Today)
                throw new BadRequestException("Ngày sinh không được lớn hơn ngày hiện tại!");

            if (await _context.Users.AnyAsync(u => u.UserId != userId
                && u.Email != null && u.Email.ToLower() == email))
                throw new ConflictException("Email đã được sử dụng bởi tài khoản khác!");
            if (await _context.Users.AnyAsync(u => u.UserId != userId
                && u.PhoneNumber == phoneNumber))
                throw new ConflictException("Số điện thoại đã được sử dụng bởi tài khoản khác!");

            var oldAvatar = user.Avatar;
            string? uploadedAvatar = null;
            if (dto.Avatar is { Length: > 0 })
            {
                uploadedAvatar = await _cloudinaryService.UploadImageAsync(
                    dto.Avatar,
                    CloudinaryService.UserAvatarFolder);
            }

            var oldValue = $"FullName: {user.FullName}, Email: {user.Email}, PhoneNumber: {user.PhoneNumber}, DateOfBirth: {user.DateOfBirth}, Avatar: {user.Avatar}";
            try
            {
                user.FullName = fullName;
                user.Email = email;
                user.PhoneNumber = phoneNumber;
                user.DateOfBirth = dto.DateOfBirth;
                if (uploadedAvatar is not null)
                    user.Avatar = uploadedAvatar;
                user.UpdateAt = DateTime.Now;

                await _context.SaveChangesAsync();
            }
            catch
            {
                await TryDeleteAvatarAsync(uploadedAvatar);
                throw;
            }

            try
            {
                await _auditLogService.LogAsync(
                    AuditLogEnums.Update,
                    "Users",
                    user.UserId,
                    oldValue,
                    $"FullName: {user.FullName}, Email: {user.Email}, PhoneNumber: {user.PhoneNumber}, DateOfBirth: {user.DateOfBirth}, Avatar: {user.Avatar}");
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Audit log failed after profile update for userId {UserId}", userId);
            }

            if (uploadedAvatar is not null && !string.Equals(oldAvatar, uploadedAvatar, StringComparison.Ordinal))
                await TryDeleteAvatarAsync(oldAvatar);

            return await GetProfileAsync(userId);
        }

        private async Task TryDeleteAvatarAsync(string? avatarUrl)
        {
            if (string.IsNullOrWhiteSpace(avatarUrl)) return;
            try
            {
                await _cloudinaryService.DeleteImageAsync(avatarUrl, CloudinaryService.UserAvatarFolder);
            }
            catch (Exception exception)
            {
                _logger.LogWarning(exception, "Could not clean up profile avatar {AvatarUrl}.", avatarUrl);
            }
        }
    }
}
