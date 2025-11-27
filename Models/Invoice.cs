using invoice_backend.Models.Enums;

namespace invoice_backend.Models;

public class Invoice : BaseEntity
{
  public string InvoiceNumber { get; set; } = string.Empty;
  public int UserId { get; set; }
  public User User { get; set; } = null!;
  public InvoiceStatus InvoiceStatus { get; set; } = InvoiceStatus.Draft;
  public string BillerName { get; set; } = string.Empty;
  public string BillerAddress { get; set; } = string.Empty;
  public string BillerEmail { get; set; } = string.Empty;
  public string ClientName { get; set; } = string.Empty;
  public string ClientAddress { get; set; } = string.Empty;
  public string ClientEmail { get; set; } = string.Empty;
  public DateTime InvoiceDate { get; set; }
  public DateTime InvoiceDueDate { get; set; }
  public string Currency { get; set; } = string.Empty;
  public decimal SubTotal { get; set; }
  public decimal TaxPercentage { get; set; }
  public decimal TaxAmount { get; set; }
  public decimal Discount { get; set; }
  public decimal Total { get; set; }
  public string Notes { get; set; } = string.Empty;
  public string? LogoUrl { get; set; }
  public ICollection<InvoiceItem> InvoiceItems { get; set; } = new List<InvoiceItem>();
}


public class InvoiceItem
{
  public int Id { get; set; }

  public int InvoiceId { get; set; }

  public Invoice Invoice { get; set; } = null!;

  public string Description { get; set; } = string.Empty;

  public int Quantity { get; set; }

  public decimal Rate { get; set; }

  public decimal Total { get; set; }
}
