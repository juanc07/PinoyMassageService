namespace PinoyMassageService.Entities
{
    public class Review
    {
        public Guid Id { get; set; }
        // the one that being reviewed
        public Guid reviewId { get; set; }
        // the reviewer
        public Guid reviewerId { get; set; }
        public string review { get; set; }
        public int rating { get; set; }
        public DateTimeOffset created { get; set; }
    }
}
