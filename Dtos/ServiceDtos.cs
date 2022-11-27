using System.ComponentModel.DataAnnotations;

namespace PinoyMassageService.Dtos
{
    public class ServiceDtos
    {
        public record ServiceDto(Guid Id, Guid ProviderId, Guid ClientId, string ServiceOffer, int ServicePrice, int Duration, string MeetUpLocation,
            int Status, int CreditCost, DateTimeOffset ExpiredAt, DateTimeOffset CreatedDate, DateTimeOffset CompletedDate);

        public record CreateServiceDto([Required] Guid ProviderId,string ServiceOffer, int ServicePrice, int Duration, string MeetUpLocation,
            int Status, int CreditCost, DateTimeOffset ExpiredAt, DateTimeOffset CreatedDate, DateTimeOffset CompletedDate);

        public record UpdateServiceDto(string ServiceOffer, int ServicePrice, int Duration, string MeetUpLocation,
            int Status, int CreditCost);

        public record UpdateProviderServiceStatusDto(int Status);
        public record UpdateClientServiceStatusDto(Guid providerId, int Status);
    }
}
