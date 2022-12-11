using PinoyMassageService.Entities;
using static PinoyMassageService.Dtos.UserDtos;

namespace PinoyMassageService.Extensions
{
    public static class UserExtensions
    {        
        public static UserDto AsDto(this User user)
        {
            return new UserDto(user.Id, user.Username, user.Password, user.PasswordHash, user.PasswordSalt, user.RefreshToken, user.TokenCreated, user.TokenExpires, user.CreatedDate);
        }
    }
}
