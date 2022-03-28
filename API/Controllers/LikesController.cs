using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Helpers;
using API.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace API.Controllers
{
    [Authorize]
    public class LikesController: BaseAPIController
    {
        private readonly ILikesRepository likesRepository;
        private readonly IUserRepository userRepository;

        public LikesController(ILikesRepository likesRepository, IUserRepository userRepository)
        {
            this.likesRepository = likesRepository;
            this.userRepository = userRepository;
        }

        [HttpPost("{username}")]
        public async Task<IActionResult> AddLike(string username)
        {
            var likedUser = await this.userRepository.GetUsersByUsernameAsync(username);
            var likedUserId = likedUser.Id;
            var sourceUserId = User.GetUserId();
            var sourceUser = await this.userRepository.GetUsersByIdAsync(sourceUserId);
            if (likedUser == null) return NotFound();
            if (sourceUser.UserName == username) return BadRequest("You cannot like yourself");
            var userLike = await this.likesRepository.GetUserLike(sourceUserId, likedUserId);
            if (userLike != null) return BadRequest("You already like this user");
            var like = new UserLike()
            {
                LikedUserId = likedUserId,
                SourceUserId = sourceUserId,
            };
            likedUser.LikedByUsers.Add(like);
            if (await this.userRepository.SaveAllAsync()) return this.Ok();
            return this.BadRequest("Failed to like user");
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<LikeDto>>> GetUserLikes([FromQuery]LikesParams likesParams)
        {
            likesParams.UserId = User.GetUserId();
            var likedUsers = await this.likesRepository.GetUserLikes(likesParams);
            Response.AddPaginationHeader(likedUsers.CurrentPage, likedUsers.PageSize, likedUsers.TotalCount, likedUsers.TotalPages);
            return Ok(likedUsers);
        }
    }
}
