using PinoyMassageService.Entities;

namespace PinoyMassageService.Repositories
{
    public interface IContactRepository
    {
        Task<Contact> GetContactAsync(Guid id);        
        Task<Contact> GetContactByUserNameAsync(string username);
        Task<Contact> GetContactByEmailAsync(string email);
        Task<Contact> GetContactByMobileNumberAsync(string mobilenumber);
        Task<bool> CheckContactMobileNumberAsync(string mobilenumber);
        Task<string> GetContactMobileNumberByProviderAsync(string provider,string providerId);
        Task<IEnumerable<Contact>> GetContactsAsync();
        Task<IEnumerable<Contact>> GetContactsByUserNameAsync(string userName);
        Task CreateContactAsync(Contact user);
        Task<bool>  UpdateContactAsync(Contact user);
        Task<bool> UpdateContactProviderIdAsync(string mobilenumber, string provider, string providerId);
        Task DeleteContactAsync(Guid id);        
    }
}
