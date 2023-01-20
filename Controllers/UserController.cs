using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PinoyMassageService.Controllers.Services;
using PinoyMassageService.Entities;
using static PinoyMassageService.Dtos.UserDtos;
using System.Security.Claims;
using System.Security.Cryptography;
using PinoyMassageService.Repositories;
using PinoyMassageService.Extensions;
using static PinoyMassageService.Dtos.AccountDtos;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using PinoyMassageService.Constant;
using static PinoyMassageService.Dtos.RefreshTokenDtos;
using Newtonsoft.Json.Linq;
using Google.Apis.Auth.OAuth2;
using PinoyMassageService.Helpers;
using FirebaseAdmin.Auth;
using PinoyMassageService.ResponseObject;
using System.Text.Json.Nodes;
using Microsoft.Extensions.Logging;

namespace PinoyMassageService.Controllers
{
    [ApiController]
    [Route("[controller]/[action]")]
    public class UserController : Controller
    {
        //public static User user = new User();
        private readonly IConfiguration _configuration;
        private readonly IUserService _userService;

        private readonly IUserRepository _repository;
        private readonly IAccountRepository _accountRepository;
        private readonly IRefreshTokenRepository _refreshTokenRepository;
        private readonly ILogger<UserController> _logger;


        public UserController(
            IConfiguration configuration, IUserService userService,
            IUserRepository repository, IAccountRepository accountRepository,
            IRefreshTokenRepository refreshTokenRepository, ILogger<UserController> logger
            )
        {
            _configuration = configuration;
            _userService = userService;
            _repository = repository;
            _accountRepository = accountRepository;
            _refreshTokenRepository = refreshTokenRepository;
            _logger = logger;
        }

        [HttpGet, Authorize(Roles = RoleType.User)]
        //[HttpGet, Authorize(Roles = UserType.Admin)]
        //[HttpGet, Authorize]
        //[HttpGet]
        public ActionResult<object> GetMe()
        {
            var userName = _userService.GetMyName();
            var role = _userService.GetRole();
            var nameIdentitifier = _userService.GetNameIdentifier();

            return Ok(new { userName, role, nameIdentitifier });
        }


        [HttpPost("register")]
        public async Task<ActionResult<AccountDto>> Register(CreateUserDto userDto)
        {
            var foundUser = await _repository.GetUserByUserNameAsync(userDto.Email);
            if (foundUser == null)
            {
                CreatePasswordHash(userDto.Password, out byte[] passwordHash, out byte[] passwordSalt);

                User user = new()
                {
                    Id = Guid.NewGuid(),
                    Username = userDto.Email,
                    Email = userDto.Email,
                    Password = userDto.Password,
                    PasswordHash = passwordHash,
                    PasswordSalt = passwordSalt,
                    AccountType = userDto.AccountType,
                    MobileNumber = userDto.MobileNumber,
                    CreatedDate = DateTime.UtcNow
                };

                await _repository.CreateUserAsync(user);
                await _accountRepository.CreateAccountAsync(new Account
                {
                    Id = Guid.NewGuid(),
                    UserId = user.Id,
                    CreatedDate = DateTime.UtcNow
                });

                return CreatedAtAction(nameof(GetUserAsync), new { id = user.Id }, user.AsDto());
            }
            return BadRequest("User already exists!");
        }

        /*[HttpPost("registerExternal")]
        public async Task<ActionResult<AccountDto>> Register(CreateUserExternalDto userDto)
        {
            var foundUser = await _repository.GetUserByUserNameAsync(userDto.Email);
            Response? response = null;
            if (foundUser == null)
            {

                User user = new()
                {
                    Id = Guid.NewGuid(),
                    Username = userDto.Email,
                    MobileNumber = userDto.MobileNumber,
                    Email = userDto.Email,
                    DisplayName = userDto.DisplayName,
                    FirebaseId = userDto.FirebaseId,
                    AccountType = userDto.AccountType,
                    FacebookId = userDto.FacebookId,
                    GoogleId = userDto.GoogleId,
                    CreatedDate = DateTime.UtcNow
                };

                await _repository.CreateUserAsync(user);
                await _accountRepository.CreateAccountAsync(new Account
                {
                    Id = Guid.NewGuid(),
                    UserId = user.Id,
                    FirstName = userDto.FirstName,
                    LastName = userDto.LastName,
                    BirthDate = userDto.BirthDate,
                    Gender = userDto.Gender,
                    CreatedDate = DateTime.UtcNow
                });

                var dto = user.AsDto();
                response = new Response
                {
                    Status = ApiResponseType.Success,
                    Message = "User created successfully.",
                    Data = dto
                };
                return CreatedAtAction(nameof(GetUserAsync), new { id = user.Id }, response);
                //return CreatedAtAction(nameof(GetUserAsync), new { id = user.Id }, user.AsDto());
            }

            response = new Response
            {
                Status = ApiResponseType.Failed,
                Message = "FB or Google user already exists!",
                Data = new JObject()
            };
            return Ok(response);
            //return BadRequest(response);
        }*/

