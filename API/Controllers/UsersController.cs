using API.Data;
using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Helpers;
using API.Interfaces;
using AutoMapper;
using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
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
        private readonly IPhotoService photoService;

        public UsersController(
            IUserRepository userRepository, 
            IMapper mapper, 
            IPhotoService photoService)
        {
           this.userRepository = userRepository;
           this.mapper = mapper;
           this.photoService = photoService;
        }

        // GET: api/<UsersController>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<MemberDto>>> GetAsync([FromQuery]UserParams userParams)
        {
            var user = await userRepository.GetMemberAsync(User.GetUsername());
            userParams.CurrentUsername = user.Username;
            if (string.IsNullOrEmpty(userParams.Gender))
            {
                userParams.Gender = user.Gender == "male"?"female":"male";
            }
            var users = await this.userRepository.GetMembersAsync(userParams);
            Response.AddPaginationHeader(users.CurrentPage, users.PageSize, users.TotalCount, users.TotalPages);
            return Ok(users);
        }

        // GET api/<UsersController>/lisa
        [HttpGet("{username}", Name ="GetUser")]
        public async Task<ActionResult<MemberDto>> GetAsync(string username)
        {
            return await this.userRepository.GetMemberAsync(username);
        }

        // POST api/<UsersController>add-photo
        [HttpPost("add-photo")]
        public async Task<ActionResult<PhotoDto>> AddPhoto(IFormFile file)
        {
            var user = await this.userRepository.GetUsersByUsernameAsync(User.GetUsername());
            var result =  await this.photoService.AddPhotoAsync(file);
            if (result.Error != null) return BadRequest(result.Error.Message);
            var photo = new Photo
            {
                Url = result.SecureUrl.AbsoluteUri,
                PublicId = result.PublicId,
            };
            if(user.Photos.Count==0) photo.IsMain = true;
            user.Photos.Add(photo);
            if(await this.userRepository.SaveAllAsync())
            {
                //return this.mapper.Map<PhotoDto>(photo);
                return CreatedAtRoute("GetUser", new { username = user.UserName}, this.mapper.Map<PhotoDto>(photo));
            }
            return BadRequest("Problem adding photo");
        }

        // PUT api/<UsersController>
        [HttpPut]
        public async Task<ActionResult> UpdateUser(MemberUpdateDto memberUpdateDto)
        {
            var user = await this.userRepository.GetUsersByUsernameAsync(User.GetUsername());
            mapper.Map(memberUpdateDto, user);
            this.userRepository.Update(user);
            if (await this.userRepository.SaveAllAsync()) return NoContent();
            return BadRequest("Failed to update user");
        }

        [HttpPut("set-main-photo/{photoId}")]
        public async Task<ActionResult> SetMainPhoto([FromRoute]int photoId)
        {
            var user = await userRepository.GetUsersByUsernameAsync(User.GetUsername());
            var photo = user.Photos.FirstOrDefault(x => x.Id == photoId);
            if (photo == null) return BadRequest("The photo was not found");
            if (photo.IsMain) return BadRequest("This is already your main photo");
            var currentMainPhoto = user.Photos.FirstOrDefault(x=>x.IsMain);
            if(currentMainPhoto!=null) currentMainPhoto.IsMain = false;
            photo.IsMain = true;
            if(await this.userRepository.SaveAllAsync()) return NoContent();
            return BadRequest("Failed to set main photo");
        }

        // DELETE api/<UsersController>/5
        [HttpDelete("delete-photo/{photoId}")]
        public async Task<ActionResult> Delete([FromRoute]int photoId)
        {
            var user = await userRepository.GetUsersByUsernameAsync(User.GetUsername());
            var photo = user.Photos.FirstOrDefault(x => x.Id == photoId);
            if (photo == null) return NotFound();
            if (photo.IsMain) return BadRequest("You can not delete your main photo");
            if (photo.PublicId!=null)
            {
                var result = await this.photoService.DeletePhotoAsync(photo.PublicId);
                if (result.Error != null) return BadRequest(result.Error.Message);
                else {
                    user.Photos.Remove(photo);
                    return NoContent();
                } 
            }
            return BadRequest("Photo is not deleted");
        }
    }
}
