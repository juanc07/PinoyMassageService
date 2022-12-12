using PinoyMassageService.Entities;
using static PinoyMassageService.Dtos.UserDtos;

namespace PinoyMassageService.Extensions
{
    public static class UserExtensions
    {
        public static UserDto AsDto(this User user)
        {
            return new UserDto(user.Id, user.Username, user.Password, user.PasswordHash, user.PasswordSalt, user.AccountType,
                user.Email, user.MobileNumber,user.FacebookId,user.GoogleId,user.CreatedDate);
        }
    }
}
