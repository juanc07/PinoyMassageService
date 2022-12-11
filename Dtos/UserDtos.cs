using System.ComponentModel.DataAnnotations;

namespace PinoyMassageService.Dtos
{
    public class UserDtos
    {
        public record UserDto(Guid Id,  string UserName, string Password, DateTimeOffset CreatedDate);
        public record CreateUserDto([Required] string UserName, [Required] string Password,DateTimeOffset CreatedDate);
        public record UpdatePasswordDto(string Password);
    }
}
