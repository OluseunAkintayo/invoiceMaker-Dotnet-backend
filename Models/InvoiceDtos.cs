using invoice_backend.Models.Enums;

namespace invoice_backend.Models;

/// <summary>
/// Invoice item data transfer object
/// </summary>
public record InvoiceItemDto(
    int Id,
    int InvoiceId,
    string Description,
    int Quantity,
    decimal Rate,
    decimal Total
);

/// <summary>
/// Create invoice item request
/// </summary>
public record CreateInvoiceItemRequest(
    string Description,
    int Quantity,
    decimal Rate
);

/// <summary>
/// Update invoice item request
/// </summary>
public record UpdateInvoiceItemRequest(
    string? Description,
    int? Quantity,
    decimal? Rate
);

/// <summary>
/// Invoice data transfer object
/// </summary>
public record InvoiceDto(
    int Id,
    string InvoiceNumber,
    int UserId,
    InvoiceStatus InvoiceStatus,
    string BillerName,
    string BillerAddress,
    string BillerEmail,
    string ClientName,
    string ClientAddress,
    string ClientEmail,
    DateTime InvoiceDate,
    DateTime InvoiceDueDate,
    string Currency,
    decimal SubTotal,
    decimal TaxPercentage,
    decimal TaxAmount,
    decimal Discount,
    decimal Total,
    string Notes,
    string? LogoUrl,
    List<InvoiceItemDto> InvoiceItems,
    DateTime CreatedAt,
    DateTime ModifiedAt
);

/// <summary>
/// Create invoice request
/// </summary>
public record CreateInvoiceRequest(
    string BillerName,
    string BillerAddress,
    string BillerEmail,
    string ClientName,
    string ClientAddress,
    string ClientEmail,
    DateTime InvoiceDate,
    DateTime InvoiceDueDate,
    string Currency,
    decimal TaxPercentage,
    decimal Discount,
    string Notes,
    string? LogoUrl,
    List<CreateInvoiceItemRequest> InvoiceItems
);

/// <summary>
/// Update invoice request
/// </summary>
public record UpdateInvoiceRequest(
    string? BillerName,
    string? BillerAddress,
    string? BillerEmail,
    string? ClientName,
    string? ClientAddress,
    string? ClientEmail,
    DateTime? InvoiceDate,
    DateTime? InvoiceDueDate,
    string? Currency,
    decimal? TaxPercentage,
    decimal? Discount,
    string? Notes,
    string? LogoUrl,
    InvoiceStatus? InvoiceStatus,
    List<CreateInvoiceItemRequest>? InvoiceItems
);

/// <summary>
/// Invoice response wrapper
/// </summary>
public record InvoiceResponse(
    bool Success,
    string Message,
    InvoiceDto? Data = null
);

/// <summary>
/// Invoice list response
/// </summary>
public record InvoiceListResponse(
    bool Success,
    string Message,
    List<InvoiceDto> Data,
    int TotalCount
);
