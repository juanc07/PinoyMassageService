using System.ComponentModel.DataAnnotations;

namespace PinoyMassageService.Dtos
{
    public class UserDtos
    {
        public record UserDto(Guid Id,  string UserName, string Password, byte[] PasswordHash, byte[] PasswordSalt, int AccountType,
            string Email, string MobileNumber,string DisplayName, string FacebookId,string GoogleId,string FirebaseId, DateTimeOffset CreatedDate);
        public record UserExternalDto(Guid Id, string UserName,int AccountType, string Email, string MobileNumber, string DisplayName, 
            string FacebookId, string GoogleId, string FirebaseId, DateTimeOffset CreatedDate);

        public record CreateUserDto([Required] string Email, [Required] string Password, int AccountType, string MobileNumber);
        public record CreateUserExternalDto([Required] string MobileNumber, [Required] string Email, [Required] string DisplayName, [Required] string FirebaseId, string FacebookId, string GoogleId, int AccountType,
            string FirstName, string LastName, int Gender, long BirthDate);
        public record CreateAdminDto([Required] string UserName, [Required] string Password);
        public record LoginUserDto([Required] string UserName, [Required] string Password);
        public record LoginUserExternalDto([Required] string UserName, [Required] string accessTokenFromExternal);
        public record UpdatePasswordDto([Required]  string Password);        

        // contact info
        public record UpdateMobileNumberDto(string MobileNumber);
        public record UpdateEmailDto(string Email);
    }
}
