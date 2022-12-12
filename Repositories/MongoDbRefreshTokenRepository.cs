using MongoDB.Driver;
using PinoyMassageService.Entities;

namespace PinoyMassageService.Repositories
{
    public class MongoDbRefreshTokenRepository : IRefreshTokenRepository
    {
        public const string databaseName = "pinoymassage";
        public const string collectionName = "refreshtoken";
        private readonly IMongoCollection<RefreshToken> refreshTokenCollection;
        private readonly FilterDefinitionBuilder<RefreshToken> filterBuilder = Builders<RefreshToken>.Filter;

        public MongoDbRefreshTokenRepository(IMongoClient mongoclient)
        {
            IMongoDatabase database = mongoclient.GetDatabase(databaseName);
            refreshTokenCollection = database.GetCollection<RefreshToken>(collectionName);
        }

        public async Task CreateRefreshTokenAsync(RefreshToken refreshToken)
        {
            await refreshTokenCollection.InsertOneAsync(refreshToken);
        }

        public async Task DeleteRefreshTokenAsync(Guid id)
        {
            var filter = filterBuilder.Eq(refreshToken => refreshToken.Id, id);
            await refreshTokenCollection.DeleteOneAsync(filter);
        }

        public async Task<RefreshToken> GetRefreshTokenAsync(Guid id)
        {
            var filter = filterBuilder.Eq(refreshToken => refreshToken.Id, id);
            return await refreshTokenCollection.Find(filter).FirstOrDefaultAsync();
        }        

        public async Task<RefreshToken> GetRefreshTokenByUserIdAsync(Guid userId)
        {
            var filter = filterBuilder.Eq(refreshToken => refreshToken.UserId, userId);
            return await refreshTokenCollection.Find(filter).FirstOrDefaultAsync();
        }        

        public async Task UpdateRefreshTokenAsync(RefreshToken refreshToken)
        {
            var filter = filterBuilder.Eq(existingRefreshToken => existingRefreshToken.Id, refreshToken.Id);
            await refreshTokenCollection.ReplaceOneAsync(filter, refreshToken);
        }
    }
}
