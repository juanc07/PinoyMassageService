using System.ComponentModel.DataAnnotations;

namespace PinoyMassageService.Dtos
{
    public class AddressDtos
    {
        public record AddressDto(Guid Id, Guid AccountId, string StreetNumber, string Branggay, string City,
             string Country, string ZipCode, DateTimeOffset CreatedDate );

        public record CreateAddressDto([Required] Guid AccountId, string StreetNumber, string Branggay, string City,
             string Country, string ZipCode, DateTimeOffset CreatedDate);

        //public record DeleteAddressDto(Guid AccountId);
    }
}
