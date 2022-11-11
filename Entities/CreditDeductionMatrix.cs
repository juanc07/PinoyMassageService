namespace PinoyMassageService.Entities
{
    public class CreditDeductionMatrix
    {
        public Guid Id { get; set; }
        public int Duration { get; set; }
        public int Deduction { get; set; }
        public DateTimeOffset CreatedDate { get; set; }
    }
}
