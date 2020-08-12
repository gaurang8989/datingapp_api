using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Claims;
using System.Threading.Tasks;
using AutoMapper;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using WebApplication1.Data;
using WebApplication1.Dtos;
using WebApplication1.Helpers;
using WebApplication1.Models;

namespace WebApplication1.Controllers
{   //[Authorize]
    [Microsoft.AspNetCore.Mvc.Route("api/users/{userId}/photos")]
    [ApiController]
    public class PhotosController : ControllerBase
    {
        private readonly IDatingRepository _repo;
        private readonly IMapper _mapper;
        private readonly IOptions<CloudinarySettings> _cloudinaryConfig;
        private Cloudinary _cloudinary;

        public PhotosController(IDatingRepository repo, IMapper mapper,
            IOptions<CloudinarySettings> cloudinaryConfig)
        {
            _repo = repo;
            _mapper = mapper;
            _cloudinaryConfig = cloudinaryConfig;

            Account acc = new Account(    //This account detail value from appsettings.json
                _cloudinaryConfig.Value.CloudName,
                _cloudinaryConfig.Value.ApiKey,
                _cloudinaryConfig.Value.ApiSecret
                );

            _cloudinary = new Cloudinary(acc);  //Instance of Cloudinary in Constructor for passing acc detail in cloudinary
        }

        [HttpGet("{id}", Name = "GetPhoto")]
        public async Task<IActionResult> GetPhoto(int id)
        {
            var photoFromRepo =await _repo.GetPhoto(id);
            var photo = _mapper.Map<PhotoForReturnDto>(photoFromRepo);
            return Ok(photo);
        }

        [HttpPost]
        public async Task<IActionResult> AddPhotoForUser(int userId, [FromForm]PhotoForCreationDto photoForCreationDto)
        {
            if (userId != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))  //Check user Authorise Or not
            {
                 return Unauthorized();
            }
            var userFromRepo = await _repo.GetUser(userId);

            var file = photoForCreationDto.File; // Store image file from form in file variable
            var uploadResult = new ImageUploadResult(); //create new 

            if (file.Length > 0)
            {
                using (var stream = file.OpenReadStream())  // if > 0 then will read this file into Memory.
                {
                    var uploadParams = new ImageUploadParams()
                    {
                        File = new FileDescription(file.Name, stream),
                        Transformation = new Transformation()
                            .Width(500).Height(500).Crop("fill").Gravity("face")
                    };
                    uploadResult = _cloudinary.Upload(uploadParams);  //Here our photo will be upload on Cloudinary.
                }
            }

            photoForCreationDto.Url = uploadResult.Uri.ToString(); // This two line add Url & Public Id to our Database
            photoForCreationDto.PublicId = uploadResult.PublicId;
            var photo = _mapper.Map<Photo>(photoForCreationDto);

            if (!userFromRepo.Photos.Any(u => u.IsMain))
                photo.IsMain = true;
            userFromRepo.Photos.Add(photo);

            if(await _repo.SaveAll())
            {
                var photoToReturn = _mapper.Map<PhotoForReturnDto>(photo); //If Save is Successful then our photo have an ID which is Gnerated by SQLite it's Stored in our PhooToReturn.
                // We set return the created at root.
                //return CreatedAtRoute("GetPhoto", new { id = photo.Id}, photoToReturn );
                return Ok(photoToReturn);
            }

            return BadRequest("Could not add the photo");
        }

        [HttpPost("{id}/setMain")]
        public async Task<IActionResult> setMainPhoto(int userId,int id)
        {
            if (userId != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))  //Check user Authorise Or not
                return Unauthorized();
            var user = await _repo.GetUser(userId);

            if (!user.Photos.Any(p => p.Id == id))  //cHECK PHOTOS FOR iD IN cOLLECTION
                return Unauthorized();

            var photoFromRepo = await _repo.GetPhoto(id);
            if (photoFromRepo.IsMain)
                return BadRequest("This IS already Main Photo");

            var currentMainPhoto = await _repo.GetMainPhotoForUser(userId);
            currentMainPhoto.IsMain = false;

            photoFromRepo.IsMain = true;

            if (await _repo.SaveAll())
                return NoContent();

            return BadRequest("Could Not Set photo to Main");

        }

        [HttpDelete("{id}")]

        public async Task<IActionResult> DeletePhoto(int userId, int id)
        {
            if (userId != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))  //Check user Authorise Or not
                return Unauthorized();
            var user = await _repo.GetUser(userId);

            if (!user.Photos.Any(p => p.Id == id))  //cHECK PHOTOS FOR iD IN cOLLECTION
                return Unauthorized();

            var photoFromRepo = await _repo.GetPhoto(id);
            if (photoFromRepo.IsMain)
                return BadRequest("You can not Delete Youtr Main Photo");

            if(photoFromRepo.PublicId != null)
            {
                var deleteParams = new DeletionParams(photoFromRepo.PublicId);
                var result = _cloudinary.Destroy(deleteParams);  //result var return that Deleted ITEM.

                if (result.Result == "ok")
                {
                    _repo.Delete(photoFromRepo);
                }
            }

            if(photoFromRepo.PublicId == null)
            {
                _repo.Delete(photoFromRepo);   // This Condition is For NON-Cloudinary Photo
            }

            if(await _repo.SaveAll())
            {
                return Ok();
            }

            return BadRequest("Failed to Delete the Photo");
        }
    }
}
