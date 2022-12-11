using PinoyMassageService.Entities;

namespace PinoyMassageService.Repositories
{
    public interface IUserRepository
    {
        Task<User> GetUserAsync(Guid id);        
        Task<User> GetUserByUserNameAsync(string username);        
        Task<IEnumerable<User>> GetUsersAsync();
        Task CreateUserAsync(User user);
        Task UpdateUserAsync(User user);
        Task DeleteUserAsync(Guid id);        
    }
}
