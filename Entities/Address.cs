namespace PinoyMassageService.Entities
{
    public class Address
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string StreetNumber { get; set; }
        public string Branggay { get; set; }
        public string City { get; set; }        
        public string Country { get; set; }
        public string ZipCode { get; set; }
        public DateTimeOffset CreatedDate { get; set; }
    }
}
