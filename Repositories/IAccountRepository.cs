using PinoyMassageService.Entities;

namespace PinoyMassageService.Repositories
{
    public interface IAccountsRepository
    {
        Task<Account> GetAccountAsync(Guid id);
        Task<Account> GetAccountByUserNameAsync(string username);
        Task<Account> GetAccountByEmailAsync(string email);
        Task<Account> GetAccountByMobileNumberAsync(string mobilenumber);
        Task<Account> GetAccountByHandleNameAsync(string handlename);
        Task<IEnumerable<Account>> GetAccountsAsync();
        Task CreateAccountAsync(Account account);
        Task UpdateAccountAsync(Account account);
        Task DeleteAccountAsync(Guid id);
    }
}
