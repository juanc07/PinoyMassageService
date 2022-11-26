using Microsoft.AspNetCore.Mvc;
using PinoyMassageService.Constant;
using PinoyMassageService.Entities;
using PinoyMassageService.Extensions;
using PinoyMassageService.Repositories;
using static PinoyMassageService.Dtos.AccountDtos;
using static PinoyMassageService.Dtos.ServiceDtos;

namespace PinoyMassageService.Controllers
{
    [ApiController]    
    [Route("[controller]/[action]")]
    public class ServicesController : ControllerBase
    {
        private readonly IServiceRepository repository;
        private readonly ILogger<ServicesController> logger;

        public ServicesController(IServiceRepository repository, ILogger<ServicesController> logger)
        {
            this.repository = repository;
            this.logger = logger;
        }

        /*
         * public record CreateServiceDto([Required] Guid ProviderId, Guid ClientId, string ServiceOffer, int ServicePrice, int Duration, string MeetUpLocation,
            int Status, int CreditCost, DateTimeOffset ExpiredAt, DateTimeOffset CreatedDate);
         */

        [HttpPost]
        public async Task<ActionResult<ServiceDto>> CreateServiceAsync(CreateServiceDto serviceDto)
        {
            Service service = new()
            {
                Id = Guid.NewGuid(),
                ProviderId = serviceDto.ProviderId,
                ClientId = serviceDto.ClientId,
                ServiceOffer = serviceDto.ServiceOffer,
                ServicePrice = serviceDto.ServicePrice,
                Duration = serviceDto.Duration,
                MeetUpLocation = serviceDto.MeetUpLocation,
                Status = serviceDto.Status,
                CreditCost = serviceDto.CreditCost,
                ExpiredAt = serviceDto.ExpiredAt,
                CreatedDate = DateTimeOffset.UtcNow
            };

            await repository.CreateServiceAsync(service);
            return CreatedAtAction(nameof(GetServiceAsync), new { id = service.Id }, service.AsDto());
        }

        // GET /services/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<ServiceDto>> GetServiceAsync(Guid id)
        {
            var service = await repository.GetServiceAsync(id);
            if (service is null)
            {
                return NotFound();
            }
            return service.AsDto();
        }

        // GET /services/{providerId}
        [HttpGet("{providerId}")]
        public async Task<ActionResult<ServiceDto>> GetServiceByProviderIdAsync(Guid providerId)
        {
            var service = await repository.GetServiceByProviderIdAsync(providerId);
            if (service is null)
            {
                return NotFound();
            }
            return service.AsDto();
        }

        // GET /services/{clientId}
        [HttpGet("{clientId}")]
        public async Task<ActionResult<ServiceDto>> GetServiceByClientIdAsync(Guid clientId)
        {
            var service = await repository.GetServiceByClientIdAsync(clientId);
            if (service is null)
            {
                return NotFound();
            }
            return service.AsDto();
        }

        // Get all Services
        // Gets /services        
        [HttpGet]
        public async Task<IEnumerable<ServiceDto>> GetServicesAsync()
        {
            var services = (await repository.GetServicesAsync()).Select(service => service.AsDto());

            logger.LogInformation($"{DateTime.UtcNow.ToString("hh:mm:ss")}: GetServicesAsync Retrieved {services.Count()} accounts");
            return services;
        }

        // Get All Active Services only
        // Gets /services        
        [HttpGet]
        public async Task<IEnumerable<ServiceDto>> GetActiveServicesAsync()
        {
            var services = (await repository.GetServicesAsync()).Select(service => service.AsDto());

            services = services.Where(service => service.Status == ServiceStatus.Active);

            logger.LogInformation($"{DateTime.UtcNow.ToString("hh:mm:ss")}: GetActiveServicesAsync Retrieved {services.Count()} services");
            return services;
        }

