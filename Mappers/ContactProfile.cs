using AutoMapper;
using PinoyMassageService.Dtos;
using PinoyMassageService.Entities;
using static PinoyMassageService.Dtos.ContactDtos;

namespace PinoyMassageService.Mappers
{
    public class ContactProfile : Profile
    {
        public ContactProfile()
        {
            CreateMap<Contact, ContactDto>();
        }
    }
}
