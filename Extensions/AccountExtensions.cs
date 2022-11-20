using PinoyMassageService.Entities;
using static PinoyMassageService.Dtos.AccountDtos;

namespace PinoyMassageService.Extensions
{
    public static class AccountExtensions
    {
        public static AccountDto AsDto(this Account account)
        {
            return new AccountDto(account.Id, account.UserName, account.Password, account.Email, account.AccountType, account.Gender,
                account.FirstName, account.LastName, account.HandleName, account.BirthDate, account.Age, account.CreatedDate,
                account.MobileNumber, account.FacebookId, account.IdentificationType, account.IdentificationNumber, account.IsVerified);
        }
    }
}