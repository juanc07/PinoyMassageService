using System.ComponentModel.DataAnnotations;

namespace PinoyMassageService.Dtos
{
    public class AddressDtos
    {
        public record AddressDto(Guid Id, Guid UserId, string StreetNumber, string Branggay, string City,
             string Country, string ZipCode, DateTimeOffset CreatedDate );

        public record CreateAddressDto([Required] Guid userId, string StreetNumber, string Branggay, string City,
             string Country, string ZipCode, DateTimeOffset CreatedDate);

        public record UpdateAddressDto(string StreetNumber, string Branggay, string City,
             string Country, string ZipCode);
    }
}
