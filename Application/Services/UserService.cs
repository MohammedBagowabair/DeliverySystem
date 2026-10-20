using Application.Common.Models;
using Application.DTO.UserDtos;
using Application.Helpers;
using Application.Interfaces;
using Domain.Common.Models;
using Domain.Constants;
using Domain.Entities;
using Domain.Exceptions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Application.Services
{
    public class UserService : IUserService
    {
        private readonly IApplicationDbContext _dbContext;
        private readonly IConfiguration _configuration;
        private readonly ILogger<UserService> _logger;

        public UserService(
            IApplicationDbContext dbContext,
            IConfiguration configuration,
            ILogger<UserService> logger)
        {
            _dbContext = dbContext;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<IEnumerable<User>> GetAll()
        {
            return await _dbContext.GetAsync<User>();
        }

        public async Task<PagedList<User>> SearchUsersAsync(string searchTerm, int page, int pageSize)
        {
            searchTerm = searchTerm?.Trim()?.ToLower();
            if (string.IsNullOrEmpty(searchTerm))
            {
                return await _dbContext.GetPagedAsync<User>(page, pageSize);
            }

            return await _dbContext.GetPagedAsync<User>(
                page,
                pageSize,
                d => d.FullName.ToLower().Contains(searchTerm) || d.PhoneNumber1.Contains(searchTerm)
            );
        }

        public async Task<User> Create(createUserDTO createUserDTO)
        {
            var userInDb = (await _dbContext.GetAsync<User>(x => x.Email == createUserDTO.Email))?.FirstOrDefault();
            if (userInDb != null)
            {
                throw new DeliveryCoreException(ErrorCodes.USER_ALREADY_EXISTS_CODE);
            }

            IdentityHelpers.GetPasswordHashAndSalt(createUserDTO.Password, out string passwordHash, out string passwordSalt);

            var role = createUserDTO.Role?.Trim();
            if (role != Roles.Admin && role != Roles.Staff)
            {
                throw new DeliveryCoreException(
                    "يجب أن يكون الدور Admin أو Staff فقط.",
                    ErrorCodes.ROLE_NAME_NOT_FOUND,
                    null);
            }

            var user = new User
            {
                Role = role,
                PasswordHash = passwordHash,
                PasswordSalt = passwordSalt,
                Email = createUserDTO.Email,
                FullName = createUserDTO.FullName,
                Address = createUserDTO.Address,
                PhoneNumber1 = createUserDTO.PhoneNumber1,
                PhoneNumber2 = createUserDTO.PhoneNumber2,
                IsActive = createUserDTO.IsActive,
                Permissions = StaffPermissions.NormalizeForRole(role, createUserDTO.Permissions)
            };

            return await _dbContext.AddAsync(user);
        }

        public async Task<User> GetById(int id)
        {
            return (await _dbContext.GetAsync<User>(x => x.Id == id))?.FirstOrDefault();
        }

        public async Task<bool> Delete(int id)
        {
            return await _dbContext.DeleteAsync<User>(id);
        }

        public async Task Update(UpdateUserDTO updateUserDTO)
        {
            var userInDb = (await _dbContext.GetAsync<User>(x => x.Id == updateUserDTO.Id))?.FirstOrDefault();
            if (userInDb != null)
            {
                userInDb.FullName = updateUserDTO.FullName;
                userInDb.Address = updateUserDTO.Address;
                userInDb.PhoneNumber1 = updateUserDTO.PhoneNumber1;
                userInDb.PhoneNumber2 = updateUserDTO.PhoneNumber2;
                userInDb.IsActive = updateUserDTO.IsActive;

                if (!string.IsNullOrWhiteSpace(updateUserDTO.Email))
                    userInDb.Email = updateUserDTO.Email.Trim();

                var role = updateUserDTO.Role?.Trim();
                if (!string.IsNullOrWhiteSpace(role))
                {
                    if (role != Roles.Admin && role != Roles.Staff)
                    {
                        throw new DeliveryCoreException(
                            "يجب أن يكون الدور Admin أو Staff فقط.",
                            ErrorCodes.ROLE_NAME_NOT_FOUND,
                            null);
                    }
                    userInDb.Role = role;
                }

                userInDb.Permissions = StaffPermissions.NormalizeForRole(
                    userInDb.Role,
                    updateUserDTO.Permissions);

                await _dbContext.UpdateAsync(userInDb);
            }
            else
            {
                throw new DeliveryCoreException(ErrorCodes.DATABASE_RECORD_NOT_FOUND_CODE);
            }
        }

        public PermissionCatalogDTO GetPermissionCatalog()
        {
            return new PermissionCatalogDTO
            {
                Permissions = StaffPermissions.BuildCatalogItems()
                    .Select(p => new PermissionCatalogItemDTO
                    {
                        Code = p.Code,
                        LabelAr = p.LabelAr,
                        DescriptionAr = p.DescriptionAr,
                        Group = p.Group,
                        Implies = p.Implies.ToList()
                    })
                    .ToList(),
                Presets = StaffPermissions.Presets
                    .Select(p => new PermissionPresetDTO
                    {
                        Id = p.Id,
                        LabelAr = p.LabelAr,
                        DescriptionAr = p.DescriptionAr,
                        Permissions = p.Permissions.ToList()
                    })
                    .ToList()
            };
        }

        public async Task<UpdateUserPermissionsResultDTO> UpdatePermissionsAsync(int userId, UpdateUserPermissionsDTO dto)
        {
            var userInDb = (await _dbContext.GetAsync<User>(x => x.Id == userId))?.FirstOrDefault()
                ?? throw new DeliveryCoreException(ErrorCodes.DATABASE_RECORD_NOT_FOUND_CODE);

            if (string.Equals(userInDb.Role, Roles.Admin, StringComparison.OrdinalIgnoreCase))
            {
                userInDb.Permissions = StaffPermissions.NormalizeForRole(Roles.Admin, null);
                await _dbContext.UpdateAsync(userInDb);
                return new UpdateUserPermissionsResultDTO
                {
                    UserId = userInDb.Id,
                    Permissions = StaffPermissions.All.ToList(),
                    RequiresReLogin = true,
                    Message = "المدير لديه كل الصلاحيات تلقائياً. يجب إعادة تسجيل الدخول لتطبيق أي تغييرات."
                };
            }

            userInDb.Permissions = StaffPermissions.NormalizeForRole(Roles.Staff, dto?.Permissions);
            await _dbContext.UpdateAsync(userInDb);

            var saved = StaffPermissions.Parse(userInDb.Permissions);
            return new UpdateUserPermissionsResultDTO
            {
                UserId = userInDb.Id,
                Permissions = saved,
                RequiresReLogin = true
            };
        }

        public async Task ChangePasswordAsync(int userId, ChangePasswordDto dto)
        {
            if (dto.NewPassword != dto.ConfirmPassword)
                throw new DeliveryCoreException(ErrorCodes.USER_PASSWORD_NOT_MATCHED_CODE);

            var user = (await _dbContext.GetAsync<User>(x => x.Id == userId))?.FirstOrDefault()
                ?? throw new DeliveryCoreException(ErrorCodes.DATABASE_RECORD_NOT_FOUND_CODE);

            if (!IdentityHelpers.ValidatePassword(dto.CurrentPassword, user.PasswordHash, user.PasswordSalt))
                throw new DeliveryCoreException(ErrorCodes.USER_CURRENT_PASSWORD_INVALID_CODE);

            IdentityHelpers.GetPasswordHashAndSalt(dto.NewPassword, out var hash, out var salt);
            user.PasswordHash = hash;
            user.PasswordSalt = salt;
            await _dbContext.UpdateAsync(user);
        }

        public async Task ForgotPasswordAsync(ForgotPasswordDto dto)
        {
            // Always succeed from the caller's perspective (no email enumeration).
            var email = dto.Email?.Trim();
            if (string.IsNullOrWhiteSpace(email))
                return;

            var user = (await _dbContext.GetAsync<User>(x => x.Email == email))?.FirstOrDefault();
            if (user is null || !user.IsActive)
                return;

            var rawToken = IdentityHelpers.CreateRawResetToken();
            var tokenHash = IdentityHelpers.HashResetToken(rawToken);
            var hours = _configuration.GetValue("Auth:ResetTokenHours", 2);
            var now = DateTime.UtcNow;

            var db = _dbContext._dbContext;
            var existing = await db.Set<PasswordResetToken>()
                .Where(t => t.UserId == user.Id && t.UsedAt == null)
                .ToListAsync();
            foreach (var t in existing)
                t.UsedAt = now;

            db.Set<PasswordResetToken>().Add(new PasswordResetToken
            {
                UserId = user.Id,
                TokenHash = tokenHash,
                CreatedAt = now,
                ExpiresAt = now.AddHours(hours)
            });
            await db.SaveChangesAsync();

            var clientBase = (_configuration["Auth:ClientBaseUrl"] ?? "https://localhost:5001").TrimEnd('/');
            var resetLink = $"{clientBase}/users/reset-password?token={Uri.EscapeDataString(rawToken)}";
            _logger.LogWarning(
                "Password reset link for {Email} (expires in {Hours}h): {ResetLink}",
                user.Email,
                hours,
                resetLink);
        }

        public async Task ResetPasswordAsync(ResetPasswordDto dto)
        {
            if (dto.NewPassword != dto.ConfirmPassword)
                throw new DeliveryCoreException(ErrorCodes.USER_PASSWORD_NOT_MATCHED_CODE);

            if (string.IsNullOrWhiteSpace(dto.Token))
                throw new DeliveryCoreException(ErrorCodes.USER_RESET_TOKEN_INVALID_CODE);

            var tokenHash = IdentityHelpers.HashResetToken(dto.Token.Trim());
            var now = DateTime.UtcNow;
            var db = _dbContext._dbContext;

            var reset = await db.Set<PasswordResetToken>()
                .Include(t => t.User)
                .FirstOrDefaultAsync(t => t.TokenHash == tokenHash);

            if (reset is null || reset.UsedAt != null || reset.ExpiresAt < now || reset.User is null || !reset.User.IsActive)
                throw new DeliveryCoreException(ErrorCodes.USER_RESET_TOKEN_INVALID_CODE);

            IdentityHelpers.GetPasswordHashAndSalt(dto.NewPassword, out var hash, out var salt);
            reset.User.PasswordHash = hash;
            reset.User.PasswordSalt = salt;
            reset.UsedAt = now;

            var siblings = await db.Set<PasswordResetToken>()
                .Where(t => t.UserId == reset.UserId && t.Id != reset.Id && t.UsedAt == null)
                .ToListAsync();
            foreach (var t in siblings)
                t.UsedAt = now;

            await db.SaveChangesAsync();
        }

        public async Task<JwtTokenModel> Authenticate(UserLoginDto userLoginDto)
        {
            User user = string.IsNullOrEmpty(userLoginDto.Email)
                ? null
                : (await _dbContext.GetAsync<User>(x => x.Email == userLoginDto.Email))?.FirstOrDefault();

            if (user is null || !IdentityHelpers.ValidatePassword(userLoginDto.Password, user.PasswordHash, user.PasswordSalt))
            {
                throw new DeliveryCoreException(ErrorCodes.USER_NOT_FOUND_CODE);
            }

            if (!user.IsActive)
            {
                throw new DeliveryCoreException(ErrorCodes.USER_ACCOUNT_INACTIVE_CODE);
            }

            // Upgrade legacy HMAC hashes to PBKDF2 on successful login
            if (!user.PasswordHash.StartsWith("PBKDF2$", StringComparison.Ordinal))
            {
                IdentityHelpers.GetPasswordHashAndSalt(userLoginDto.Password, out var hash, out var salt);
                user.PasswordHash = hash;
                user.PasswordSalt = salt;
                await _dbContext.UpdateAsync(user);
            }

            return GenerateToken(user);
        }

        public JwtTokenModel GenerateToken(User user)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role),
                new Claim("UserId", user.Id.ToString()),
            };

            // Admin gets every Staff permission claim; Staff gets assigned set.
            var perms = string.Equals(user.Role, Roles.Admin, StringComparison.OrdinalIgnoreCase)
                ? StaffPermissions.All.ToList()
                : StaffPermissions.Parse(user.Permissions);

            if (perms.Count == 0 && string.Equals(user.Role, Roles.Staff, StringComparison.OrdinalIgnoreCase))
                perms = StaffPermissions.DefaultStaff.ToList();

            foreach (var perm in perms)
                claims.Add(new Claim(ClaimTypes.Role, perm));

            var currentDate = DateTime.Now;
            var expiresInSeconds = _configuration.GetValue<int>("Jwt:ExpiresIn");
            var expiresIn = currentDate.AddSeconds(expiresInSeconds);
            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: expiresIn,
                signingCredentials: credentials);

            return new JwtTokenModel
            {
                AccessToken = new JwtSecurityTokenHandler().WriteToken(token),
                TokenType = "Bearer",
                ExpiresIn = expiresInSeconds
            };
        }
    }
}
