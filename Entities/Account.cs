using System.ComponentModel.DataAnnotations;

namespace PinoyMassageService.Entities
{
    public class Account
    {
        public Guid Id { get; set; }        
        public string UserName { get; set; }
        public string Password { get; set; }
        public string Email { get; set; }        
        public int AccountType { get; set; }        
        public string ProfileName { get; set; }
        public DateTimeOffset BirthDate { get; set; }
        public int Age { get; set; }
        [DataType(DataType.PhoneNumber)]
        public string MobileNumber { get; set; }
        public bool IsVerified { get; set; }
        public string FacebookId { get; set; }        
        public DateTimeOffset CreatedDate { get; set; }
    }
}
