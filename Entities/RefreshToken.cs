namespace PinoyMassageService.Entities
{
    public class RefreshToken
    {
        // add an id and userid here and store this information in database
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string Token { get; set; } = string.Empty;
        public DateTime Created { get; set; } = DateTime.UtcNow;
        public DateTime Expires { get; set; }
    }
}
