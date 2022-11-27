using Microsoft.AspNetCore.Mvc;
using PinoyMassageService.Entities;
using PinoyMassageService.Extensions;
using PinoyMassageService.Repositories;
using static PinoyMassageService.Dtos.AddressDtos;
using static PinoyMassageService.Dtos.ProfileImageDtos;

namespace PinoyMassageService.Controllers
{
    [ApiController]
    [Route("[controller]/[action]")]
    public class ProfileImageController : ControllerBase
    {
        private readonly IProfileImageRepository repository;
        private readonly ILogger<ProfileImageController> logger;

        public ProfileImageController(IProfileImageRepository repository, ILogger<ProfileImageController> logger)
        {
            this.repository = repository;
            this.logger = logger;
        }

        [HttpPost]
        public async Task<ActionResult<ProfileImageDto>> CreateProfileImageAsync(CreateProfileImageDto profileImageDto)
        {
            // we only allow one profile image for now reason very low budget for this project
            var foundProfileImage = await repository.GetProfileImageByAccountIdAsync(profileImageDto.AccountId);
            if (foundProfileImage is null)
            {
                ProfileImage profileImage = new()
                {
                    Id = Guid.NewGuid(),
                    AccountId = profileImageDto.AccountId,
                    Image = profileImageDto.Image,                    
                    Description = profileImageDto.Description,
                    CreatedDate = DateTimeOffset.UtcNow
                };

                await repository.CreateProfileImageAsync(profileImage);
                return CreatedAtAction(nameof(GetProfileImageAsync), new { id = profileImage.Id }, profileImage.AsDto());
            }
            return BadRequest("ProfileImage already exists!");
        }

        // GET /ProfileImage/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<ProfileImageDto>> GetProfileImageAsync(Guid id)
        {
            var profileImage = await repository.GetProfileImageAsync(id);
            if (profileImage is null)
            {
                return NotFound();
            }
            return profileImage.AsDto();
        }

        // GET /ProfileImage/{id}
        [HttpGet("{accountId}")]
        public async Task<ActionResult<ProfileImageDto>> GetProfileImageByAccountIdAsync(Guid accountId)
        {
            var profileImage = await repository.GetProfileImageByAccountIdAsync(accountId);
            if (profileImage is null)
            {
                return NotFound();
            }
            return profileImage.AsDto();
        }

        // /ProfileImage/GetProfileImagesAsync
        [HttpGet]
        public async Task<IEnumerable<ProfileImageDto>> GetProfileImagesAsync()
        {
            var profileImages = (await repository.GetProfileImagesAsync()).Select(profileImage => profileImage.AsDto());

            logger.LogInformation($"{DateTime.UtcNow.ToString("hh:mm:ss")}: GetProfileImagesAsync Retrieved {profileImages.Count()} profileImages");
            return profileImages;
        }

        [HttpPut("{id}")]
        public async Task<ActionResult> UpdateProfileImageAsync(Guid id, UpdateProfileImageDto profileImageDto)
        {
            var existingProfileImage = await repository.GetProfileImageAsync(id);
            if (existingProfileImage is null)
            {
                return NotFound();
            }

            existingProfileImage.Image = profileImageDto.Image;
            existingProfileImage.Description = profileImageDto.Description;
            

            await repository.UpdateProfileImageAsync(existingProfileImage);
            return NoContent();
        }

        [HttpPut("{accountId}")]
        public async Task<ActionResult> UpdateProfileImageByAccountIdAsync(Guid accountId, UpdateProfileImageDto profileImageDto)
        {
            var existingProfileImage = await repository.GetProfileImageByAccountIdAsync(accountId);
            if (existingProfileImage is null)
            {
                return NotFound();
            }

            existingProfileImage.Image = profileImageDto.Image;
            existingProfileImage.Description = profileImageDto.Description;


            await repository.UpdateProfileImageAsync(existingProfileImage);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteProfileImageAsync(Guid id)
        {
            var existingProfileImage = await repository.GetProfileImageAsync(id);
            if (existingProfileImage is null)
            {
                return NotFound();
            }

            await repository.DeleteProfileImageAsync(id);
            return NoContent();
        }

        [HttpDelete("{accountId}")]
        public async Task<ActionResult> DeleteProfileImageByAccountIdAsync(Guid accountId)
        {
            var existingProfileImage = await repository.GetProfileImageByAccountIdAsync(accountId);
            if (existingProfileImage is null)
            {
                return NotFound();
            }

            await repository.DeleteProfileImageByAccountIdAsync(accountId);
            return NoContent();
        }

        [HttpDelete("{accountId}")]
        public async Task<ActionResult> DeleteAllProfileImageByAccountIdAsync(Guid accountId)
        {
            var deleteResult = await repository.DeleteAllProfileImageByAccountIdAsync(accountId);
            if (deleteResult.DeletedCount == 0)
            {
                return NotFound();
            }
            else
            {
                return Content($"Profile Image deleted count: {deleteResult.DeletedCount}");
            }
        }
    }
}