        [HttpPost("registerExternal")]
        public async Task<IActionResult> Register(CreateUserExternalDto userDto)
        {
            var foundUser = await _repository.GetUserByUserNameAsync(userDto.Email);
            if (foundUser != null)
            {
                return Conflict(new Response
                {
                    Status = ApiResponseType.Failed,
                    Message = "User with this email already exists.",
                    Data = new JObject()
                });
            }

            User user = new User
            {
                Id = Guid.NewGuid(),
                Username = userDto.MobileNumber,
                MobileNumber = userDto.MobileNumber,
                Email = userDto.Email,
                DisplayName = userDto.DisplayName,
                FirebaseId = userDto.FirebaseId,
                AccountType = userDto.AccountType,
                FacebookId = userDto.FacebookId,
                GoogleId = userDto.GoogleId,
                CreatedDate = DateTime.UtcNow
            };

            await _repository.CreateUserAsync(user);
            await _accountRepository.CreateAccountAsync(new Account
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,                
                CreatedDate = DateTime.UtcNow
            });

            var dto = user.AsDto();
            return CreatedAtAction(nameof(GetUserAsync), new { id = user.Id }, new Response
            {
                Status = ApiResponseType.Success,
                Message = "User created successfully.",
                Data = dto
            });
        }


        // GET /accounts/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<UserDto>> GetUserAsync(Guid id)
        {
            var user = await _repository.GetUserAsync(id);
            if (user is null)
            {
                return NotFound();
            }
            return user.AsDto();
        }

        [HttpPost("login")]
        public async Task<ActionResult<string>> Login(LoginUserDto userDto)
        {
            var user = await _repository.GetUserByUserNameAsync(userDto.UserName);

            if (user != null)
            {
                if (!user.Username.Equals(userDto.UserName, StringComparison.Ordinal))
                {
                    return BadRequest("User not found");
                }

                if (!VerifyPasswordHash(userDto.Password, user.PasswordHash, user.PasswordSalt))
                {
                    return BadRequest("Wrong password");
                }

                // creates token
                string token = CreateToken(user);
                // create refresh token
                var refreshToken = GenerateRefreshToken();

                // the client needs to save the userid and refresh token locally 
                // we need this to auto login user if the token is still valid , if not needs to login again
                // validity is 7 days for refresh token

                // save the refresh token info on db
                await UpdateRefreshTokenAsync(user, refreshToken);


                JObject jsonObject = new JObject();
                jsonObject["token"] = token;

                return Ok(new Response
                {
                    Status = ApiResponseType.Success,
                    Message = "login successfull.",
                    Data = jsonObject
                });
            }
            return BadRequest("User not found");
        }

