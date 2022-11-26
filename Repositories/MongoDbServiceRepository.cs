using MongoDB.Bson;
using MongoDB.Driver;
using PinoyMassageService.Entities;

namespace PinoyMassageService.Repositories
{
    public class MongoDbServiceRepository : IServiceRepository
    {

        public const string databaseName = "pinoymassage";
        public const string collectionName = "service";
        private readonly IMongoCollection<Service> serviceCollection;
        private readonly FilterDefinitionBuilder<Service> filterBuilder = Builders<Service>.Filter;

        public MongoDbServiceRepository(IMongoClient mongoclient)
        {
            IMongoDatabase database = mongoclient.GetDatabase(databaseName);
            serviceCollection = database.GetCollection<Service>(collectionName);
        }        

        public async Task CreateServiceAsync(Service service)
        {
            await serviceCollection.InsertOneAsync(service);
        }        

        public async Task<Service> GetServiceAsync(Guid id)
        {
            var filter = filterBuilder.Eq(service => service.Id, id);
            return await serviceCollection.Find(filter).FirstOrDefaultAsync();
        }

        public async Task<Service> GetServiceByClientIdAsync(Guid clientId)
        {
            var filter = filterBuilder.Eq(service => service.ClientId, clientId);
            return await serviceCollection.Find(filter).FirstOrDefaultAsync();
        }

        public async Task<Service> GetServiceByProviderIdAsync(Guid providerId)
        {
            var filter = filterBuilder.Eq(service => service.ProviderId, providerId);
            return await serviceCollection.Find(filter).FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<Service>> GetServicesAsync()
        {
            return await serviceCollection.Find(new BsonDocument()).ToListAsync();
        }

        public async Task UpdateServiceAsync(Service service)
        {
            var filter = filterBuilder.Eq(existingService => existingService.ProviderId, service.ProviderId);
            await serviceCollection.ReplaceOneAsync(filter, service);
        }                

        public async Task DeleteServiceAsync(Guid id)
        {
            var filter = filterBuilder.Eq(existingService => existingService.Id, id);
            await serviceCollection.DeleteOneAsync(filter);
        }

        public async Task DeleteServiceByProviderIdAsync(Guid providerId)
        {
            var filter = filterBuilder.Eq(existingService => existingService.Id, providerId);
            await serviceCollection.DeleteOneAsync(filter);
        }        

        public async Task<DeleteResult> DeleteAllServiceByProviderIdAsync(Guid providerId)
        {
            var filter = filterBuilder.Eq(existingService => existingService.ProviderId, providerId);
            return await serviceCollection.DeleteManyAsync(filter);
        }
    }
}
