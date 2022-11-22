namespace PinoyMassageService.Entities
{
    public class Service
    {
        public Guid Id { get; set; }        
        public Guid ProviderId { get; set; }
        // this is empty until some client avail it
        public Guid ClientId { get; set; }

        // Massage type or types
        public string ServiceOffer { get; set; }
        public int ServicePrice { get; set; }

        // will be zero if the client is a subscriber
        // we only take money from client , the provider will be free at the beginning until we decide that we can ask them
        public int CreditCost { get; set; }

        public int Duration { get; set; }        
        public string MeetUpLocation { get; set; }

        // Pending, Accepted, Completed,Canceled, Expired
        public int Status { get; set; }
        public DateTimeOffset ExpiredAt { get; set; }
        public DateTimeOffset CreatedDate { get; set; }
    }
}
