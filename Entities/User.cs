using System.ComponentModel.DataAnnotations;

namespace PinoyMassageService.Entities
{
    // will become Contact class
    public class User
    {
        public Guid Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public byte[] PasswordHash { get; set; }
        public byte[] PasswordSalt { get; set; }        

        // verification data        
        public string Email { get; set; }
        [DataType(DataType.PhoneNumber)]
        public string MobileNumber { get; set; }
        public string DisplayName { get; set; }
        // use firebaseid of phone not fb or google
        public string FirebaseId { get; set; }
        public string FacebookId { get; set; }
        public string GoogleId { get; set; }

        // upcoming general info
        // public string FirstName { get; set; }
        // public string LastName { get; set; }
        // public int Gender { get; set; }
        // public long BirthDate { get; set; }
        // public int Age { get; set; }
        // identification info
        // public string IdentificationType { get; set; }
        // public string IdentificationNumber { get; set; }
        // upcoming general info

        // account type
        public int AccountType { get; set; }

        // time this user created
        public DateTimeOffset CreatedDate { get; set; }
    }
}
