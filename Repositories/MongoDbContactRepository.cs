using MongoDB.Bson;
using MongoDB.Driver;
using PinoyMassageService.Constant;
using PinoyMassageService.Entities;

namespace PinoyMassageService.Repositories
{
    public class MongoDbContactRepository : IContactRepository
    {
        public const string databaseName = "pinoymassage";
        public const string collectionName = "contact";
        private readonly IMongoCollection<Contact> usersCollection;
        private readonly FilterDefinitionBuilder<Contact> filterBuilder = Builders<Contact>.Filter;

        public MongoDbContactRepository(IMongoClient mongoclient)
        {
            IMongoDatabase database = mongoclient.GetDatabase(databaseName);
            usersCollection = database.GetCollection<Contact>(collectionName);
        }        

        public async Task CreateContactAsync(Contact user)
        {
            await usersCollection.InsertOneAsync(user);
        }

        public async Task DeleteContactAsync(Guid id)
        {
            var filter = filterBuilder.Eq(user => user.Id, id);
            await usersCollection.DeleteOneAsync(filter);
        }        

        public async Task<Contact> GetContactAsync(Guid id)
        {
            var filter = filterBuilder.Eq(user => user.Id, id);
            return await usersCollection.Find(filter).FirstOrDefaultAsync();
        }

        public async Task<Contact> GetContactByUserNameAsync(string username)
        {
            var filter = filterBuilder.Eq(user => user.Username, username);
            return await usersCollection.Find(filter).FirstOrDefaultAsync();
        }
        public async Task<Contact> GetContactByEmailAsync(string email)
        {
            var filter = filterBuilder.Eq(user => user.Email, email);
            return await usersCollection.Find(filter).FirstOrDefaultAsync();
        }

        public async Task<Contact> GetContactByMobileNumberAsync(string mobilenumber)
        {
            var filter = filterBuilder.Eq(user => user.MobileNumber, mobilenumber);            
            return await usersCollection.Find(filter).FirstOrDefaultAsync();
        }

       public async Task<string> GetContactMobileNumberByProviderAsync(string provider, string providerId)
        {
            if (provider==Provider.FACEBOOK)
            {
                var filter = filterBuilder.Eq(user => user.FacebookId, providerId);
                var projection = Builders<Contact>.Projection.Expression(x => x.MobileNumber);
                return await usersCollection.Find(filter).Project(projection).FirstOrDefaultAsync();
            }
            else
            {
                var filter = filterBuilder.Eq(user => user.GoogleId, providerId);
                var projection = Builders<Contact>.Projection.Expression(x => x.MobileNumber);
                return await usersCollection.Find(filter).Project(projection).FirstOrDefaultAsync();
            }            
        }

        public async Task<bool> CheckContactMobileNumberAsync(string mobilenumber)
        {
            var filter = filterBuilder.Eq(user => user.MobileNumber, mobilenumber);
            return await usersCollection.Find(filter).AnyAsync();            
        }

        public async Task<IEnumerable<Contact>> GetContactsAsync()
        {
            return await usersCollection.Find(new BsonDocument()).ToListAsync();
        }

        public async Task<IEnumerable<Contact>> GetContactsByUserNameAsync(string userName)
        {
            var filter = filterBuilder.Eq(user => user.Username, userName);
            return await usersCollection.Find(filter).ToListAsync();
        }

        public async Task<bool> UpdateContactAsync(Contact user)
        {
            var filter = filterBuilder.Eq(existingUser => existingUser.Id, user.Id);
            var result = await usersCollection.ReplaceOneAsync(filter, user);
            return result.ModifiedCount > 0;
        }

        public async Task<bool> UpdateContactProviderIdAsync(string mobilenumber, string provider, string providerId)
        {
            var filter = filterBuilder.Eq(user => user.MobileNumber, mobilenumber);
            var update = Builders<Contact>.Update.Set(GetProviderFieldName(provider), providerId);
            var result = await usersCollection.UpdateOneAsync(filter, update);
            return result.ModifiedCount > 0;
        }

        // grab correct field name on db based on provider
        private string GetProviderFieldName(string provider)
        {
            switch (provider)
            {
                case Provider.FACEBOOK:
                    return "FacebookId";
                case Provider.GOOGLE:
                    return "GoogleId";
                case Provider.PHONE:
                    return "PhoneId";
                case Provider.PASSWORD:
                    return "PasswordId";
                default:
                    throw new ArgumentException("Invalid provider", nameof(provider));
            }
        }

    }
}
