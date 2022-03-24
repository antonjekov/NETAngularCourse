using API.Data;
using API.DTOs;
using API.Entities;
using API.Interfaces;
using AutoMapper;
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
        private readonly IMapper mapper;

        public AccountController(DataContext context, ITokenService tokenService, IMapper mapper)
        {
            this.context = context;
            this.tokenService = tokenService;
            this.mapper = mapper;
        }

        [HttpPost("register")]
        public async Task<ActionResult<UserDto>> Register(RegisterDto data)
        {
            if (await UserExists(data.Username))
            {
                return BadRequest("Username is already taken");
            }

            var user = this.mapper.Map<AppUser>(data);

            using var hmac = new HMACSHA512();

            user.UserName = data.Username.ToLower();
            user.PasswordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(data.Password));
            user.PasswordSalt = hmac.Key;
           
            await context.Users.AddAsync(user);
            await context.SaveChangesAsync();
            var userDto = new UserDto()
            {
                Username = user.UserName,
                Token = this.tokenService.CreateToken(user),
                PhotoUrl = user.Photos.FirstOrDefault(x => x.IsMain)?.Url,
                KnownAs = user.KnownAs,
                Gender = user.Gender,
            };
            return userDto;
        }

        [HttpPost("login")]
        public async Task<ActionResult<UserDto>> Login(LoginDto data)
        {
            var user = await this.context.Users
                .Include(x => x.Photos)
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
                Token = this.tokenService.CreateToken(user),
                PhotoUrl = user.Photos.FirstOrDefault(x => x.IsMain)?.Url,
                KnownAs = user.KnownAs,
                Gender= user.Gender,
            };
        }

        private async Task<bool> UserExists(string username)
        {
            return await this.context.Users.AnyAsync(x => x.UserName == username.ToLower());
        }
    }
}
