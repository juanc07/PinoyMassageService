using System.ComponentModel.DataAnnotations;

namespace PinoyMassageService.Dtos
{
    public class AccountDtos
    {        
        public record AccountDto(Guid Id, string UserName, string Password, string Email, int AccountType,string ProfileName,
            DateTimeOffset BirthDate, int Age,string MobileNumber, bool IsVerified,string FacebookId, DateTimeOffset CreatedDate);
        public record CreateAccountDto([Required] string UserName, string Password, string Email, int AccountType, DateTimeOffset CreatedDate);
        public record UpdateAccountDto([Required] string UserName, string Password, string Email,string ProfileName, string MobileNumber, DateTimeOffset BirthDate);
    }
}
