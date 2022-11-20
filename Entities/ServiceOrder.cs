namespace PinoyMassageService.Entities
{
    public class ServiceOrder
    {
        public Guid Id { get; set; }
        public Guid ClientId { get; set; }
        public Guid ProviderId { get; set; }
        public string AvailService { get; set; }
        public int ServiceDuration { get; set; }
        public int ServicePrice { get; set; }
        public string MeetUpLocation { get; set; }
        public int Status { get; set; }
        public DateTimeOffset ExpiredAt { get; set; }
        public DateTimeOffset CreatedDate { get; set; }
    }
}
