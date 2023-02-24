using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PinoyMassageService.Controllers.Services;
using PinoyMassageService.Entities;
using static PinoyMassageService.Dtos.ContactDtos;
using System.Security.Claims;
using System.Security.Cryptography;
using PinoyMassageService.Repositories;
using static PinoyMassageService.Dtos.AccountDtos;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using PinoyMassageService.Constant;
using static PinoyMassageService.Dtos.RefreshTokenDtos;
using Newtonsoft.Json.Linq;
using PinoyMassageService.Helpers;
using FirebaseAdmin.Auth;
using PinoyMassageService.ResponseObject;
using AutoMapper;
using Newtonsoft.Json;
using PinoyMassageService.Dtos;



namespace PinoyMassageService.Controllers
{
    [ApiController]
    [Route("[controller]/[action]")]
    // must be ContactController
    public class ContactController : Controller
    {
        //public static Contact contact = new Contact();
        private readonly IConfiguration _configuration;
        private readonly IContactService _contactService;

        private readonly IContactRepository _repository;
        private readonly IAccountRepository _accountRepository;
        private readonly IRefreshTokenRepository _refreshTokenRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<ContactController> _logger;


        public ContactController(
            IConfiguration configuration, IContactService contactService,
            IContactRepository repository, IAccountRepository accountRepository,
            IRefreshTokenRepository refreshTokenRepository, IMapper mapper,
            ILogger<ContactController> logger
            )
        {
            _configuration = configuration;
            _contactService = contactService;
            _repository = repository;
            _accountRepository = accountRepository;
            _refreshTokenRepository = refreshTokenRepository;
            _mapper = mapper;
            _logger = logger;
        }

        [HttpGet, Authorize(Roles = RoleType.User)]
        //[HttpGet, Authorize(Roles = UserType.Admin)]
        //[HttpGet, Authorize]
        //[HttpGet]
        public ActionResult<object> GetMe()
        {
            var userName = _contactService.GetMyName();
            var role = _contactService.GetRole();
            var nameIdentitifier = _contactService.GetNameIdentifier();

            return Ok(new { userName, role, nameIdentitifier });
        }


        [HttpPost("register")]
        public async Task<IActionResult> Register(CreateContactDto contactDto)
        {
            var exists = await _repository.GetContactByEmailAsync(contactDto.Email);
            if (exists != null)
            {
                return Conflict(new Response
                {
                    Status = ApiResponseType.Failed,
                    Message = "Contact with this email already exists.",
                    Data = new JObject()
                });
            }
            else
            {
                CreatePasswordHash(contactDto.Password, out byte[] passwordHash, out byte[] passwordSalt);

                Contact contact = new()
                {
                    Id = Guid.NewGuid(),
                    Username = contactDto.Email,
                    Email = contactDto.Email,
                    Password = contactDto.Password,
                    PasswordHash = passwordHash,
                    PasswordSalt = passwordSalt,
                    AccountType = contactDto.AccountType,
                    MobileNumber = contactDto.MobileNumber,
                    DisplayName = "",
                    FacebookId="",
                    GoogleId = "",
                    FirebaseId = "",
                    CreatedDate = DateTime.UtcNow
                };

                await _repository.CreateContactAsync(contact);
                await _accountRepository.CreateAccountAsync(new Account
                {
                    Id = Guid.NewGuid(),
                    UserId = contact.Id,
                    CreatedDate = DateTime.UtcNow
                });                

                return CreatedAtAction(nameof(GetContactAsync), new { id = contact.Id }, new Response
                {
                    Status = ApiResponseType.Success,
                    Message = "Contact created successfully.",
                    Data = _mapper.Map<Contact, ContactDto>(contact)
                });
            }            
        }        

