using PinoyMassageService.Entities;

namespace PinoyMassageService.Repositories
{
    public interface IRefreshTokenRepository
    {
        Task<RefreshToken> GetRefreshTokenAsync(Guid id);
        Task<RefreshToken> GetRefreshTokenByUserIdAsync(Guid userId);
        Task CreateRefreshTokenAsync(RefreshToken refreshToken);
        Task UpdateRefreshTokenAsync(RefreshToken refreshToken);
        Task DeleteRefreshTokenAsync(Guid id);

    }
}
