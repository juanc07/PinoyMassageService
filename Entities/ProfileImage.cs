namespace PinoyMassageService.Entities
{
    public class ProfileImage
    {
        public Guid Id { get; set; }
        public byte[] Image { get; set; }
        public DateTimeOffset CreatedDate { get; set; }
    }
}
