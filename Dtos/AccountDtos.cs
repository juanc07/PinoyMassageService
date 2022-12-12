﻿using PinoyMassageService.Entities;
using System.ComponentModel.DataAnnotations;

namespace PinoyMassageService.Dtos
{
    public class AccountDtos
    {
        public record AccountDto(Guid Id, Guid UserId,string FirstName, string LastName, string HandleName, int Gender,
            DateTimeOffset BirthDate, int Age,string IdentificationType, string IdentificationNumber, bool IsVerified, DateTimeOffset CreatedDate);
        public record CreateAccountDto([Required] Guid UserId, DateTimeOffset CreatedDate);

        // Basic information        
        public record UpdateBasicDto( string FirstName, string LastName, string HandleName);                

        // identification info
        public record UpdateIdentificationDto(string IdentificationType, string IdentificationNumber);        

        // all mutable account info
        public record UpdateAccountDto(string FirstName, string LastName, string HandleName,
            string IdentificationType, string IdentificationNumber);
    }
}
