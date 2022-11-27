using MongoDB.Driver;
using PinoyMassageService.Entities;

namespace PinoyMassageService.Repositories
{
    public interface IServiceHistoryRepository
    {
        Task<Service> GetServiceAsync(Guid id);
        Task<Service> GetServiceByProviderIdAsync(Guid providerId);
        Task<Service> GetServiceByClientIdAsync(Guid clientId);
        Task<IEnumerable<Service>> GetServicesAsync();
        Task CreateServiceAsync(Service service);
        // for changing status from active,pending, accepted, completed, canceled and expired
        Task UpdateServiceAsync(Service service);
        Task DeleteServiceAsync(Guid id);
        Task DeleteServiceByProviderIdAsync(Guid providerId);
        Task<DeleteResult> DeleteAllServiceByProviderIdAsync(Guid providerId);
    }
}
