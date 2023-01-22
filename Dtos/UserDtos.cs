﻿using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;

namespace PinoyMassageService.Dtos
{
    public class UserDtos
    {
        /*public record UserDto(Guid Id,  string UserName, string Password, byte[] PasswordHash, byte[] PasswordSalt, int AccountType,
            string Email, string MobileNumber,string DisplayName, string FacebookId,string GoogleId,string FirebaseId, DateTimeOffset CreatedDate);*/

        public record UserDto(Guid Id, string UserName, int AccountType,
            string Email, string MobileNumber, string DisplayName, string FacebookId, string GoogleId, string FirebaseId, DateTimeOffset CreatedDate);

        public record UserExternalDto(Guid Id, string UserName,int AccountType, string Email, string MobileNumber, string DisplayName, 
            string FacebookId, string GoogleId, string FirebaseId, DateTimeOffset CreatedDate);
        public record CreateUserDto([Required] string Email, [Required] string Password, int AccountType, string MobileNumber, string DisplayName,
            string FirebaseId, string FacebookId, string GoogleId);        
        public record CreateUserExternalDto([Required] string MobileNumber, [Required] string Email, [Required] string DisplayName,
            string FirebaseId, string FacebookId, string GoogleId, int AccountType);
        public record CreateAdminDto([Required] string UserName, [Required] string Password);
        public record LoginUserDto([Required] string UserName, [Required] string Password);
        public record LoginUserExternalDto(string UserName, [Required] string IdTokenFromExternal,string Email);
        public record UpdatePasswordDto([Required]  string Password);        

        // contact info
        public record UpdateMobileNumberDto(string MobileNumber);
        public record UpdateEmailDto(string Email);
    }
}
