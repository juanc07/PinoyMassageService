using Microsoft.AspNetCore.Mvc;
using PinoyMassageService.Entities;
using PinoyMassageService.Extensions;
using PinoyMassageService.Repositories;
using static PinoyMassageService.Dtos.AddressDtos;

namespace PinoyMassageService.Controllers
{
    [ApiController]
    [Route("[controller]/[action]")]
    public class AddressController : ControllerBase
    {
        private readonly IAddressRepository repository;
        private readonly ILogger<AddressController> logger;

        public AddressController(IAddressRepository repository, ILogger<AddressController> logger)
        {
            this.repository = repository;
            this.logger = logger;
        }

        [HttpPost]
        public async Task<ActionResult<AddressDto>> CreateAddressAsync(CreateAddressDto addressDto)
        {
            var foundAddress = await repository.GetAddressByUserIdAsync(addressDto.userId);
            if (foundAddress is null)
            {
                Address address = new()
                {
                    Id = Guid.NewGuid(),
                    UserId = addressDto.userId,
                    StreetNumber = addressDto.StreetNumber,
                    Branggay = addressDto.Branggay,
                    City = addressDto.City,
                    Country = addressDto.Country,
                    ZipCode = addressDto.ZipCode,
                    CreatedDate = DateTimeOffset.UtcNow
                };

                await repository.CreateAddressAsync(address);
                return CreatedAtAction(nameof(GetAddressAsync), new { id = address.Id }, address.AsDto());
            }
            return BadRequest("Address already exists!");
        }

        // GET /Address/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<AddressDto>> GetAddressAsync(Guid id)
        {
            var address = await repository.GetAddressAsync(id);
            if (address is null)
            {
                return NotFound();
            }
            return address.AsDto();
        }

        // GET /Address/{id}
        [HttpGet("{userId}")]
        public async Task<ActionResult<AddressDto>> GetAddressByUserIdAsync(Guid userId)
        {
            var address = await repository.GetAddressByUserIdAsync(userId);
            if (address is null)
            {
                return NotFound();
            }
            return address.AsDto();
        }

        // /Address/GetAddressesAsync
        [HttpGet]
        public async Task<IEnumerable<AddressDto>> GetAddressesAsync()
        {
            var addresses = (await repository.GetAddressessAsync()).Select(address => address.AsDto());

            logger.LogInformation($"{DateTime.UtcNow.ToString("hh:mm:ss")}: GetAddressesAsync Retrieved {addresses.Count()} addresses");
            return addresses;
        }        

        [HttpPut("{id}")]        
        public async Task<ActionResult> UpdateAddressAsync(Guid id, UpdateAddressDto addressDto)
        {
            var existingAddress = await repository.GetAddressAsync(id);
            if (existingAddress is null)
            {
                return NotFound();
            }

            existingAddress.StreetNumber = addressDto.StreetNumber;
            existingAddress.Branggay = addressDto.Branggay;
            existingAddress.City = addressDto.City;
            existingAddress.Country = addressDto.Country;
            existingAddress.ZipCode = addressDto.ZipCode;

            await repository.UpdateAddressAsync(existingAddress);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteAddressAsync(Guid id)
        {
            var existingAddress = await repository.GetAddressAsync(id);
            if (existingAddress is null)
            {
                return NotFound();
            }

            await repository.DeleteAddressAsync(id);
            return NoContent();
        }

        [HttpDelete("{userId}")]
        public async Task<ActionResult> DeleteAddressByUserIdAsync(Guid userId)
        {
            var existingAddress = await repository.GetAddressByUserIdAsync(userId);
            if (existingAddress is null)
            {
                return NotFound();
            }

            await repository.DeleteAddressByUserIdAsync(userId);
            return NoContent();
        }

        [HttpDelete("{userId}")]
        public async Task<ActionResult> DeleteAllAddressByUserIdAsync(Guid userId)
        {
            var deleteResult = await repository.DeleteAllAddressByUserIdAsync(userId);
            if (deleteResult.DeletedCount == 0)
            {
                return NotFound();
            }
            else
            {
                return Content($"Address deleted count: {deleteResult.DeletedCount}");
            }
        }
    }
}
