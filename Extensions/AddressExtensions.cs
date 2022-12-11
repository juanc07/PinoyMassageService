using PinoyMassageService.Entities;
using static PinoyMassageService.Dtos.AddressDtos;

namespace PinoyMassageService.Extensions
{    
    public static class AddressExtensions
    {
        public static AddressDto AsDto(this Address address)
        {
            return new AddressDto(address.Id, address.UserId, address.StreetNumber, address.Branggay, address.City,
                address.Country, address.ZipCode, address.CreatedDate);
        }        
    }
}
