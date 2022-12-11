using System.ComponentModel.DataAnnotations;

namespace PinoyMassageService.Dtos
{
    public class UserDtos
    {
        public record UserDto(Guid Id,  string UserName, string Password, byte[] PasswordHash, byte[] PasswordSalt, string RefreshToken, DateTime? TokenCreated, DateTime? TokenExpires, DateTimeOffset CreatedDate);
        public record CreateUserDto([Required] string UserName, [Required] string Password);
        public record CreateAdminDto([Required] string UserName, [Required] string Password);
        public record LoginUserDto([Required] string UserName, [Required] string Password);
        public record UpdatePasswordDto([Required]  string Password);
        public record RefreshTokenDto([Required] string refreshToken);
    }
}
