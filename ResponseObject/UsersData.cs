using PinoyMassageService.Dtos;
using static PinoyMassageService.Dtos.UserDtos;

namespace PinoyMassageService.ResponseObject
{
    public class UsersData
    {
        public List<UserDto> Users { get; set; }
    }
}
