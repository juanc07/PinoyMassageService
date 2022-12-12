﻿using Microsoft.AspNetCore.Authorization;
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
            IConfiguration configuration,IUserService userService,
            IUserRepository repository,IAccountRepository accountRepository,
            IRefreshTokenRepository refreshTokenRepository,ILogger<UserController> logger
            )
        {
            _configuration = configuration;
            _userService = userService;
            _repository = repository;
            _accountRepository = accountRepository;
            _refreshTokenRepository = refreshTokenRepository;
            _logger = logger;
        }

        //[HttpGet, Authorize(Roles = UserType.Admin)]
        [HttpGet, Authorize]
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
            if(foundUser == null)
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

        [HttpPost("registerExternal")]
        public async Task<ActionResult<AccountDto>> Register(CreateUserExternalDto userDto)
        {
            var foundUser = await _repository.GetUserByUserNameAsync(userDto.Email);
            if (foundUser == null)
            {
                User user = new()
                {
                    Id = Guid.NewGuid(),
                    Username = userDto.Email,
                    Email = userDto.Email,                    
                    AccountType = userDto.AccountType,                    
                    FacebookId= userDto.FacebookId,
                    GoogleId  = userDto.GoogleId,   
                    CreatedDate = DateTime.UtcNow                    
                };

                await _repository.CreateUserAsync(user);
                await _accountRepository.CreateAccountAsync( new Account
                {
                    Id = Guid.NewGuid(),
                    UserId = user.Id,
                    FirstName = userDto.FirstName,
                    LastName = userDto.LastName,
                    BirthDate= userDto.BirthDate,
                    Gender = userDto.Gender,
                    CreatedDate = DateTime.UtcNow
                } );

                return CreatedAtAction(nameof(GetUserAsync), new { id = user.Id }, user.AsDto());
            }
            return BadRequest("FB or Google user already exists!");
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

            if(user != null)
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

                return Ok(token);
            }
            return BadRequest("User not found");
        }

        [HttpPost("loginExternal")]
        public async Task<ActionResult<string>> Login(LoginUserExternalDto userDto)
        {
            var user = await _repository.GetUserByUserNameAsync(userDto.UserName);

            if (user != null)
            {
                if (!user.Username.Equals(userDto.UserName, StringComparison.Ordinal))
                {
                    return BadRequest("User not found");
                }                

                // not sure yet what to do with external access key 
                // save it probably to access fb data info of user?

                // creates token
                string token = CreateToken(user);
                // create refresh token
                var refreshToken = GenerateRefreshToken();

                // the client needs to save the userid and refresh token locally 
                // we need this to auto login user if the token is still valid , if not needs to login again
                // validity is 7 days for refresh token

                // save the refresh token info on db
                await UpdateRefreshTokenAsync(user, refreshToken);

                return Ok(token);
            }
            return BadRequest("User not found");
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

            if (existingRefreshToken!=null)
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
                new Claim(ClaimTypes.Role, UserType.User)
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
        public async Task<ActionResult<UserDto>> GetAccountByEmailAsync(string email)
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
        public async Task<ActionResult<UserDto>> GetAccountByMobileNumberAsync(string mobilenumber)
        {
            var user = await _repository.GetUserByMobileNumberAsync(mobilenumber);
            if (user is null)
            {
                return NotFound();
            }
            return user.AsDto();
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
    }
}
