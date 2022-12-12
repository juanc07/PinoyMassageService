using System.ComponentModel.DataAnnotations;

namespace PinoyMassageService.Entities
{
    public class Account
    {
        // account base
        public Guid Id { get; set; }
        public Guid UserId { get; set; }                
        public string HandleName { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public int Gender { get; set; }
        public DateTimeOffset BirthDate { get; set; }
        public int Age { get; set; }
        public Address? Address { get; set; }

        // identification info
        public string IdentificationType { get; set; }
        public string IdentificationNumber { get; set; }

        // if identification is verified or not
        public bool IsVerified { get; set; }

        public DateTimeOffset CreatedDate { get; set; }
    }
}
