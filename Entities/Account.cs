using System.ComponentModel.DataAnnotations;

namespace PinoyMassageService.Entities
{
    public class Account
    {
        // account base
        public Guid Id { get; set; }
        // will become array of contacts
        public Guid UserId { get; set; }
        // need to add this CompanyName, Company Address, Company Business certifications or permit if required, DTI, Tax Identification ID etc

        // to be remove chunk
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public int Gender { get; set; }
        public long BirthDate { get; set; }
        public int Age { get; set; }
        public Address? Address { get; set; }

        // identification info
        public string IdentificationType { get; set; }
        public string IdentificationNumber { get; set; }
        // to be remove chunk

        // if identification is verified or not
        public bool IsVerified { get; set; }

        public DateTimeOffset CreatedDate { get; set; }
    }
}
