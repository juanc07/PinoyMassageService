using PinoyMassageService.Entities;

namespace PinoyMassageService.Repositories
{
    public interface IAccountsRepository
    {
        Task<Account> GetAccountAsync(Guid id);
        Task<IEnumerable<Account>> GetAccountsAsync();
        Task CreateAccountAsync(Account account);
        Task UpdateAccountAsync(Account account);
        Task DeleteAccountAsync(Guid id);
    }
}
