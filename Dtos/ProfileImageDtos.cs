using System.ComponentModel.DataAnnotations;

namespace PinoyMassageService.Dtos
{
    public class ProfileImageDtos
    {
        public record ProfileImageDto(Guid Id, Guid UserId, byte[] Image,string Description, DateTimeOffset CreatedDate);

        public record CreateProfileImageDto([Required] Guid UserId,byte[] Image, string Description, DateTimeOffset CreatedDate);

        public record UpdateProfileImageDto(byte[] Image, string Description);
    }
}
