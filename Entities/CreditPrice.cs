namespace PinoyMassageService.Entities
{
    public class CreditPrice
    {
        public Guid Id { get; set; }
        public int Price { get; set; }
        public int CreditValue { get; set; }
        public DateTimeOffset CreatedDate { get; set; }
    }
}
