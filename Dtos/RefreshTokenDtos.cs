using System.ComponentModel.DataAnnotations;

namespace PinoyMassageService.Dtos
{
    public class RefreshTokenDtos
    {
        public record RefreshTokenDto(Guid Id, Guid UserId, string Token, DateTime TokenCreated, DateTime TokenExpires, DateTime Created);
        public record CreateRefreshTokenDto([Required] Guid UserId, string Token, string TokenCreated, string TokenExpires, DateTime Created);
    }
}
