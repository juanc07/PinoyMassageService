using MongoDB.Bson;
using MongoDB.Driver;
using PinoyMassageService.Entities;

namespace PinoyMassageService.Repositories
{
    public class MongoDbAccountRepository: IAccountsRepository
    {
        public const string databaseName = "pinoymassage";
        public const string collectionName = "accounts";
        private readonly IMongoCollection<Account> accountsCollection;
        private readonly FilterDefinitionBuilder<Account> filterBuilder = Builders<Account>.Filter;

        public MongoDbAccountRepository(IMongoClient mongoclient)
        {
            IMongoDatabase database = mongoclient.GetDatabase(databaseName);
            accountsCollection = database.GetCollection<Account>(collectionName);
        }

        public async Task CreateAccountAsync(Account account)
        {
            await accountsCollection.InsertOneAsync(account);
        }

        public async Task DeleteAccountAsync(Guid id)
        {
            var filter = filterBuilder.Eq(account => account.Id, id);
            await accountsCollection.DeleteOneAsync(filter);
        }

        public async Task<Account> GetAccountAsync(Guid id)
        {
            var filter = filterBuilder.Eq(account => account.Id, id);
            return await accountsCollection.Find(filter).FirstOrDefaultAsync();
        }

        public async Task<Account> GetAccountByUserNameAsync(string username)
        {
            var filter = filterBuilder.Eq(account => account.UserName, username);
            return await accountsCollection.Find(filter).FirstOrDefaultAsync();
        }

        public async Task<Account> GetAccountByEmailAsync(string email)
        {
            var filter = filterBuilder.Eq(account => account.Email, email);
            return await accountsCollection.Find(filter).FirstOrDefaultAsync();
        }        

        public async Task<Account> GetAccountByMobileNumberAsync(string mobilenumber)
        {
            var filter = filterBuilder.Eq(account => account.MobileNumber, mobilenumber);
            return await accountsCollection.Find(filter).FirstOrDefaultAsync();
        }        

        public async Task<Account> GetAccountByHandleNameAsync(string handlename)
        {
            var filter = filterBuilder.Eq(account => account.HandleName, handlename);
            return await accountsCollection.Find(filter).FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<Account>> GetAccountsAsync()
        {
            return await accountsCollection.Find(new BsonDocument()).ToListAsync();
        }

        public async Task UpdateAccountAsync(Account account)
        {
            var filter = filterBuilder.Eq(existingAccount => existingAccount.Id, account.Id);
            await accountsCollection.ReplaceOneAsync(filter, account);
        }
    }
}
