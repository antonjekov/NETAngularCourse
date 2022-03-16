using API.Data;
using API.DTOs;
using API.Entities;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace API.Controllers
{
    [Authorize]
    public class UsersController : BaseAPIController
    {
        private readonly IUserRepository userRepository;
        private readonly IMapper mapper;

        public UsersController(IUserRepository userRepository, IMapper mapper)
        {
           this.userRepository = userRepository;
            this.mapper = mapper;
        }

        // GET: api/<UsersController>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<MemberDto>>> GetAsync()
        {
            var users = await this.userRepository.GetMembersAsync();
            return Ok(users);
        }

        //// GET api/<UsersController>/5
        //[HttpGet("{id}")]
        //public async Task<ActionResult<AppUser>> GetAsync(int id)
        //{
        //    return await this.userRepository.GetUsersByIdAsync(id);
        //}

        // GET api/<UsersController>/lisa
        [HttpGet("{username}")]
        public async Task<ActionResult<MemberDto>> GetAsync(string username)
        {
            return await this.userRepository.GetMemberAsync(username);
        }

        // POST api/<UsersController>
        [HttpPost]
        public void Post([FromBody] string value)
        {
        }

        // PUT api/<UsersController>
        [HttpPut]
        public async Task<ActionResult> UpdateUser(MemberUpdateDto memberUpdateDto)
        {
            var username = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var user = await this.userRepository.GetUsersByUsernameAsync(username);
            mapper.Map(memberUpdateDto, user);
            this.userRepository.Update(user);
            if (await this.userRepository.SaveAllAsync()) return NoContent();
            return BadRequest("Failed to update user");
        }

        // DELETE api/<UsersController>/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
