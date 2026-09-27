namespace HotelManagement.DTOs.Invoice
{
    public class InvoiceSnapshotDto
    {
        public int BookingId { get; set; }
        public string? CustomerFullName { get; set; }
        public string? CustomerCCCD { get; set; }
        public string? CustomerNationality { get; set; }
        public DateTime ExpectedCheckIn { get; set; }
        public DateTime ExpectedCheckOut { get; set; }
        public DateTime? ActualCheckIn { get; set; }
        public DateTime? ActualCheckOut { get; set; }
        public decimal SubTotal { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal AmountDue { get; set; }
        public List<InvoiceLineItemDto> LineItems { get; set; } = new();
    }
}
