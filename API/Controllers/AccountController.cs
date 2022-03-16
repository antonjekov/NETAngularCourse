using API.Data;
using API.DTOs;
using API.Entities;
using API.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace API.Controllers
{
    public class AccountController : BaseAPIController
    {
        private DataContext context;
        private readonly ITokenService tokenService;

        public AccountController(DataContext context, ITokenService tokenService)
        {
            this.context = context;
            this.tokenService = tokenService;
        }

        [HttpPost("register")]
        public async Task<ActionResult<UserDto>> Register(RegisterDto data)
        {
            if (await UserExists(data.Username))
            {
                return BadRequest("Username is already taken");
            }

            using var hmac = new HMACSHA512();
            var user = new AppUser()
            {
                UserName = data.Username.ToLower(),
                PasswordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(data.Password)),
                PasswordSalt = hmac.Key,
            };
            await context.Users.AddAsync(user);
            await context.SaveChangesAsync();
            return new UserDto() 
            { 
                Username = user.UserName, 
                Token = this.tokenService.CreateToken(user) 
            };
        }

        [HttpPost("login")]
        public async Task<ActionResult<UserDto>> Login(LoginDto data)
        {
            var user = await this.context.Users
                .FirstOrDefaultAsync(x => x.UserName == data.Username.ToLower());

            if (user == null) return Unauthorized("Invalid username or password");

            using var hmac = new HMACSHA512(user.PasswordSalt);
            var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(data.Password));
            for (int i = 0; i < hash.Length; i++)
            {
                if (hash[i] != user.PasswordHash[i]) return Unauthorized("Invalid username or password");
            }

            return new UserDto()
            {
                Username = user.UserName,
                Token = this.tokenService.CreateToken(user)
            };
        }

        private async Task<bool> UserExists(string username)
        {
            return await this.context.Users.AnyAsync(x => x.UserName == username.ToLower());
        }
    }
}