        /*
         * public record UpdateServiceDto(string ServiceOffer, int ServicePrice, int Duration, string MeetUpLocation,
            int Status, int CreditCost);

        public record UpdateServiceStatusDto(Guid ProviderId, Guid ClientId, int Status);
         */

        [HttpPut("{id}")]
        //[Route("/UpdateServiceAsync")]
        public async Task<ActionResult> UpdateServiceAsync(Guid id, UpdateServiceDto serviceDto)
        {
            var existingService = await repository.GetServiceAsync(id);
            if (existingService is null)
            {
                return NotFound();
            }

            existingService.ServiceOffer = serviceDto.ServiceOffer;
            existingService.ServicePrice = serviceDto.ServicePrice;
            existingService.Duration = serviceDto.Duration;
            existingService.MeetUpLocation = serviceDto.MeetUpLocation;
            existingService.CreditCost = serviceDto.CreditCost;

            await repository.UpdateServiceAsync(existingService);
            return NoContent();
        }

        [HttpPut("{providerId}")]
        //[Route("/UpdateServiceByProviderIdAsync")]
        public async Task<ActionResult> UpdateServiceByProviderIdAsync(Guid providerId, UpdateServiceDto serviceDto)
        {
            var existingService = await repository.GetServiceByProviderIdAsync(providerId);
            if (existingService is null)
            {
                return NotFound();
            }

            existingService.ServiceOffer = serviceDto.ServiceOffer;
            existingService.ServicePrice = serviceDto.ServicePrice;
            existingService.Duration = serviceDto.Duration;
            existingService.MeetUpLocation = serviceDto.MeetUpLocation;
            // you can call oracle here or not but the goal is to query the correct pricing but maybe this is a wrong approach
            existingService.CreditCost = serviceDto.CreditCost;

            await repository.UpdateServiceAsync(existingService);
            return NoContent();
        }

        [HttpPut("{clientId}")]
        //[Route("/UpdateServiceByClientIdAsync")]
        public async Task<ActionResult> UpdateServiceByClientIdAsync(Guid clientId, UpdateServiceStatusDto serviceDto)
        {
            var existingService = await repository.GetServiceByClientIdAsync(clientId);
            if (existingService is null)
            {
                return NotFound();
            }

            existingService.ClientId = serviceDto.ClientId;
            existingService.Status = serviceDto.Status;


            await repository.UpdateServiceAsync(existingService);
            return NoContent();
        }

        [HttpPut("{providerId}")]
        //[Route("/UpdateServiceByProviderIdAsync")]
        public async Task<ActionResult> UpdateServiceByProviderIdAsync(Guid providerId, UpdateServiceStatusDto serviceDto)
        {
            var existingService = await repository.GetServiceByProviderIdAsync(providerId);
            if (existingService is null)
            {
                return NotFound();
            }

            existingService.ProviderId = serviceDto.ProviderId;
            existingService.Status = serviceDto.Status;

            await repository.UpdateServiceAsync(existingService);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteServiceAsync(Guid id)
        {
            var existingService = await repository.GetServiceAsync(id);
            if (existingService is null)
            {
                return NotFound();
            }

            await repository.DeleteServiceAsync(id);
            return NoContent();
        }

        [HttpDelete("{providerId}")]
        public async Task<ActionResult> DeleteServiceByProviderIdAsync(Guid providerId)
        {
            var existingService = await repository.GetServiceByProviderIdAsync(providerId);
            if (existingService is null)
            {
                return NotFound();
            }

            await repository.DeleteServiceByProviderIdAsync(providerId);
            return NoContent();
        }

        [HttpDelete("{providerId}")]
        public async Task<ActionResult> DeleteAllServiceByProviderIdAsync(Guid providerId)
        {
            var deleteResult = await repository.DeleteAllServiceByProviderIdAsync(providerId);
            if (deleteResult.DeletedCount == 0)
            {
                return NotFound();
            }
            else
            {
                return Content($"Service deleted count: {deleteResult.DeletedCount}");
            }
        }
    }
}
