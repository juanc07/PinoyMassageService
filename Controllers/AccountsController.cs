using Microsoft.AspNetCore.Mvc;
using PinoyMassageService.Entities;
using PinoyMassageService.Extensions;
using PinoyMassageService.Repositories;
using static PinoyMassageService.Dtos.AccountDtos;

namespace PinoyMassageService.Controllers
{
    [ApiController]
    [Route("accounts")]
    public class AccountsController:ControllerBase
    {
        private readonly IAccountsRepository repository;
        private readonly ILogger<AccountsController> logger;

        public AccountsController(IAccountsRepository repository, ILogger<AccountsController> logger)
        {
            this.repository = repository;
            this.logger = logger;
        }

        // Gets /accounts        
        [HttpGet]
        public async Task<IEnumerable<AccountDto>> GetAccountsAsync(string userName = null)
        {
            var accounts = (await repository.GetAccountsAsync()).Select(account => account.AsDto());

            if (!string.IsNullOrWhiteSpace(userName))
            {
                accounts = accounts.Where(account => account.UserName.Contains(userName, StringComparison.OrdinalIgnoreCase));
            }

            logger.LogInformation($"{DateTime.UtcNow.ToString("hh:mm:ss")}: Retrieved {accounts.Count()} accounts");
            return accounts;
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
            return account.AsDto();
        }        

        [HttpPost]
        public async Task<ActionResult<AccountDto>> CreateAccountAsync(CreateAccountDto itemDto)
        {
            Account account = new()
            {
                Id = Guid.NewGuid(),
                UserName = itemDto.UserName,
                Password = itemDto.Password,
                Email = itemDto.Email,
                AccountType = itemDto.AccountType,
                CreatedDate = DateTimeOffset.UtcNow
            };

            await repository.CreateAccountAsync(account);
            return CreatedAtAction(nameof(GetAccountAsync), new { id = account.Id }, account.AsDto());
        }

        // PUT /accounts/id        
        [HttpPut("{id}")]
        public async Task<ActionResult> UpdateAccountAsync(Guid id, UpdateAccountDto accountDto)
        {
            var existingAccount = await repository.GetAccountAsync(id);
            if (existingAccount is null)
            {
                return NotFound();
            }

            existingAccount.ProfileName = accountDto.ProfileName;
            existingAccount.MobileNumber = accountDto.MobileNumber;
            existingAccount.BirthDate = accountDto.BirthDate;
            existingAccount.Password = accountDto.Password;

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
    }
}