        [HttpPost("registerExternal")]
        public async Task<IActionResult> Register(CreateContactExternalDto contactDto)
        {
            var foundContact = await _repository.GetContactByUserNameAsync(contactDto.MobileNumber);
            if (foundContact != null)
            {
                return Conflict(new Response
                {
                    Status = ApiResponseType.Failed,
                    Message = "Contact with this phone number already exists.",
                    Data = new JObject()
                });
            }

            Contact contact = new Contact
            {
                Id = Guid.NewGuid(),
                Username = contactDto.MobileNumber,
                MobileNumber = contactDto.MobileNumber,
                Email = contactDto.Email,
                DisplayName = contactDto.DisplayName,
                FirebaseId = contactDto.FirebaseId,
                AccountType = contactDto.AccountType,
                FacebookId = contactDto.FacebookId,
                GoogleId = contactDto.GoogleId,
                CreatedDate = DateTime.UtcNow
            };

            await _repository.CreateContactAsync(contact);
            await _accountRepository.CreateAccountAsync(new Account
            {
                Id = Guid.NewGuid(),
                UserId = contact.Id,                
                CreatedDate = DateTime.UtcNow
            });
            
            var dto = _mapper.Map<Contact, ContactDto>(contact);            
            return CreatedAtAction(nameof(GetContactAsync), new { id = contact.Id }, new Response
            {
                Status = ApiResponseType.Success,
                Message = "Contact created successfully.",
                Data = dto
            });
        }
        

        [HttpPost("login")]
        public async Task<ActionResult<string>> Login(LoginContactDto contactDto)
        {
            var contact = await _repository.GetContactByUserNameAsync(contactDto.UserName);
            if (contact != null)
            {
                if (!VerifyPasswordHash(contactDto.Password, contact.PasswordHash, contact.PasswordSalt))
                {
                    return BadRequest("Wrong password");
                }

                // creates token
                string token = CreateToken(contact);
                // create refresh token
                var refreshToken = GenerateRefreshToken();

                // the client needs to save the userid and refresh token locally 
                // we need this to auto login contact if the token is still valid , if not needs to login again
                // validity is 7 days for refresh token

                // save the refresh token info on db
                await UpdateRefreshTokenAsync(contact, refreshToken);                

                return Ok(new Response
                {
                    Status = ApiResponseType.Success,
                    Message = "login successfull.",
                    Data = new Token
                    {
                        Value = token,
                        RoleType = RoleType.User.ToString()
                    }
                });
            }
            else
            {
                return NotFound(new Response
                {
                    Status = ApiResponseType.Failed,
                    Message = $"Contact with username {contactDto.UserName} not found.",
                    Data = new Token
                    {
                        Value = "",
                        RoleType = RoleType.User.ToString()
                    }
                });
            }            
        }

        [HttpPost("loginExternal")]
        public async Task<ActionResult<string>> Login(LoginContactExternalDto contactDto)
        {
            _logger.LogInformation($"ContactControler: {DateTime.UtcNow.ToString("hh:mm:ss")} call loginExternal!");
            try
            {
                string uid = await FirebaseAuthService.Instance.GetUserUid(contactDto.IdTokenFromExternal, _logger);
                if (string.IsNullOrWhiteSpace(uid))
                {
                    _logger.LogInformation($"ContactControler: {DateTime.UtcNow.ToString("hh:mm:ss")} is null or empty Guid: {uid}");
                    return Unauthorized(new Response
                    {
                        Status = ApiResponseType.Failed,
                        Message = "Invalid id token",
                        Data = new Token
                        {
                            Value = "",
                            RoleType=RoleType.User.ToString()
                        }
                    });
                }
            }
            catch (FirebaseAuthException ex)
            {
                _logger.LogInformation($"ContactControler: {DateTime.UtcNow.ToString("hh:mm:ss")} FirebaseAuthException ex: {ex.Message}");
                return BadRequest(new Response
                {
                    Status = ApiResponseType.Failed,
                    Message = "Failed to verify id token.",
                    Data = new Token
                    {
                        Value = "",
                        RoleType = RoleType.User.ToString()
                    }
                });
            }


            Contact? contact = null;

            if (!String.IsNullOrEmpty(contactDto.UserName))
            {
                // this is also phone numer
                contact = await _repository.GetContactByUserNameAsync(contactDto.UserName);
            }            

            if (contact == null)
            {
                _logger.LogInformation($"ContactControler: {DateTime.UtcNow.ToString("hh:mm:ss")} contact not found 1st!");
                return NotFound(new Response
                {
                    Status = ApiResponseType.Failed,
                    Message = $"Contact with username {contactDto.UserName} not found.",
                    Data = new Token
                    {
                        Value = "",
                        RoleType = RoleType.User.ToString()
                    }
                });
            }            

            // creates token
            // this token needs to be save in contact phone , contact can then use it to call api without login
            // until token is still valid and not expired, if expired we tell the contact to login again manually
            string token = CreateToken(contact);
            // create refresh token
            var refreshToken = GenerateRefreshToken();

            // the client needs to save the userid and refresh token locally 
            // we need this to auto login contact if the token is still valid , if not needs to login again
            // validity is 7 days for refresh token

            // save the refresh token info on db
            await UpdateRefreshTokenAsync(contact, refreshToken);

            _logger.LogInformation($"ContactControler: {DateTime.UtcNow.ToString("hh:mm:ss")} got access token: {token}");

            return Ok(new Response
            {
                Status = ApiResponseType.Success,
                Message = "login successfull.",
                Data = new Token
                {
                    Value = token,
                    RoleType = RoleType.User.ToString()
                }
            });
        }

