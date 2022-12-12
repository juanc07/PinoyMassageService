using PinoyMassageService.Entities;

namespace PinoyMassageService.Repositories
{
    public interface IAccountRepository
    {
        Task<Account> GetAccountAsync(Guid id);
        Task<Account> GetAccountByUserIdAsync(Guid userId);                
        Task<Account> GetAccountByHandleNameAsync(string handlename);
        Task<IEnumerable<Account>> GetAccountsAsync();
        Task CreateAccountAsync(Account account);
        Task UpdateAccountAsync(Account account);
        Task DeleteAccountAsync(Guid id);
        Task DeleteAccountByUserIdAsync(Guid userId);
    }
}
