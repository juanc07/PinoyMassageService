using MongoDB.Bson;
using MongoDB.Driver;
using PinoyMassageService.Entities;

namespace PinoyMassageService.Repositories
{
    public class MongoDbUserRepository : IUserRepository
    {
        public const string databaseName = "pinoymassage";
        public const string collectionName = "user";
        private readonly IMongoCollection<User> usersCollection;
        private readonly FilterDefinitionBuilder<User> filterBuilder = Builders<User>.Filter;

        public MongoDbUserRepository(IMongoClient mongoclient)
        {
            IMongoDatabase database = mongoclient.GetDatabase(databaseName);
            usersCollection = database.GetCollection<User>(collectionName);
        }        

        public async Task CreateUserAsync(User user)
        {
            await usersCollection.InsertOneAsync(user);
        }

        public async Task DeleteUserAsync(Guid id)
        {
            var filter = filterBuilder.Eq(user => user.Id, id);
            await usersCollection.DeleteOneAsync(filter);
        }        

        public async Task<User> GetUserAsync(Guid id)
        {
            var filter = filterBuilder.Eq(user => user.Id, id);
            return await usersCollection.Find(filter).FirstOrDefaultAsync();
        }

        public async Task<User> GetUserByUserNameAsync(string username)
        {
            var filter = filterBuilder.Eq(user => user.Username, username);
            return await usersCollection.Find(filter).FirstOrDefaultAsync();
        }
        public async Task<User> GetUserByEmailAsync(string email)
        {
            var filter = filterBuilder.Eq(user => user.Email, email);
            return await usersCollection.Find(filter).FirstOrDefaultAsync();
        }

        public async Task<User> GetUserByMobileNumberAsync(string mobilenumber)
        {
            var filter = filterBuilder.Eq(user => user.MobileNumber, mobilenumber);            
            return await usersCollection.Find(filter).FirstOrDefaultAsync();
        }

       public async Task<string> GetUserMobileNumberByProviderAsync(string provider, string providerId)
        {
            if (provider=="facebook")
            {
                var filter = filterBuilder.Eq(user => user.FacebookId, providerId);
                var projection = Builders<User>.Projection.Expression(x => x.MobileNumber);
                return await usersCollection.Find(filter).Project(projection).FirstOrDefaultAsync();
            }
            else
            {
                var filter = filterBuilder.Eq(user => user.GoogleId, providerId);
                var projection = Builders<User>.Projection.Expression(x => x.MobileNumber);
                return await usersCollection.Find(filter).Project(projection).FirstOrDefaultAsync();
            }            
        }

        public async Task<bool> CheckUserMobileNumberAsync(string mobilenumber)
        {
            var filter = filterBuilder.Eq(user => user.MobileNumber, mobilenumber);
            return await usersCollection.Find(filter).AnyAsync();            
        }

        public async Task<IEnumerable<User>> GetUsersAsync()
        {
            return await usersCollection.Find(new BsonDocument()).ToListAsync();
        }

        public async Task UpdateUserAsync(User user)
        {
            var filter = filterBuilder.Eq(existingUser => existingUser.Id, user.Id);
            await usersCollection.ReplaceOneAsync(filter, user);
        }

        public async Task UpdateUserGoogleUserIdAsync(string mobilenumber, string googleUserId)
        {
            var filter = filterBuilder.Eq(user => user.MobileNumber, mobilenumber);
            var update = Builders<User>.Update.Set("GoogleId", googleUserId);
            await usersCollection.UpdateOneAsync(filter, update);
        }

        public async Task UpdateUserFacebookUserIdAsync(string mobilenumber, string facebookUserId)
        {
            var filter = filterBuilder.Eq(user => user.MobileNumber, mobilenumber);
            var update = Builders<User>.Update.Set("FacebookId", facebookUserId);
            await usersCollection.UpdateOneAsync(filter, update);
        }
    }
}
