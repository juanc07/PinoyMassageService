﻿using PinoyMassageService.Entities;

namespace PinoyMassageService.Repositories
{
    public interface IUserRepository
    {
        Task<User> GetUserAsync(Guid id);        
        Task<User> GetUserByUserNameAsync(string username);
        Task<User> GetUserByEmailAsync(string email);
        Task<User> GetUserByMobileNumberAsync(string mobilenumber);
        Task<bool> CheckUserMobileNumberAsync(string mobilenumber);
        Task<string> GetUserMobileNumberByProviderAsync(string provider,string providerId);
        Task<IEnumerable<User>> GetUsersAsync();
        Task CreateUserAsync(User user);
        Task UpdateUserAsync(User user);
        Task<bool> UpdateUserGoogleUserIdAsync(string mobilenumber, string googleUserId);
        Task<bool> UpdateUserFacebookUserIdAsync(string mobilenumber, string facebookUserId);
        Task DeleteUserAsync(Guid id);        
    }
}
