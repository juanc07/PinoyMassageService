using AutoMapper;
using PinoyMassageService.Entities;
using static PinoyMassageService.Dtos.AccountDtos;
using static PinoyMassageService.Dtos.UserDtos;

namespace PinoyMassageService.Mappers
{
    public class AccountProfile: Profile
    {
        public AccountProfile()
        {
            CreateMap<Account, AccountDto>();
        }
    }
}
