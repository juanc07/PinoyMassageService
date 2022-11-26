using System.ComponentModel.DataAnnotations;

namespace PinoyMassageService.Dtos
{
    public class ServiceDtos
    {
        public record ServiceDto(Guid Id, Guid ProviderId, Guid ClientId, string ServiceOffer, int ServicePrice, int Duration, string MeetUpLocation,
            int Status, int CreditCost, DateTimeOffset ExpiredAt, DateTimeOffset CreatedDate);

        public record CreateServiceDto([Required] Guid ProviderId, Guid ClientId, string ServiceOffer, int ServicePrice, int Duration, string MeetUpLocation,
            int Status, int CreditCost, DateTimeOffset ExpiredAt, DateTimeOffset CreatedDate);

        public record UpdateServiceDto(string ServiceOffer, int ServicePrice, int Duration, string MeetUpLocation,
            int Status, int CreditCost);

        public record UpdateServiceStatusDto(Guid ProviderId, Guid ClientId, int Status);
    }
}
