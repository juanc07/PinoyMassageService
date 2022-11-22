using MongoDB.Bson;
using MongoDB.Driver;
using PinoyMassageService.Entities;

namespace PinoyMassageService.Repositories
{
    public class MongoDbProfileImageRepository: IProfileImageRepository
    {

        public const string databaseName = "pinoymassage";
        public const string collectionName = "profileimage";
        private readonly IMongoCollection<ProfileImage> profileImageCollection;
        private readonly FilterDefinitionBuilder<ProfileImage> filterBuilder = Builders<ProfileImage>.Filter;

        public MongoDbProfileImageRepository(IMongoClient mongoclient)
        {
            IMongoDatabase database = mongoclient.GetDatabase(databaseName);
            profileImageCollection = database.GetCollection<ProfileImage>(collectionName);
        }

        public async Task CreateProfileImageAsync(ProfileImage profileImage)
        {
            await profileImageCollection.InsertOneAsync(profileImage);
        }                

        public async Task<ProfileImage> GetProfileImageAsync(Guid id)
        {
            var filter = filterBuilder.Eq(profileImage => profileImage.Id, id);
            return await profileImageCollection.Find(filter).FirstOrDefaultAsync();
        }

        public async Task<ProfileImage> GetProfileImageByAccountIdAsync(Guid accountId)
        {
            var filter = filterBuilder.Eq(profileImage => profileImage.AccountId, accountId);
            return await profileImageCollection.Find(filter).FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<ProfileImage>> GetProfileImagesAsync()
        {
            return await profileImageCollection.Find(new BsonDocument()).ToListAsync();
        }
        

        public async Task UpdateProfileImageAsync(ProfileImage profileImage)
        {
            var filter = filterBuilder.Eq(existingprofileImage => existingprofileImage.AccountId, profileImage.AccountId);
            await profileImageCollection.ReplaceOneAsync(filter, profileImage);
        }

        public async Task DeleteProfileImageAsync(Guid id)
        {
            var filter = filterBuilder.Eq(profileImage => profileImage.Id, id);
            await profileImageCollection.DeleteOneAsync(filter);
        }

        public async Task DeleteProfileImageByAccountIdAsync(Guid accountId)
        {
            var filter = filterBuilder.Eq(profileImage => profileImage.AccountId, accountId);
            await profileImageCollection.DeleteOneAsync(filter);
        }

        public async Task<DeleteResult> DeleteAllProfileImageByAccountIdAsync(Guid accountId)
        {
            var filter = filterBuilder.Eq(profileImage => profileImage.AccountId, accountId);
            return await profileImageCollection.DeleteManyAsync(filter);
        }        
    }
}