        [HttpPost("loginExternal")]
        public async Task<ActionResult<string>> Login(LoginUserExternalDto userDto)
        {
            _logger.LogInformation($"UserControler: {DateTime.UtcNow.ToString("hh:mm:ss")} call loginExternal!");
            string uid = null;
            try
            {
                uid = await FirebaseAuthService.Instance.GetUserUid(userDto.IdTokenFromExternal, _logger);
                if (string.IsNullOrWhiteSpace(uid))
                {
                    _logger.LogInformation($"UserControler: {DateTime.UtcNow.ToString("hh:mm:ss")} is null or empty Guid: {uid}");
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
                _logger.LogInformation($"UserControler: {DateTime.UtcNow.ToString("hh:mm:ss")} FirebaseAuthException ex: {ex.Message}");
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


            User? user = null;

            if (!String.IsNullOrEmpty(userDto.UserName))
            {
                // this is also phone numer
                user = await _repository.GetUserByUserNameAsync(userDto.UserName);
            }
            else
            {
                // so this is redundant should we pick email instead?
                // or not phone number no registration?
                //user = await _repository.GetUserByMobileNumberAsync(userDto.email);             
            }

            if (user == null)
            {
                _logger.LogInformation($"UserControler: {DateTime.UtcNow.ToString("hh:mm:ss")} user not found 1st!");
                return NotFound(new Response
                {
                    Status = ApiResponseType.Failed,
                    Message = $"User with username {userDto.UserName} not found.",
                    Data = new Token
                    {
                        Value = "",
                        RoleType = RoleType.User.ToString()
                    }
                });
            }            

            // creates token
            // this token needs to be save in user phone , user can then use it to call api without login
            // until token is still valid and not expired, if expired we tell the user to login again manually
            string token = CreateToken(user);
            // create refresh token
            var refreshToken = GenerateRefreshToken();

            // the client needs to save the userid and refresh token locally 
            // we need this to auto login user if the token is still valid , if not needs to login again
            // validity is 7 days for refresh token

            // save the refresh token info on db
            await UpdateRefreshTokenAsync(user, refreshToken);

            _logger.LogInformation($"UserControler: {DateTime.UtcNow.ToString("hh:mm:ss")} got access token: {token}");

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

        // this will be called when the user opens the app, if user receive UnAuthorized meaning the user needs to login again
        // using username/password or via fb or gmail
        // note that the app must check 1st if there's a save local data with username,userid and the refresh token
        // you need to have this data before calling this in the app
        [HttpPost("{userId}")]
        public async Task<ActionResult<string>> RefreshToken(Guid userId, RefreshTokenDto refreshTokenDto)
        {
            // get the user info from db
            var existingRefreshToken = await _refreshTokenRepository.GetRefreshTokenByUserIdAsync(userId);

            if (existingRefreshToken != null)
            {
                if (!existingRefreshToken.Token.Equals(refreshTokenDto.Token))
                {
                    return Unauthorized($"Invalid Refresh Token. user refresh token: {existingRefreshToken.Token} curren refresh token: {refreshTokenDto.Token}  expires {existingRefreshToken.TokenExpires} DateTime.Now: {DateTime.UtcNow}");
                }
                else if (existingRefreshToken.TokenExpires < DateTime.UtcNow)
                {
                    return Unauthorized("Refresh Token expired.");
                }

                var existingUser = await _repository.GetUserAsync(userId);
                if (existingUser != null)
                {
                    string token = CreateToken(existingUser);
                    var newRefreshToken = GenerateRefreshToken();

                    // the client needs to save the userid and refresh token locally 
                    // we need this to auto login user if the token is still valid , if not needs to login again
                    // validity is 7 days for refresh token

                    // update the refresh token info on db
                    await UpdateRefreshTokenAsync(existingUser, newRefreshToken);
                    return Ok(token);
                }
                else
                {
                    return Unauthorized("User Not found.");
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

        private string CreateToken(User user)
        {
            // in here you need to check user role and give them the correct role claims
            // role can be regular user, admin, tester, marketing, accounting  and etc.
            List<Claim> claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Username),
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

        // PUT /users/id
        [HttpPut("{id}")]
        //[Authorize(Roles = $"{UserType.User},{UserType.Admin}")]        
        [Authorize]
        public async Task<ActionResult> UpdatePasswordAsync(Guid id, UpdatePasswordDto UpdatePasswordDto)
        {
            var existingUser = await _repository.GetUserAsync(id);
            if (existingUser is null)
            {
                return NotFound();
            }

            CreatePasswordHash(UpdatePasswordDto.Password, out byte[] passwordHash, out byte[] passwordSalt);

            existingUser.Password = UpdatePasswordDto.Password;
            existingUser.PasswordHash = passwordHash;
            existingUser.PasswordSalt = passwordSalt;

            await _repository.UpdateUserAsync(existingUser);
            return NoContent();
        }

        private async Task UpdateRefreshTokenAsync(User user, RefreshToken newRefreshToken)
        {
            if (user != null)
            {
                var existingRefreshToken = await _refreshTokenRepository.GetRefreshTokenByUserIdAsync(user.Id);
                if (existingRefreshToken != null)
                {
                    existingRefreshToken.Token = newRefreshToken.Token;
                    existingRefreshToken.TokenCreated = newRefreshToken.TokenCreated;
                    existingRefreshToken.TokenExpires = newRefreshToken.TokenExpires;

                    await _refreshTokenRepository.UpdateRefreshTokenAsync(existingRefreshToken);
                }
                else
                {
                    // not found create new refresh token entry related to this user
                    await _refreshTokenRepository.CreateRefreshTokenAsync(new RefreshToken
                    {
                        Id = Guid.NewGuid(),
                        UserId = user.Id,
                        Token = newRefreshToken.Token,
                        TokenCreated = newRefreshToken.TokenCreated,
                        TokenExpires = newRefreshToken.TokenExpires,
                        CreatedDate = DateTime.UtcNow
                    }); ;
                }
            }
        }

        // Gets /accounts        
        //[HttpGet, Authorize]
        [HttpGet]
        public async Task<IEnumerable<UserDto>> GetUsersAsync()
        {
            var users = (await _repository.GetUsersAsync()).Select(user => user.AsDto());

            _logger.LogInformation($"{DateTime.UtcNow.ToString("hh:mm:ss")}: GetAllAccountsAsync Retrieved {users.Count()} users");
            return users;
        }

        // GET /users/{username}
        [HttpGet("{username}")]
        public async Task<ActionResult<UserDto>> GetUserByUserNameAsync(string username)
        {
            var user = await _repository.GetUserByUserNameAsync(username);
            if (user is null)
            {
                return NotFound();
            }
            return user.AsDto();
        }

        // Gets /users        
        //[HttpGet,Authorize]
        [HttpGet]
        public async Task<IEnumerable<UserDto>> GetUsersByUserNameAsync(string userName = null)
        {
            var users = (await _repository.GetUsersAsync()).Select(user => user.AsDto());

            if (!string.IsNullOrWhiteSpace(userName))
            {
                users = users.Where(user => user.UserName.Equals(userName, StringComparison.OrdinalIgnoreCase));
            }

            _logger.LogInformation($"{DateTime.UtcNow.ToString("hh:mm:ss")}: Retrieved {users.Count()} users");
            return users;
        }

        // GET /accounts/{email}
        [HttpGet("{email}")]
        public async Task<ActionResult<UserDto>> GetUserByEmailAsync(string email)
        {
            var user = await _repository.GetUserByEmailAsync(email);
            if (user is null)
            {
                return NotFound();
            }
            return user.AsDto();
        }

        // GET /accounts/{mobilenumber}
        [HttpGet("{mobilenumber}")]
        public async Task<ActionResult<UserDto>> GetUserByMobileNumberAsync(string mobilenumber)
        {
            var user = await _repository.GetUserByMobileNumberAsync(mobilenumber);
            if (user is null)
            {
                return NotFound();
            }
            return user.AsDto();
        }

        [HttpGet("{provider}/{providerId}")]
        public async Task<ActionResult<string>> GetUserMobileNumberByProviderAsync(string provider, string providerId)
        {
            var mobileNumber = await _repository.GetUserMobileNumberByProviderAsync(provider, providerId);
            if ( string.IsNullOrEmpty(mobileNumber))
            {
                return Ok(new Response
                {
                    Status = ApiResponseType.Failed,
                    Message = $"User with provider {provider} and providerId {providerId} was not found.",
                    Data = new PhoneNumberData
                    {
                        PhoneNumber = ""
                    }
                });
            }
            return Ok(new Response
            {
                Status = ApiResponseType.Success,
                Message = $"User with provider {provider} and providerId {providerId} was found.",
                Data = new PhoneNumberData
                {
                    PhoneNumber = mobileNumber
                }
            });
        }

        [HttpGet("{mobilenumber}")]
        public async Task<ActionResult<string>> CheckUserMobileNumberAsync(string mobilenumber)
        {
            // TODO: needs to check if mobile number is in correct format

            var exists = await _repository.CheckUserMobileNumberAsync(mobilenumber);
            if (exists)
            {
                return Ok(new Response
                {
                    Status = ApiResponseType.Success,
                    Message = $"User with mobilenumber {mobilenumber} was found.",
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
                    Message = $"User with mobilenumber {mobilenumber} was not found.",
                    Data = new PhoneNumberCheckData
                    {
                        Exists = false
                    }
                });
            }            
        }

        // PUT /accounts/id        
        [HttpPut("{id}")]
        public async Task<ActionResult> UpdateMobileNumberAsync(Guid id, UpdateMobileNumberDto UpdateMobileNumberDto)
        {
            var existingUser = await _repository.GetUserAsync(id);
            if (existingUser is null)
            {
                return NotFound();
            }

            existingUser.MobileNumber = UpdateMobileNumberDto.MobileNumber;

            await _repository.UpdateUserAsync(existingUser);
            return NoContent();
        }

        // PUT /accounts/id        
        [HttpPut("{id}")]
        public async Task<ActionResult> UpdateEmailAsync(Guid id, UpdateEmailDto updateEmailDto)
        {
            var existingUser = await _repository.GetUserAsync(id);
            if (existingUser is null)
            {
                return NotFound();
            }

            existingUser.Email = updateEmailDto.Email;

            await _repository.UpdateUserAsync(existingUser);
            return NoContent();
        }

        [HttpPut("{mobileNumber}/{provider}/{providerId}")]
        public async Task<ActionResult> UpdateUserProviderIdAsync(string mobileNumber, string provider, string providerId)
        {
            var exists = await _repository.CheckUserMobileNumberAsync(mobileNumber);
            if (!exists)
            {
                return NotFound(new Response
                {
                    Status = "failed",
                    Message = $"User with mobile number {mobileNumber} was not found.",
                    Data = new JObject()
                });
            }
            else
            {
                var result = await _repository.UpdateUserProviderIdAsync(mobileNumber, provider,providerId);
                if (result)
                {
                    return Ok(new Response
                    {
                        Status = "success",
                        Message = $"User with mobile number {mobileNumber} has been updated with {provider} User ID {providerId}.",
                        Data = new JObject()
                    });
                }
                return Conflict(new Response
                {
                    Status = "failed",
                    Message = $"failed to update the {provider} User id with user with mobileNumber {mobileNumber}.",
                    Data = new JObject()
                });
            }
        }
    }
}
