using System.ComponentModel.DataAnnotations;

namespace PinoyMassageService.Entities
{
    public class User
    {
        public Guid Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public byte[] PasswordHash { get; set; }
        public byte[] PasswordSalt { get; set; }

        // tokens
        //public string? RefreshToken { get; set; } = string.Empty;
        //public DateTime? TokenCreated { get; set; }
        //public DateTime? TokenExpires { get; set; }

        // verification data        
        public string Email { get; set; }
        [DataType(DataType.PhoneNumber)]
        public string MobileNumber { get; set; }
        public string FacebookId { get; set; }
        public string GoogleId { get; set; }

        // account type
        public int AccountType { get; set; }

        // time this user created
        public DateTimeOffset CreatedDate { get; set; }
    }
}
