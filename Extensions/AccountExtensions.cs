using PinoyMassageService.Entities;
using static PinoyMassageService.Dtos.AccountDtos;

namespace PinoyMassageService.Extensions
{
    public static class AccountExtensions
    {
        public static AccountDto AsDto(this Account account)
        {
            return new AccountDto(account.Id, account.UserId,account.FirstName, account.LastName, account.HandleName, account.Gender,
                account.BirthDate, account.Age,account.IdentificationType, account.IdentificationNumber, account.IsVerified, account.CreatedDate);
        }
    }
}