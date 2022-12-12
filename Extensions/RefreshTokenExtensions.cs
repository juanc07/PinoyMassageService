using PinoyMassageService.Entities;
using static PinoyMassageService.Dtos.RefreshTokenDtos;

namespace PinoyMassageService.Extensions
{
    public static class RefreshTokenExtensions
    {
        public static RefreshTokenDto AsDto(this RefreshToken refreshToken)
        {
            return new RefreshTokenDto(refreshToken.Id, refreshToken.UserId, refreshToken.Token,
                refreshToken.TokenCreated, refreshToken.TokenExpires, refreshToken.CreatedDate );
        }
    }
}