        // this will be called when the contact opens the app, if contact receive UnAuthorized meaning the contact needs to login again
        // using username/password or via fb or gmail
        // note that the app must check 1st if there's a save local data with username,userid and the refresh token
        // you need to have this data before calling this in the app
        [HttpPost("{userId}")]
        public async Task<ActionResult<string>> RefreshToken(Guid userId, RefreshTokenDto refreshTokenDto)
        {
            // get the contact info from db
            var existingRefreshToken = await _refreshTokenRepository.GetRefreshTokenByUserIdAsync(userId);

            if (existingRefreshToken != null)
            {
                if (!existingRefreshToken.Token.Equals(refreshTokenDto.Token))
                {
                    return Unauthorized($"Invalid Refresh Token. contact refresh token: {existingRefreshToken.Token} curren refresh token: {refreshTokenDto.Token}  expires {existingRefreshToken.TokenExpires} DateTime.Now: {DateTime.UtcNow}");
                }
                else if (existingRefreshToken.TokenExpires < DateTime.UtcNow)
                {
                    return Unauthorized("Refresh Token expired.");
                }

                var existingContact = await _repository.GetContactAsync(userId);
                if (existingContact != null)
                {
                    string token = CreateToken(existingContact);
                    var newRefreshToken = GenerateRefreshToken();

                    // the client needs to save the userid and refresh token locally 
                    // we need this to auto login contact if the token is still valid , if not needs to login again
                    // validity is 7 days for refresh token

                    // update the refresh token info on db
                    await UpdateRefreshTokenAsync(existingContact, newRefreshToken);
                    return Ok(token);
                }
                else
                {
                    return Unauthorized("Contact Not found.");
                }
            }
            else
            {
                return Unauthorized("Refresh Token Not found.");
            }
        }

        private RefreshToken GenerateRefreshToken()
        {
            var refreshToken = new RefreshToken
            {
                Token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64)),
                TokenExpires = DateTime.UtcNow.AddDays(7),
                TokenCreated = DateTime.UtcNow
            };

