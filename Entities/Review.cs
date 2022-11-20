namespace PinoyMassageService.Entities
{
    public class Review
    {
        public Guid Id { get; set; }
        // the one that being reviewed
        public Guid ReviewId { get; set; }
        // the reviewer
        public Guid ReviewerId { get; set; }
        public string Comment { get; set; }
        public int Rating { get; set; }
        public DateTimeOffset CreatedDate { get; set; }
    }
}
