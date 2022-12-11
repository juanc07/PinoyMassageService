using MongoDB.Driver;
using PinoyMassageService.Entities;

namespace PinoyMassageService.Repositories
{
    public interface IProfileImageRepository
    {
        Task<ProfileImage> GetProfileImageAsync(Guid id);
        Task<ProfileImage> GetProfileImageByUserIdAsync(Guid userId);
        Task<IEnumerable<ProfileImage>> GetProfileImagesAsync();
        Task CreateProfileImageAsync(ProfileImage profileImage);
        Task UpdateProfileImageAsync(ProfileImage profileImage);
        Task DeleteProfileImageAsync(Guid id);
        Task DeleteProfileImageByUserIdAsync(Guid UserId);
        Task<DeleteResult> DeleteAllProfileImageByUserIdAsync(Guid UserId);
    }
}
