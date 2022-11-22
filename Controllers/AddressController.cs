﻿using Microsoft.AspNetCore.Mvc;
using PinoyMassageService.Entities;
using PinoyMassageService.Extensions;
using PinoyMassageService.Repositories;
using System.Net;
using static PinoyMassageService.Dtos.AccountDtos;
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
            var foundAddress = await repository.GetAddressByAccountIdAsync(addressDto.AccountId);
            if (foundAddress is null)
            {
                Address address = new()
                {
                    Id = Guid.NewGuid(),
                    AccountId = addressDto.AccountId,
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

        // GET /address/{id}
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

        // GET /address/{id}
        [HttpGet("{accountId}")]
        public async Task<ActionResult<AddressDto>> GetAddressByAccountIdAsync(Guid accountId)
        {
            var address = await repository.GetAddressByAccountIdAsync(accountId);
            if (address is null)
            {
                return NotFound();
            }
            return address.AsDto();
        }

        // Gets /accounts        
        [HttpGet]
        public async Task<IEnumerable<AddressDto>> GetAddressesAsync()
        {
            var addresses = (await repository.GetAddressessAsync()).Select(address => address.AsDto());

            logger.LogInformation($"{DateTime.UtcNow.ToString("hh:mm:ss")}: GetAddressesAsync Retrieved {addresses.Count()} addresses");
            return addresses;
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

        [HttpDelete("{accountId}")]
        public async Task<ActionResult> DeleteAddressByAccountIdAsync(Guid accountId)
        {
            var existingAddress = await repository.GetAddressByAccountIdAsync(accountId);
            if (existingAddress is null)
            {
                return NotFound();
            }

            await repository.DeleteAddressByAccountIdAsync(accountId);
            return NoContent();
        }

        [HttpDelete("{accountId}")]
        public async Task<ActionResult> DeleteAllAddressByAccountIdAsync(Guid accountId)
        {
            var deleteResult = await repository.DeleteAllAddressByAccountIdAsync(accountId);
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
