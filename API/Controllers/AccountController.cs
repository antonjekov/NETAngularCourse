using API.Data;
using API.DTOs;
using API.Entities;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Identity;
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
        private readonly UserManager<AppUser> userManager;
        private readonly SignInManager<AppUser> signInManager;
        private readonly ITokenService tokenService;
        private readonly IMapper mapper;

        public AccountController(
            UserManager<AppUser> userManager,
            SignInManager<AppUser> signInManager, 
            ITokenService tokenService, 
            IMapper mapper)
        {
            this.userManager = userManager;
            this.signInManager = signInManager;
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

            user.UserName = data.Username.ToLower();

            var result = await this.userManager.CreateAsync(user, data.Password);
            if (!result.Succeeded) return this.BadRequest(result.Errors);
            var roleResult = await this.userManager.AddToRoleAsync(user, "Member");
            if (!roleResult.Succeeded) return this.BadRequest(result.Errors);

            var userDto = new UserDto()
            {
                Username = user.UserName,
                Token = await this.tokenService.CreateTokenAsync(user),
                PhotoUrl = user.Photos.FirstOrDefault(x => x.IsMain)?.Url,
                KnownAs = user.KnownAs,
                Gender = user.Gender,
            };
            return userDto;
        }

        [HttpPost("login")]
        public async Task<ActionResult<UserDto>> Login(LoginDto data)
        {
            var user = await this.userManager.Users
                .Include(x => x.Photos)
                .FirstOrDefaultAsync(x => x.UserName == data.Username.ToLower());

            if (user == null) return Unauthorized("Invalid username or password");

            var result = await this.signInManager.CheckPasswordSignInAsync(user, data.Password, false);

            if (!result.Succeeded) return this.Unauthorized();

            return new UserDto()
            {
                Username = user.UserName,
                Token = await this.tokenService.CreateTokenAsync(user),
                PhotoUrl = user.Photos.FirstOrDefault(x => x.IsMain)?.Url,
                KnownAs = user.KnownAs,
                Gender= user.Gender,
            };
        }

        private async Task<bool> UserExists(string username)
        {
            return await this.userManager.Users.AnyAsync(x => x.UserName == username.ToLower());
        }
    }
}
