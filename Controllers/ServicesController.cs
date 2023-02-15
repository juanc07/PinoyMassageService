using Microsoft.AspNetCore.Mvc;
using PinoyMassageService.Constant;
using PinoyMassageService.Entities;
using PinoyMassageService.Extensions;
using PinoyMassageService.Repositories;
using static PinoyMassageService.Dtos.ServiceDtos;

namespace PinoyMassageService.Controllers
{
    // we might change or revise this we gonna based on dynamics 365 field service!!
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

        [HttpPost]
        public async Task<ActionResult<ServiceDto>> CreateServiceAsync(CreateServiceDto serviceDto)
        {
            var existingService = await repository.GetServiceByProviderIdAsync(serviceDto.ProviderId);
            if (existingService is null)
            {
                Service service = new()
                {
                    Id = Guid.NewGuid(),
                    ProviderId = serviceDto.ProviderId,
                    ClientId = Guid.Empty,
                    ServiceOffer = serviceDto.ServiceOffer,
                    ServicePrice = serviceDto.ServicePrice,
                    Duration = serviceDto.Duration,
                    MeetUpLocation = serviceDto.MeetUpLocation,
                    Status = serviceDto.Status,
                    CreditCost = serviceDto.CreditCost,
                    ExpiredAt = DateTimeOffset.UtcNow.AddHours(APIConfig.ServiceExpirationInHours),
                    CreatedDate = DateTimeOffset.UtcNow
                };

                await repository.CreateServiceAsync(service);
                return CreatedAtAction(nameof(GetServiceAsync), new { id = service.Id }, service.AsDto());
            }
            return BadRequest("This Provider Service still exists only one service at a time only!");
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
        

        [HttpPut("{id}")]        
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
        public async Task<ActionResult> UpdateServiceStatusByClientIdAsync(Guid clientId, UpdateClientServiceStatusDto serviceDto)
        {
            var existingService = await repository.GetServiceByClientIdAsync(clientId);
            if (existingService is null)
            {
                // not avail service yet
                var availService = await repository.GetServiceByProviderIdAsync(serviceDto.providerId);
                if (availService is null)
                {
                    return NotFound();
                }
                // check if service is taken or not
                if (availService.ClientId == Guid.Empty)
                {
                    // client is just trying to request to avail the service
                    availService.Status = serviceDto.Status;
                    availService.ClientId = clientId;
                    await repository.UpdateServiceAsync(availService);
                    return StatusCode(StatusCodes.Status200OK, "Request Service Successful");
                }                
                return StatusCode(StatusCodes.Status200OK, "Service is already taken");
            }
            else
            {
                if(existingService.Status != serviceDto.Status)
                {
                    // meaning the client want to cancel the service request
                    if (serviceDto.Status == ServiceStatus.Active)
                    {
                        existingService.ClientId = Guid.Empty;
                        existingService.Status = serviceDto.Status;
                        await repository.UpdateServiceAsync(existingService);
                        return StatusCode(StatusCodes.Status200OK, "Cancel Service request Successful");
                    }                    
                }
                return NoContent();
            }                        
        }

        [HttpPut("{providerId}")]        
        public async Task<ActionResult> UpdateServiceStatusByProviderIdAsync(Guid providerId, UpdateProviderServiceStatusDto serviceDto)
        {
            var existingService = await repository.GetServiceByProviderIdAsync(providerId);
            if (existingService is null)
            {
                return NotFound();
            }

            if (existingService.ClientId != Guid.Empty)
            {
                if (existingService.Status == ServiceStatus.Pending && serviceDto.Status == ServiceStatus.Decline)
                {
                    // reset the status to active again after declining
                    existingService.ClientId = Guid.Empty;
                    existingService.Status = ServiceStatus.Active;
                    await repository.UpdateServiceAsync(existingService);
                    // send notification or message that the avail of service is declined
                    //return NoContent();
                    return StatusCode(StatusCodes.Status200OK,"Decline Service request Successful");
                }
                else if (existingService.Status == ServiceStatus.Pending && serviceDto.Status == ServiceStatus.Accepted)
                {
                    existingService.Status = serviceDto.Status;
                    await repository.UpdateServiceAsync(existingService);
                    // send notification or message that the avail of service is accepted
                    return NoContent();
                }
                else if (existingService.Status == ServiceStatus.Accepted && (serviceDto.Status == ServiceStatus.Completed ||
                    serviceDto.Status == ServiceStatus.Canceled))
                {
                    existingService.Status = serviceDto.Status;
                    await repository.UpdateServiceAsync(existingService);
                    // send notification or message that the avail of service is completed or canceled
                    // save the service to service history table
                    return NoContent();
                }
            }

            return BadRequest("There's no Pending or Accepted service");
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
