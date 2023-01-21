using AutoMapper;
using PinoyMassageService.Dtos;
using PinoyMassageService.Entities;
using static PinoyMassageService.Dtos.UserDtos;

namespace PinoyMassageService.Mappers
{
    public class UserProfile : Profile
    {
        public UserProfile()
        {
            CreateMap<User, UserDto>();
        }
    }
}
