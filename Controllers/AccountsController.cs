using Microsoft.AspNetCore.Mvc;
using PinoyMassageService.Entities;
using PinoyMassageService.Extensions;
using PinoyMassageService.Repositories;
using static PinoyMassageService.Dtos.AccountDtos;

namespace PinoyMassageService.Controllers
{
    [ApiController]
    //[Route("accounts")]
    [Route("[controller]/[action]")]
    public class AccountsController:ControllerBase
    {
        private readonly IAccountsRepository repository;
        private readonly ILogger<AccountsController> logger;

        public AccountsController(IAccountsRepository repository, ILogger<AccountsController> logger)
        {
            this.repository = repository;
            this.logger = logger;
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
                MobileNumber = itemDto.MobileNumber,
                AccountType = itemDto.AccountType,
                HandleName = itemDto.HandleName,
                BirthDate = itemDto.BirthDate,
                Gender = itemDto.Gender,
                CreatedDate = DateTimeOffset.UtcNow
            };

            await repository.CreateAccountAsync(account);
            return CreatedAtAction(nameof(GetAccountAsync), new { id = account.Id }, account.AsDto());
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

        // GET /accounts/{email}
        [HttpGet("{email}")]
        public async Task<ActionResult<AccountDto>> GetAccountByEmailAsync(string email)
        {
            var account = await repository.GetAccountByEmailAsync(email);
            if (account is null)
            {
                return NotFound();
            }
            return account.AsDto();
        }

        // GET /accounts/{username}
        [HttpGet("{username}")]
        public async Task<ActionResult<AccountDto>> GetAccountByUserNameAsync(string username)
        {
            var account = await repository.GetAccountByUserNameAsync(username);
            if (account is null)
            {
                return NotFound();
            }
            return account.AsDto();
        }

        // GET /accounts/{mobilenumber}
        [HttpGet("{mobilenumber}")]
        public async Task<ActionResult<AccountDto>> GetAccountByMobileNumberAsync(string mobilenumber)
        {
            var account = await repository.GetAccountByMobileNumberAsync(mobilenumber);
            if (account is null)
            {
                return NotFound();
            }
            return account.AsDto();
        }

        // GET /accounts/{handle}
        [HttpGet("{handle}")]
        public async Task<ActionResult<AccountDto>> GetAccountByHandleNameAsync(string handle)
        {
            var account = await repository.GetAccountByHandleNameAsync(handle);
            if (account is null)
            {
                return NotFound();
            }
            return account.AsDto();
        }

        // Gets /accounts        
        [HttpGet]
        public async Task<IEnumerable<AccountDto>> GetAccountsByUserNameAsync(string userName = null)
        {
            var accounts = (await repository.GetAccountsAsync()).Select(account => account.AsDto());

            if (!string.IsNullOrWhiteSpace(userName))
            {
                accounts = accounts.Where(account => account.UserName.Contains(userName, StringComparison.OrdinalIgnoreCase));
            }

            logger.LogInformation($"{DateTime.UtcNow.ToString("hh:mm:ss")}: Retrieved {accounts.Count()} accounts");
            return accounts;
        }

        // Gets /accounts        
        [HttpGet]
        public async Task<IEnumerable<AccountDto>> GetAccountsAsync()
        {
            var accounts = (await repository.GetAccountsAsync()).Select(account => account.AsDto());            

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
            existingAccount.HandleName = accountDto.HandleName;

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
            existingAccount.HandleName = accountDto.HandleName;
            existingAccount.Password = accountDto.Password;
            existingAccount.MobileNumber = accountDto.MobileNumber;
            existingAccount.IdentificationType = accountDto.IdentificationType;
            existingAccount.IdentificationNumber = accountDto.IdentificationNumber;

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
