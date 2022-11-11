using PinoyMassageService.Entities;
using static PinoyMassageService.Dtos.AccountDtos;

namespace PinoyMassageService.Extensions
{
    public static class AccountExtensions
    {
        public static AccountDto AsDto(this Account account)
        {          
            return new AccountDto(account.Id, account.UserName, account.Password, account.Email, account.AccountType, account.ProfileName,
                account.BirthDate, account.Age, account.MobileNumber, account.IsVerified, account.FacebookId, account.CreatedDate);
        }
    }
}
