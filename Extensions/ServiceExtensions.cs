using PinoyMassageService.Entities;
using static PinoyMassageService.Dtos.ServiceDtos;

namespace PinoyMassageService.Extensions
{
    public static class ServiceExtensions
    {
        public static ServiceDto AsDto(this Service service)
        {
            return new ServiceDto(service.Id, service.ProviderId, service.ClientId, service.ServiceOffer, service.ServicePrice, service.Duration,
                service.MeetUpLocation, service.Status, service.CreditCost, service.ExpiredAt, service.CreatedDate);
        }
    }
}
