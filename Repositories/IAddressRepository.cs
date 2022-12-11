using MongoDB.Driver;
using PinoyMassageService.Entities;

namespace PinoyMassageService.Repositories
{
    public interface IAddressRepository
    {
        Task<Address> GetAddressAsync(Guid id);
        Task<Address> GetAddressByUserIdAsync(Guid userId);
        Task<IEnumerable<Address>> GetAddressessAsync();
        Task CreateAddressAsync(Address address);
        Task UpdateAddressAsync(Address address);
        Task DeleteAddressAsync(Guid Id);
        Task DeleteAddressByUserIdAsync(Guid userId);
        Task<DeleteResult> DeleteAllAddressByUserIdAsync(Guid userId);
    }
}
