using System.ComponentModel.DataAnnotations;

namespace PinoyMassageService.Dtos
{
    public class AccountDtos
    {
        public record AccountDto(Guid Id, string UserName, string Password, string Email, int AccountType, int Gender,
            string FirstName, string LastName, string HandleName, DateTimeOffset BirthDate, int Age, DateTimeOffset CreatedDate,
            string MobileNumber,string FacebookId,string IdentificationType, string IdentificationNumber, bool IsVerified);
        public record CreateAccountDto([Required] string UserName, string Password, string Email, string HandleName,
           string MobileNumber, int AccountType,int Gender, DateTimeOffset BirthDate, DateTimeOffset CreatedDate);

        // Basic information        
        public record UpdateBasicDto( string FirstName, string LastName, string HandleName);
        // security info
        public record UpdatePasswordDto(string Password);
        // contact info
        public record UpdateMobileNumberDto(string MobileNumber);

        // identification info
        public record UpdateIdentificationDto(string IdentificationType, string IdentificationNumber);

        // all mutable account info
        public record UpdateAccountDto(string FirstName, string LastName, string HandleName,
            string Password, string MobileNumber, string IdentificationType, string IdentificationNumber);
    }
}
