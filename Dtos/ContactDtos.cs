using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;

namespace PinoyMassageService.Dtos
{
    public class ContactDtos
    {
        /*public record ContactDto(Guid Id,  string UserName, string Password, byte[] PasswordHash, byte[] PasswordSalt, int AccountType,
            string Email, string MobileNumber,string DisplayName, string FacebookId,string GoogleId,string FirebaseId, DateTimeOffset CreatedDate);*/

        public record ContactDto(Guid Id, string UserName, int AccountType,
            string Email, string MobileNumber, string DisplayName, string FacebookId, string GoogleId, string FirebaseId, DateTimeOffset CreatedDate);

        public record ContactExternalDto(Guid Id, string UserName,int AccountType, string Email, string MobileNumber, string DisplayName, 
            string FacebookId, string GoogleId, string FirebaseId, DateTimeOffset CreatedDate);
        public record CreateContactDto([Required] string Email, [Required] string Password, int AccountType, string MobileNumber, string DisplayName,
            string FirebaseId, string FacebookId, string GoogleId);        
        public record CreateContactExternalDto([Required] string MobileNumber, [Required] string Email, [Required] string DisplayName,
            string FirebaseId, string FacebookId, string GoogleId, int AccountType);
        public record CreateAdminDto([Required] string UserName, [Required] string Password);
        public record LoginContactDto([Required] string UserName, [Required] string Password);
        public record LoginContactExternalDto(string UserName, [Required] string IdTokenFromExternal,string Email);
        public record UpdatePasswordDto([Required]  string Password);        

        // contact info
        public record UpdateMobileNumberDto(string MobileNumber);
        public record UpdateEmailDto(string Email);
    }
}
