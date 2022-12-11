using System.ComponentModel.DataAnnotations;

namespace PinoyMassageService.Entities
{
    public class Account
    {
        // account base
        public Guid Id { get; set; }
        public Guid UserId { get; set; }        
        public string Email { get; set; }        
        public int AccountType { get; set; }
        public DateTimeOffset BirthDate { get; set; }
        public int Gender { get; set; }
        public DateTimeOffset CreatedDate { get; set; }

        // basic info
        public string HandleName { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }

        // extra info                
        [DataType(DataType.PhoneNumber)]
        public string MobileNumber { get; set; }        
        public string FacebookId { get; set; }
        public int Age { get; set; }
        public Address? Address { get; set; }


        // identification info
        public string IdentificationType { get; set; }
        public string IdentificationNumber { get; set; }

        // if identification is verified or not
        public bool IsVerified { get; set; }
    }
}
