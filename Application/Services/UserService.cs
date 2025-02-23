using Application.Common.Models;
using Application.DTO.AccountDtos;
using Application.DTO.UserDtos;
using Application.Helpers;
using Application.Interfaces;
using Domain.Common.Models;
using Domain.Constants;
using Domain.Entities;
using Domain.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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

        public async Task<IEnumerable<User>> GetAll()
        {
            return await _dbContext.GetAsync<User>();
        }
        public async Task<PagedList<User>> SearchUsersAsync(string searchTerm, int page, int pageSize)
        {
            // Normalize search term
            searchTerm = searchTerm?.Trim()?.ToLower();
            if (string.IsNullOrEmpty(searchTerm))
            {
                return await _dbContext.GetPagedAsync<User>(page, pageSize);
            }

            // Use predicate for search
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

            GetPasswordHashAndSalt(createUserDTO.Password, out string passwordHash, out string passwordSalt);
            User user = new User();
            user.Role= Roles.Staff;
            user.PasswordHash = passwordHash;
            user.PasswordSalt = passwordSalt;
            user.Email = createUserDTO.Email;
            user.FullName = createUserDTO.FullName;
            user.Address = createUserDTO.Address;
            user.PhoneNumber1 = createUserDTO.PhoneNumber1;
            user.PhoneNumber2 = createUserDTO.PhoneNumber2;
            user.Role = Roles.Staff;


            return await _dbContext.AddAsync<User>(user);
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
            var driverInDb = (await _dbContext.GetAsync<User>(x => x.Id == updateUserDTO.Id))?.FirstOrDefault();
            if (driverInDb != null)
            {
                driverInDb.FullName = updateUserDTO.FullName;
                driverInDb.Address = updateUserDTO.Address;
                driverInDb.PhoneNumber1 = updateUserDTO.PhoneNumber1;
                driverInDb.PhoneNumber2 = updateUserDTO.PhoneNumber2;
                await _dbContext.UpdateAsync<User>(driverInDb);
            }
            else
            {
                throw new DeliveryCoreException(ErrorCodes.USER_ALREADY_EXISTS_CODE);
            }

        }

        public static void GetPasswordHashAndSalt(string password, out string hash, out string salt)
        {
            using HMAC hMAC = new HMACSHA256();
            salt = Convert.ToBase64String(hMAC.Key);
            hash = Convert.ToBase64String(hMAC.ComputeHash(Encoding.UTF8.GetBytes(password)));
        }


        public async Task<JwtTokenModel> Authenticate(UserLoginDto userLoginDto)
        {
            User user = string.IsNullOrEmpty(userLoginDto.Email) ? null
                              : (await _dbContext.GetAsync<User>(x => x.Email == userLoginDto.Email))?.FirstOrDefault();




            if (user is null || !IdentityHelpers.ValidatePassword(userLoginDto.Password, user.PasswordHash, user.PasswordSalt))
            {

                throw new DeliveryCoreException(ErrorCodes.USER_NOT_FOUND_CODE);
            }
            var jwtSecurityToken = GenerateToken(user);

            return jwtSecurityToken;
        }

        public JwtTokenModel GenerateToken(User user)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
            var cerdentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
				//new Claim(ClaimTypes.Name, $"{user.FirstName} {user.LastName}"),
                //new Claim("UserName", user.UserName),
                //new Claim("FirstName", user.FirstName),
				//new Claim("LastName", user.LastName),
				new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role),
                new Claim("UserId", user.Id.ToString()),

            };
            DateTime currentDate = DateTime.Now;
            var expiresInSeconds = _configuration.GetValue<int>("Jwt:ExpiresIn");
            DateTime expiresIn = currentDate.AddSeconds(expiresInSeconds);
            var token = new JwtSecurityToken(issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: expiresIn,
                signingCredentials: cerdentials);

            JwtTokenModel tokenModel = new JwtTokenModel()
            {
                AccessToken = new JwtSecurityTokenHandler().WriteToken(token),
                TokenType = "Bearer",
                ExpiresIn = expiresInSeconds
            };
            return tokenModel;
        }





    }
}