            return refreshToken;
        }

        private string CreateToken(Contact contact)
        {
            // in here you need to check contact role and give them the correct role claims
            // role can be regular contact, admin, tester, marketing, accounting  and etc.
            List<Claim> claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, contact.Id.ToString()),
                new Claim(ClaimTypes.Name, contact.Username),
                new Claim(ClaimTypes.Role, RoleType.User)
            };

            var key = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(
                _configuration.GetSection("AppSettings:Token").Value));

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);
            var token = new JwtSecurityToken(
                claims: claims,
                expires: DateTime.UtcNow.AddDays(1),
                signingCredentials: creds);

            var jwt = new JwtSecurityTokenHandler().WriteToken(token);

            return jwt;
        }

        private void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
        {
            using (var hmac = new HMACSHA512())
            {
                passwordSalt = hmac.Key;
                passwordHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
            }
        }

        private bool VerifyPasswordHash(string password, byte[] passwordHash, byte[] passwordSalt)
        {
            using (var hmac = new HMACSHA512(passwordSalt))
            {
                var computedHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
                return computedHash.SequenceEqual(passwordHash);
            }
        }        

        // GET /accounts/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<ContactDto>> GetContactAsync(Guid id)
        {
            var contact = await _repository.GetContactAsync(id);
            if (contact != null)
            {
                return Ok(new Response
                {
                    Status = ApiResponseType.Success,
                    Message = $"Get contact by id: {id} successful.",
                    Data = _mapper.Map<Contact, ContactDto>(contact)
                });
            }
            else
            {
                return NotFound(new Response
                {
                    Status = ApiResponseType.Failed,
                    Message = $"Get contact by id: {id} failed, contact not found.",
                    Data = new JObject()
                });
            }            
        }

        // GET /contacts/{username}
        [HttpGet("{username}")]
        public async Task<ActionResult<ContactDto>> GetContactByUserNameAsync(string username)
        {
            var contact = await _repository.GetContactByUserNameAsync(username);
            if (contact != null)
            {
                return Ok(new Response
                {
                    Status = ApiResponseType.Success,
                    Message = $"Get contact by UserName: {username} successful.",
                    Data = _mapper.Map<Contact, ContactDto>(contact)
                });
            }
            else
            {
                return NotFound(new Response
                {
                    Status = ApiResponseType.Failed,
                    Message = $"Get contact by UserName: {username} failed, contact not found.",
                    Data = new JObject()
                });
            }
        }        

        // GET /accounts/{email}
        [HttpGet("{email}")]
        public async Task<ActionResult<ContactDto>> GetContactByEmailAsync(string email)
        {
            var contact = await _repository.GetContactByEmailAsync(email);
            if (contact != null)
            {
                return Ok(new Response
                {
                    Status = ApiResponseType.Success,
                    Message = $"Get contact by email: {email} successful.",
                    Data = _mapper.Map<Contact, ContactDto>(contact)
                });
            }
            else
            {
                return NotFound(new Response
                {
                    Status = ApiResponseType.Failed,
                    Message = $"Get contact by email: {email} failed, contact not found.",
                    Data = new JObject()
                });
            }            
        }

        // GET /accounts/{mobilenumber}
        [HttpGet("{mobilenumber}")]
        public async Task<ActionResult<ContactDto>> GetContactByMobileNumberAsync(string mobilenumber)
        {
            var contact = await _repository.GetContactByMobileNumberAsync(mobilenumber);
            if (contact != null)
            {
                return Ok(new Response
                {
                    Status = ApiResponseType.Success,
                    Message = $"Get contact by mobilenumber: {mobilenumber} successful.",
                    Data = _mapper.Map<Contact, ContactDto>(contact)
                });
            }
            else
            {
                return NotFound(new Response
                {
                    Status = ApiResponseType.Failed,
                    Message = $"Get contact by mobilenumber: {mobilenumber} failed, contact not found.",
                    Data = new JObject()
                });
            }            
        }

        [HttpGet("{provider}/{providerId}")]
        public async Task<ActionResult<string>> GetContactMobileNumberByProviderAsync(string provider, string providerId)
        {
            var mobileNumber = await _repository.GetContactMobileNumberByProviderAsync(provider, providerId);
            if (string.IsNullOrEmpty(mobileNumber))
            {
                return NotFound(new Response
                {
                    Status = ApiResponseType.Failed,
                    Message = $"Contact with provider {provider} and providerId {providerId} was not found.",
                    Data = new PhoneNumberData
                    {
                        PhoneNumber = ""
                    }
                });
            }
            return Ok(new Response
            {
                Status = ApiResponseType.Success,
                Message = $"Contact with provider {provider} and providerId {providerId} was found.",
                Data = new PhoneNumberData
                {
                    PhoneNumber = mobileNumber
                }
            });
        }

        // Gets /accounts        
        //[HttpGet, Authorize]
        [HttpGet]        
        public async Task<IActionResult> GetContactsAsync()
        {            
            var contacts = (await _repository.GetContactsAsync()).Select(user => _mapper.Map<ContactDto>(user));

            _logger.LogInformation($"{DateTime.UtcNow.ToString("hh:mm:ss")}: GetAllAccountsAsync Retrieved {contacts.Count()} contacts");

            if (contacts.Count() > 0)
            {
                return Ok(new Response
                {
                    Status = ApiResponseType.Success,
                    Message = "Retrieved contacts successfully.",
                    Data = new ContactsData
                    {
                        Contacts = contacts.ToList()
                    }
                });
            }
            else
            {
                return Ok(new Response
                {
                    Status = ApiResponseType.Failed,
                    Message = "Retrieved contacts failed contacts is empty.",
                    Data = new JObject()
                });
            }
        }

        // Gets /contacts
        //[HttpGet,Authorize]        
        [HttpGet("{username}")]
        public async Task<IActionResult> GetContactsByUserNameAsync(string username = null)
        {
            if (string.IsNullOrEmpty(username))
            {
                return NotFound();
            }

            var contacts = (await _repository.GetContactsByUserNameAsync(username));
            var mappedContacts = contacts.Select(contact => _mapper.Map<ContactDto>(contact)).ToList();
            _logger.LogInformation($"{DateTime.UtcNow.ToString("hh:mm:ss")}: Retrieved {contacts.Count()} contacts");

            if (contacts.Count() > 0)
            {
                return Ok(new Response
                {
                    Status = ApiResponseType.Success,
                    Message = $"Retrieved contacts by username: {username} successfully.",
                    Data = new ContactsData
                    {
                        Contacts = mappedContacts.ToList()
                    }                    
                });
            }
            else
            {
                return Ok(new Response
                {
                    Status = ApiResponseType.Failed,
                    Message = $"Retrieved contacts by username: {username} failed contacts is empty.",
                    Data = new JObject()
                });
            }
        }

        [HttpGet("{mobilenumber}")]
        public async Task<ActionResult<string>> CheckContactMobileNumberAsync(string mobilenumber)
        {
            // TODO: needs to check if mobile number is in correct format

            var exists = await _repository.CheckContactMobileNumberAsync(mobilenumber);
            if (exists)
            {
                return Ok(new Response
                {
                    Status = ApiResponseType.Success,
                    Message = $"Contact with mobilenumber {mobilenumber} was found.",
                    Data = new PhoneNumberCheckData
                    {
                        Exists = true
                    }
                });                
            }
            else
            {
                return Ok(new Response
                {
                    Status = ApiResponseType.Success,
                    Message = $"Contact with mobilenumber {mobilenumber} was not found.",
                    Data = new PhoneNumberCheckData
                    {
                        Exists = false
                    }
                });
            }            
        }        

        // use in server side only
        private async Task UpdateRefreshTokenAsync(Contact contact, RefreshToken newRefreshToken)
        {
            if (contact != null)
            {
                var existingRefreshToken = await _refreshTokenRepository.GetRefreshTokenByUserIdAsync(contact.Id);
                if (existingRefreshToken != null)
                {
                    existingRefreshToken.Token = newRefreshToken.Token;
                    existingRefreshToken.TokenCreated = newRefreshToken.TokenCreated;
                    existingRefreshToken.TokenExpires = newRefreshToken.TokenExpires;

                    await _refreshTokenRepository.UpdateRefreshTokenAsync(existingRefreshToken);
                }
                else
                {
                    // not found create new refresh token entry related to this contact
                    await _refreshTokenRepository.CreateRefreshTokenAsync(new RefreshToken
                    {
                        Id = Guid.NewGuid(),
                        UserId = contact.Id,
                        Token = newRefreshToken.Token,
                        TokenCreated = newRefreshToken.TokenCreated,
                        TokenExpires = newRefreshToken.TokenExpires,
                        CreatedDate = DateTime.UtcNow
                    }); ;
                }
            }
        }

        // PUT /contacts/id
        [HttpPut("{id}")]
        //[Authorize(Roles = RoleType.Contact)]
        //[Authorize(Roles = $"{UserType.Contact},{UserType.Admin}")]        
        //[Authorize]
        public async Task<ActionResult> UpdatePasswordAsync(Guid id, UpdatePasswordDto UpdatePasswordDto)
        {
            var existingContact = await _repository.GetContactAsync(id);
            if (existingContact == null)
            {
                return NotFound(new Response
                {
                    Status = ApiResponseType.Failed,
                    Message = $"Contact with id: {id} was not found.",
                    Data = new JObject()
                });
            }

            CreatePasswordHash(UpdatePasswordDto.Password, out byte[] passwordHash, out byte[] passwordSalt);

            existingContact.Password = UpdatePasswordDto.Password;
            existingContact.PasswordHash = passwordHash;
            existingContact.PasswordSalt = passwordSalt;

            var result = await _repository.UpdateContactAsync(existingContact);
            if (result)
            {
                return Ok(new Response
                {
                    Status = ApiResponseType.Success,
                    Message = $"Contact with id: {id} update password to {UpdatePasswordDto.Password} successfully.",
                    Data = new JObject()
                });
            }
            return Conflict(new Response
            {
                Status = ApiResponseType.Failed,
                Message = $"Contact with id: {id} update password to {UpdatePasswordDto.Password} failed.",
                Data = new JObject()
            });
        }

        // PUT /accounts/id        
        [HttpPut("{id}")]
        public async Task<ActionResult> UpdateMobileNumberAsync(Guid id, UpdateMobileNumberDto UpdateMobileNumberDto)
        {
            var existingContact = await _repository.GetContactAsync(id);
            if (existingContact == null)
            {
                return NotFound(new Response
                {
                    Status = ApiResponseType.Failed,
                    Message = $"Contact with id: {id} was not found.",
                    Data = new JObject()
                });
            }

            existingContact.MobileNumber = UpdateMobileNumberDto.MobileNumber;

            var result = await _repository.UpdateContactAsync(existingContact);
            if (result)
            {
                return Ok(new Response
                {
                    Status = ApiResponseType.Success,
                    Message = $"Contact with id: {id} update phone number to {UpdateMobileNumberDto.MobileNumber} successfully.",
                    Data = new JObject()
                });
            }
            return Conflict(new Response
            {
                Status = ApiResponseType.Failed,
                Message = $"Contact with id: {id} update phone number to {UpdateMobileNumberDto.MobileNumber} failed.",
                Data = new JObject()
            });
        }

        // PUT /accounts/id        
        [HttpPut("{id}")]
        public async Task<ActionResult> UpdateEmailAsync(Guid id, UpdateEmailDto updateEmailDto)
        {
            var existingContact = await _repository.GetContactAsync(id);
            if (existingContact == null)
            {
                return NotFound(new Response
                {
                    Status = ApiResponseType.Failed,
                    Message = $"Contact with id: {id} was not found.",
                    Data = new JObject()
                });
            }

            existingContact.Email = updateEmailDto.Email;

            var result = await _repository.UpdateContactAsync(existingContact);
            if (result)
            {
                return Ok(new Response
                {
                    Status = ApiResponseType.Success,
                    Message = $"Contact with id: {id} update email to {updateEmailDto.Email} successfully.",
                    Data = new JObject()
                });
            }
            return Conflict(new Response
            {
                Status = ApiResponseType.Failed,
                Message = $"Contact with id: {id} update email to {updateEmailDto.Email} failed.",
                Data = new JObject()
            });            
        }

        [HttpPut("{mobileNumber}/{provider}/{providerId}")]
        public async Task<ActionResult> UpdateContactProviderIdAsync(string mobileNumber, string provider, string providerId)
        {
            var exists = await _repository.CheckContactMobileNumberAsync(mobileNumber);
            if (!exists)
            {
                return NotFound(new Response
                {
                    Status = ApiResponseType.Failed,
                    Message = $"Contact with mobile number {mobileNumber} was not found.",
                    Data = new JObject()
                });
            }
            else
            {
                var result = await _repository.UpdateContactProviderIdAsync(mobileNumber, provider,providerId);
                if (result)
                {
                    return Ok(new Response
                    {
                        Status = ApiResponseType.Success,
                        Message = $"Contact with mobile number {mobileNumber} has been updated with {provider} Contact ID {providerId}.",
                        Data = new JObject()
                    });
                }
                return Conflict(new Response
                {
                    Status = ApiResponseType.Failed,
                    Message = $"failed to update the {provider} Contact id with contact with mobileNumber {mobileNumber}.",
                    Data = new JObject()
                });
            }
        }
    }
}