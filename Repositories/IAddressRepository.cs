using MongoDB.Driver;
using PinoyMassageService.Entities;

namespace PinoyMassageService.Repositories
{
    public interface IAddressRepository
    {
        Task<Address> GetAddressAsync(Guid id);
        Task<Address> GetAddressByAccountIdAsync(Guid accountId);
        Task<IEnumerable<Address>> GetAddressessAsync();
        Task CreateAddressAsync(Address address);
        Task UpdateAddressAsync(Address address);
        Task DeleteAddressAsync(Guid Id);
        Task DeleteAddressByAccountIdAsync(Guid accountId);
        Task<DeleteResult> DeleteAllAddressByAccountIdAsync(Guid accountId);
    }
}
