using PinoyMassageService.Entities;
using System.ComponentModel.DataAnnotations;

namespace PinoyMassageService.Dtos
{
    public class AccountDtos
    {
        public record AccountDto(Guid Id, Guid UserId,string Email, int AccountType, int Gender,
            string FirstName, string LastName, string HandleName, DateTimeOffset BirthDate, int Age, DateTimeOffset CreatedDate,
            string MobileNumber,string FacebookId,string IdentificationType, string IdentificationNumber, bool IsVerified);
        public record CreateAccountDto([Required] Guid UserId, string Email, string MobileNumber,
            string HandleName,int AccountType,int Gender, DateTimeOffset BirthDate, DateTimeOffset CreatedDate);

        // Basic information        
        public record UpdateBasicDto( string FirstName, string LastName, string HandleName);        
        // contact info
        public record UpdateMobileNumberDto(string MobileNumber);
        public record UpdateEmailDto(string Email);

        // identification info
        public record UpdateIdentificationDto(string IdentificationType, string IdentificationNumber);        

        // all mutable account info
        public record UpdateAccountDto(string FirstName, string LastName, string HandleName,
            string MobileNumber, string IdentificationType, string IdentificationNumber);
    }
}
