using System.ComponentModel.DataAnnotations;

namespace PinoyMassageService.Dtos
{
    public class UserDtos
    {
        public record UserDto(Guid Id,  string UserName, string Password, byte[] PasswordHash, byte[] PasswordSalt, int AccountType,
            string Email, string MobileNumber,string FacebookId,string GoogleId,DateTimeOffset CreatedDate);
        public record CreateUserDto([Required] string Email, [Required] string Password, int AccountType, string MobileNumber);
        public record CreateUserExternalDto([Required] string Email,string FacebookId, string GoogleId, int AccountType,
            string FirstName, string LastName, int Gender, DateTime BirthDate);
        public record CreateAdminDto([Required] string UserName, [Required] string Password);
        public record LoginUserDto([Required] string UserName, [Required] string Password);
        public record LoginUserExternalDto([Required] string UserName, [Required] string accessTokenFromExternal);
        public record UpdatePasswordDto([Required]  string Password);        

        // contact info
        public record UpdateMobileNumberDto(string MobileNumber);
        public record UpdateEmailDto(string Email);
    }
}
