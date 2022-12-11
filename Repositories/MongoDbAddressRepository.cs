using MongoDB.Bson;
using MongoDB.Driver;
using PinoyMassageService.Entities;

namespace PinoyMassageService.Repositories
{
    public class MongoDbAddressRepository : IAddressRepository
    {
        public const string databaseName = "pinoymassage";
        public const string collectionName = "address";
        private readonly IMongoCollection<Address> addressCollection;
        private readonly FilterDefinitionBuilder<Address> filterBuilder = Builders<Address>.Filter;

        public MongoDbAddressRepository(IMongoClient mongoclient)
        {
            IMongoDatabase database = mongoclient.GetDatabase(databaseName);
            addressCollection = database.GetCollection<Address>(collectionName);
        }

        public async Task CreateAddressAsync(Address address)
        {
            await addressCollection.InsertOneAsync(address);
        }        

        public async Task<Address> GetAddressAsync(Guid id)
        {
            var filter = filterBuilder.Eq(address => address.Id, id);
            return await addressCollection.Find(filter).FirstOrDefaultAsync();
        }

        public async Task<Address> GetAddressByUserIdAsync(Guid userId)
        {
            var filter = filterBuilder.Eq(address => address.UserId, userId);
            return await addressCollection.Find(filter).FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<Address>> GetAddressessAsync()
        {
            return await addressCollection.Find(new BsonDocument()).ToListAsync();
        }

        public async Task UpdateAddressAsync(Address address)
        {
            var filter = filterBuilder.Eq(existingAccount => existingAccount.UserId, address.UserId);
            await addressCollection.ReplaceOneAsync(filter, address);
        }

        public async Task DeleteAddressAsync(Guid id)
        {
            var filter = filterBuilder.Eq(address => address.Id, id);
            await addressCollection.DeleteOneAsync(filter);
        }

        public async Task DeleteAddressByUserIdAsync(Guid userId)
        {
            var filter = filterBuilder.Eq(address => address.UserId, userId);
            await addressCollection.DeleteOneAsync(filter);
        }

        public async Task<DeleteResult> DeleteAllAddressByUserIdAsync(Guid UserId)
        {
            var filter = filterBuilder.Eq(address => address.UserId, UserId);
            return await addressCollection.DeleteManyAsync(filter);
        }
    }
}
