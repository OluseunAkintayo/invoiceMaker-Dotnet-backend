namespace invoice_backend.Models.Enums;

public enum InvoiceStatus
{
    Draft = 0,
    Sent = 1,
    Viewed = 2,
    PartiallyPaid = 3,
    Paid = 4,
    Overdue = 5,
    Cancelled = 6,
    Refunded = 7,
    Archived = 8
}
