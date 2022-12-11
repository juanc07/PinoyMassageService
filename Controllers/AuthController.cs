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

namespace PinoyMassageService.Controllers
{
    [ApiController]    
    [Route("[controller]/[action]")]
    public class AuthController : Controller
    {
        //public static User user = new User();
        private readonly IConfiguration _configuration;
        private readonly IUserService _userService;

        private readonly IUserRepository repository;
        private readonly ILogger<AuthController> logger;


        public AuthController(IConfiguration configuration, IUserService userService, IUserRepository repository, ILogger<AuthController> logger)
        {
            _configuration = configuration;
            _userService = userService;
            this.repository = repository;
            this.logger = logger;
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
            CreatePasswordHash(userDto.Password, out byte[] passwordHash, out byte[] passwordSalt);

            User user = new()
            {
                Id = Guid.NewGuid(),
                Username = userDto.UserName,
                Password = userDto.Password,
                PasswordHash = passwordHash,
                PasswordSalt = passwordSalt,
                TokenCreated = null,
                TokenExpires = null,
                CreatedDate = DateTime.UtcNow                
            };

            await repository.CreateUserAsync(user);
            return CreatedAtAction(nameof(GetUserAsync), new { id = user.Id }, user.AsDto());
        }

        // GET /accounts/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<UserDto>> GetUserAsync(Guid id)
        {
            var user = await repository.GetUserAsync(id);
            if (user is null)
            {
                return NotFound();
            }
            return user.AsDto();
        }

        [HttpPost("login")]
        public async Task<ActionResult<string>> Login(LoginUserDto userDto)
        {
            var user = await repository.GetUserByUserNameAsync(userDto.UserName);

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

        // this will be called when the user opens the app is user receive UnAuthorized meaning the user needs to login again
        // using username/password or via fb or gmail
        [HttpPost("{id}")]
        public async Task<ActionResult<string>> RefreshToken(Guid id, RefreshTokenDto refreshTokenDto)
        {
            // get the user info from db
            var user = await repository.GetUserAsync(id);            

            if (!user.RefreshToken.Equals(refreshTokenDto.refreshToken))
            {
                return Unauthorized($"Invalid Refresh Token. user refresh token: {user.RefreshToken} curren refresh token: {refreshTokenDto.refreshToken}  expires {user.TokenExpires} DateTime.Now: {DateTime.UtcNow}");
            }
            else if (user.TokenExpires < DateTime.UtcNow)
            {
                return Unauthorized("Refresh Token expired.");
            }

            string token = CreateToken(user);
            var newRefreshToken = GenerateRefreshToken();
            
            // the client needs to save the userid and refresh token locally 
            // we need this to auto login user if the token is still valid , if not needs to login again
            // validity is 7 days for refresh token

            // update the refresh token info on db
            await UpdateRefreshTokenAsync(user, newRefreshToken);

            return Ok(token);
        }


        private RefreshToken GenerateRefreshToken()
        {
            var refreshToken = new RefreshToken
            {
                Token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64)),
                Expires = DateTime.UtcNow.AddDays(7),
                Created = DateTime.UtcNow
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
            var existingUser = await repository.GetUserAsync(id);
            if (existingUser is null)
            {
                return NotFound();
            }

            CreatePasswordHash(UpdatePasswordDto.Password, out byte[] passwordHash, out byte[] passwordSalt);

            existingUser.Password = UpdatePasswordDto.Password;
            existingUser.PasswordHash = passwordHash;
            existingUser.PasswordSalt = passwordSalt;

            await repository.UpdateUserAsync(existingUser);
            return NoContent();
        }

        private async Task UpdateRefreshTokenAsync(User user,RefreshToken newRefreshToken)
        {            
            if (user != null)
            {
                user.RefreshToken = newRefreshToken.Token;
                user.TokenCreated = newRefreshToken.Created;
                user.TokenExpires = newRefreshToken.Expires;

                await repository.UpdateUserAsync(user);
            }            
        }

        // Gets /accounts        
        //[HttpGet, Authorize]
        [HttpGet]
        public async Task<IEnumerable<UserDto>> GetUsersAsync()
        {
            var users = (await repository.GetUsersAsync()).Select(user => user.AsDto());

            logger.LogInformation($"{DateTime.UtcNow.ToString("hh:mm:ss")}: GetAllAccountsAsync Retrieved {users.Count()} users");
            return users;
        }

        // GET /users/{username}
        [HttpGet("{username}")]
        public async Task<ActionResult<UserDto>> GetUserByUserNameAsync(string username)
        {
            var user = await repository.GetUserByUserNameAsync(username);
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
            var users = (await repository.GetUsersAsync()).Select(user => user.AsDto());

            if (!string.IsNullOrWhiteSpace(userName))
            {
                users = users.Where(user => user.UserName.Equals(userName, StringComparison.OrdinalIgnoreCase));
            }

            logger.LogInformation($"{DateTime.UtcNow.ToString("hh:mm:ss")}: Retrieved {users.Count()} users");
            return users;
        }
    }
}
