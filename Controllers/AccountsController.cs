﻿using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using PinoyMassageService.Entities;
using PinoyMassageService.Repositories;
using static PinoyMassageService.Dtos.AccountDtos;

namespace PinoyMassageService.Controllers
{
    [ApiController]
    //[Route("accounts")]
    [Route("[controller]/[action]")]
    public class AccountsController:ControllerBase
    {
        private readonly IAccountRepository repository;
        private readonly IMapper _mapper;
        private readonly ILogger<AccountsController> logger;

        public AccountsController(IAccountRepository repository, IMapper mapper, ILogger<AccountsController> logger)
        {
            this.repository = repository;
            this._mapper = mapper;
            this.logger = logger;
        }

        [HttpPost]
        public async Task<ActionResult<AccountDto>> CreateAccountAsync(CreateAccountDto accountDto)
        {
            // make sure account does not exists before creating it
            var foundAccount = await repository.GetAccountByUserIdAsync(accountDto.UserId);
            if (foundAccount is null)
            {
                Account account = new()
                {
                    Id = Guid.NewGuid(),
                    UserId = accountDto.UserId,                    
                    CreatedDate = DateTimeOffset.UtcNow
                };

                await repository.CreateAccountAsync(account);
                return CreatedAtAction(nameof(GetAccountAsync), new { id = account.Id }, _mapper.Map<Account, AccountDto>(account));
            }
            return BadRequest("Account already exists!");
        }

        // GET /accounts/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<AccountDto>> GetAccountAsync(Guid id)
        {
            var account = await repository.GetAccountAsync(id);
            if (account is null)
            {
                return NotFound();
            }
            return _mapper.Map<Account, AccountDto>(account);
        }

        [HttpGet("{userId}")]
        public async Task<ActionResult<AccountDto>> GetAccountByUserIdAsync(Guid userId)
        {
            var account = await repository.GetAccountByUserIdAsync(userId);
            if (account is null)
            {
                return NotFound();
            }
            return _mapper.Map<Account, AccountDto>(account);
        }        

        // Gets /accounts        
        [HttpGet]
        public async Task<IEnumerable<AccountDto>> GetAccountsAsync()
        {
            var accounts = (await repository.GetAccountsAsync()).Select(account => _mapper.Map<AccountDto>(account));
            logger.LogInformation($"{DateTime.UtcNow.ToString("hh:mm:ss")}: GetAllAccountsAsync Retrieved {accounts.Count()} accounts");
            return accounts;
        }


        // PUT /accounts/id        
        [HttpPut("{id}")]
        //[Route("/updatebasicinfoasync")]
        public async Task<ActionResult> UpdateBasicInfoAsync(Guid id, UpdateBasicDto accountDto)
        {
            var existingAccount = await repository.GetAccountAsync(id);
            if (existingAccount is null)
            {
                return NotFound();
            }

            existingAccount.FirstName = accountDto.FirstName;
            existingAccount.LastName = accountDto.LastName;            

            await repository.UpdateAccountAsync(existingAccount);
            return NoContent();
        }

        
        [HttpPut("{id}")]
        //[Route("/updateaccountasync")]
        public async Task<ActionResult> UpdateAccountAsync(Guid id, UpdateAccountDto accountDto)
        {
            var existingAccount = await repository.GetAccountAsync(id);
            if (existingAccount is null)
            {
                return NotFound();
            }

            existingAccount.FirstName = accountDto.FirstName;
            existingAccount.LastName = accountDto.LastName;            
            existingAccount.IdentificationType = accountDto.IdentificationType;
            existingAccount.IdentificationNumber = accountDto.IdentificationNumber;

            await repository.UpdateAccountAsync(existingAccount);
            return NoContent();
        }                        

        // PUT /accounts/id        
        [HttpPut("{id}")]
        public async Task<ActionResult> UpdateIdentificationAsync(Guid id, UpdateIdentificationDto identificationDto)
        {
            var existingAccount = await repository.GetAccountAsync(id);
            if (existingAccount is null)
            {
                return NotFound();
            }

            existingAccount.IdentificationType = identificationDto.IdentificationType;
            existingAccount.IdentificationNumber = identificationDto.IdentificationNumber;

            await repository.UpdateAccountAsync(existingAccount);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteAccountAsync(Guid id)
        {
            var existingAccount = await repository.GetAccountAsync(id);
            if (existingAccount is null)
            {
                return NotFound();
            }

            await repository.DeleteAccountAsync(id);
            return NoContent();
        }

        [HttpDelete("{userId}")]
        public async Task<ActionResult> DeleteAccountByUserIdAsync(Guid userId)
        {
            var existingAccount = await repository.GetAccountByUserIdAsync(userId);
            if (existingAccount is null)
            {
                return NotFound();
            }

            await repository.DeleteAccountByUserIdAsync(userId);
            return NoContent();
        }
    }
}
