using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApplication1.Data;
using WebApplication1.Dtos;
using WebApplication1.Helpers;
using WebApplication1.Models;

namespace WebApplication1.Controllers
{
    [ServiceFilter(typeof(LogUserActivity))]
    //[Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly IDatingRepository _repo;
        private readonly IMapper _mapper;

        public UsersController(IDatingRepository repo, IMapper mapper)
        {
            _repo = repo;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<IActionResult> GetUsers([FromQuery] UserParams userParams)  //UserParams Coming From Query String
        {
            var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);  // For Sorting
            var userFromRepo = await _repo.GetUser(currentUserId);

            userParams.UserId = currentUserId;  //Current UserId feed into userParams with parameter

            if (string.IsNullOrEmpty(userParams.Gender))    //For Sorting
            {
                userParams.Gender = userFromRepo.Gender == "male" ? "female" : "male";   //Store Gender Value in userParams
            }

            var users = await _repo.GetUsers(userParams);   //Return above sorted params Array of Objects and Return base on Params
            var userToReturn = _mapper.Map<IEnumerable<UserForListDto>>(users);   //User To Dto

            // for Header to the Client
            Response.AddPagination(users.CurrentPage, users.PageSize, users.TotalCount, users.TotalPage); // AddPagination frm Extensions

            return Ok(userToReturn);  //Return list of users
        }



        [AllowAnonymous]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetUser(int id)
        {
            var user = await _repo.GetUser(id);
            var userToReturn = _mapper.Map<UserForDetailDto>(user);
            return Ok(userToReturn);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(int id, UserForUpdateDto userForUpdateDto)
        {
            if (id != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
                return Unauthorized();

            var userFromRepo = await _repo.GetUser(id);

            _mapper.Map(userForUpdateDto, userFromRepo);
            if (await _repo.SaveAll())
                return NoContent();

            throw new Exception($"Updating user {id} failed on Server"); // For frontend
        }

        [HttpPost("{id}/like/{recipientId}")]
        public async Task<IActionResult> LikeUser(int id, int recipientId)
        {
            if (id != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
                return Unauthorized();
            var like = await _repo.GetLike(id, recipientId);

            if (like != null)
                return BadRequest("you are alredy liked this user");
            if (await _repo.GetUser(recipientId) == null)
                return NotFound();
            like = new Like
            {   LikerId = id,
                LikeeId = recipientId
            };


            _repo.Add<Like>(like);
            if(await _repo.SaveAll())
            {
                return Ok();
            }

            return BadRequest("Failed to like user");

        }
    }
}
