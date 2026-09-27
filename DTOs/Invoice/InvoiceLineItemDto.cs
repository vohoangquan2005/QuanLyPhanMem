namespace HotelManagement.DTOs.Invoice
{
    public class InvoiceLineItemDto
    {
        public string Category { get; set; } = "Other";
        public string Description { get; set; } = string.Empty;
        public decimal Quantity { get; set; } = 1;
        public decimal UnitPrice { get; set; }
        public decimal Amount { get; set; }
    }
}
