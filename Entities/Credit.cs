﻿namespace PinoyMassageService.Entities
{
    public class Credit
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public int Amount { get; set; }
        public DateTimeOffset CreatedDate { get; set; }
    }
}
