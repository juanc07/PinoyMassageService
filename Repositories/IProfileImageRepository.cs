using MongoDB.Driver;
using PinoyMassageService.Entities;

namespace PinoyMassageService.Repositories
{
    public interface IProfileImageRepository
    {
        Task<ProfileImage> GetProfileImageAsync(Guid id);
        Task<ProfileImage> GetProfileImageByAccountIdAsync(Guid accountId);
        Task<IEnumerable<ProfileImage>> GetProfileImagesAsync();
        Task CreateProfileImageAsync(ProfileImage profileImage);
        Task UpdateProfileImageAsync(ProfileImage profileImage);
        Task DeleteProfileImageAsync(Guid id);
        Task DeleteProfileImageByAccountIdAsync(Guid accountId);
        Task<DeleteResult> DeleteAllProfileImageByAccountIdAsync(Guid accountId);
    }
}
