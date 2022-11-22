using System.ComponentModel.DataAnnotations;

namespace PinoyMassageService.Dtos
{
    public class ProfileImageDtos
    {
        public record ProfileImageDto(Guid Id, Guid AccountId, byte[] Image,string Description, DateTimeOffset CreatedDate);

        public record CreateProfileImageDto([Required] Guid AccountId,byte[] Image, string Description, DateTimeOffset CreatedDate);

        public record UpdateProfileImageDto(byte[] Image, string Description);
    }
}
