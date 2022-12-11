using PinoyMassageService.Entities;
using static PinoyMassageService.Dtos.ProfileImageDtos;

namespace PinoyMassageService.Extensions
{
    public static class ProfileImageExtensions
    {
        public static ProfileImageDto AsDto(this ProfileImage profileImage)
        {
            return new ProfileImageDto(profileImage.Id, profileImage.userId, profileImage.Image,
                profileImage.Description, profileImage.CreatedDate);
        }
    }
}
