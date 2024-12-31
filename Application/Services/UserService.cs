using Application.Common.Models;
using Application.DTO.AccountDtos;
using Application.DTO.UserDtos;
using Application.Interfaces;
using Domain.Constants;
using Domain.Entities;
using Domain.Exceptions;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services
{
    public class UserService : IUserService
    {
        public IApplicationDbContext _dbContext;
        public IConfiguration _configuration;
        public UserService(IApplicationDbContext dbContext, IConfiguration configuration)
        {
            _dbContext = dbContext;
            _configuration = configuration;
        }

        public async Task<User> Register(UserDTO userDTO)
        {
            var userInDb= (await _dbContext.GetAsync<User>(x => x.Email == userDTO.Email))?.FirstOrDefault();
            if (userInDb != null) 
            {
                throw new DeliveryCoreException(ErrorCodes.USER_ALREADY_EXISTS_CODE);
            }

            GetPasswordHashAndSalt(userDTO.Password, out string passwordHash, out string passwordSalt);
            User user =new User();
            user.PasswordHash = passwordHash;
            user.PasswordSalt = passwordSalt;
            user.Email = userDTO.Email;
            user.FullName = userDTO.FullName;
            user.Address = userDTO.Address;
            user.PhoneNumber1 = userDTO.PhoneNumber1;
            user.PhoneNumber2 = userDTO.PhoneNumber2;
            user.Role =Roles.Staff;
            

            return await _dbContext.AddAsync<User>(user);
        }
        public static void GetPasswordHashAndSalt(string password, out string hash, out string salt)
        {
            using HMAC hMAC = new HMACSHA256();
            salt = Convert.ToBase64String(hMAC.Key);
            hash = Convert.ToBase64String(hMAC.ComputeHash(Encoding.UTF8.GetBytes(password)));
        }
       
        public async Task<JwtTokenModel> Login(LoginDto loginDto)
        {
            var userInDb = (await _dbContext.GetAsync<User>(x => x.Email == loginDto.Email))?.FirstOrDefault();
            
            if (userInDb == null)
            {
                throw new DeliveryCoreException(ErrorCodes.INVALID_USERNAME_OR_EMAIL_ADDRESS_CODE);
            }

            bool isPasswordValid = ValidatePassword(loginDto.Password, userInDb.PasswordHash, userInDb.PasswordSalt);

            if (!isPasswordValid)
            {
               throw new DeliveryCoreException(ErrorCodes.INVALID_USERNAME_OR_EMAIL_ADDRESS_CODE);
            }

            // Generate and return JWT token if user is authenticated
            return await GenerateToken(userInDb);
        }
        public static bool ValidatePassword(string password, string hash, string salt)
        {
            using HMAC hMAC = new HMACSHA256(Convert.FromBase64String(salt));
            return Convert.ToBase64String(hMAC.ComputeHash(Encoding.UTF8.GetBytes(password))) == hash;
        }
        public async Task<JwtTokenModel> GenerateToken(User user)
        {
            //int? UserId = user.Id;

            //// Check if the user has an Admin role
            //if (user.Role == Roles.Admin)
            //{
            //    var adminUser = (await _dbContext.GetAsync<User>(f => f.Id == user.Id))?.FirstOrDefault();
            //    UserId = adminUser?.Id;
            //}

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            // Define JWT claims
            var claims = new[]
            {
                //new Claim("FullName", user.FullName ?? ""),
                //new Claim("Email", user.Email ?? ""),
                new Claim(ClaimTypes.Role, user.Role.ToString()),
                new Claim("UserId", user.Id.ToString())
            };

            DateTime currentDate = DateTime.Now;
            var expiresInSeconds = int.Parse(_configuration["Jwt:ExpiresIn"]);
            DateTime expiresIn = currentDate.AddSeconds(expiresInSeconds);

            // Generate JWT token
            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: expiresIn,
                signingCredentials: credentials
            );

            // Create and return JwtTokenModel
            JwtTokenModel tokenModel = new()
            {
                AccessToken = new JwtSecurityTokenHandler().WriteToken(token),
                TokenType = "Bearer",
                ExpiresIn = expiresInSeconds,
                IssueDate = currentDate
            };

            return tokenModel;
        }


        public async Task<bool> Delete(int id)
        {
            return await _dbContext.DeleteAsync<User>(id);
        }

        public async Task<IEnumerable<User>> GetAll()
        {
            return await _dbContext.GetAsync<User>();
        }

        public async Task<User> GetById(int id)
        {
            return (await _dbContext.GetAsync<User>(x=>x.Id == id))?.FirstOrDefault();
        }

        public async Task Update(User user)
        {
            await _dbContext.UpdateAsync<User>(user);
        }
    }
}
