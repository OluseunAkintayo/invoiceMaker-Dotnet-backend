using invoice_backend.Models;

namespace invoice_backend.Services.Invoice;

/// <summary>
/// Service for managing invoices
/// </summary>
public interface IInvoiceService
{
    /// <summary>
    /// Get all invoices for a user
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <returns>List of invoices</returns>
    Task<List<InvoiceDto>> GetAllInvoicesByUserAsync(int userId);

    /// <summary>
    /// Get invoice by ID
    /// </summary>
    /// <param name="invoiceId">Invoice ID</param>
    /// <param name="userId">User ID (for authorization)</param>
    /// <returns>Invoice details or null if not found</returns>
    Task<InvoiceDto?> GetInvoiceByIdAsync(int invoiceId, int userId);

    /// <summary>
    /// Create a new invoice
    /// </summary>
    /// <param name="request">Invoice creation request</param>
    /// <param name="userId">User ID</param>
    /// <returns>Created invoice</returns>
    Task<InvoiceDto> CreateInvoiceAsync(CreateInvoiceRequest request, int userId);

    /// <summary>
    /// Update an existing invoice
    /// </summary>
    /// <param name="invoiceId">Invoice ID</param>
    /// <param name="request">Update request</param>
    /// <param name="userId">User ID (for authorization)</param>
    /// <returns>Updated invoice or null if not found</returns>
    Task<InvoiceDto?> UpdateInvoiceAsync(int invoiceId, UpdateInvoiceRequest request, int userId);

    /// <summary>
    /// Delete an invoice
    /// </summary>
    /// <param name="invoiceId">Invoice ID</param>
    /// <param name="userId">User ID (for authorization)</param>
    /// <returns>True if deleted, false if not found</returns>
    Task<bool> DeleteInvoiceAsync(int invoiceId, int userId);

    /// <summary>
    /// Get invoices by status
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="status">Invoice status</param>
    /// <returns>List of invoices with specified status</returns>
    Task<List<InvoiceDto>> GetInvoicesByStatusAsync(int userId, Models.Enums.InvoiceStatus status);

    /// <summary>
    /// Update invoice status
    /// </summary>
    /// <param name="invoiceId">Invoice ID</param>
    /// <param name="status">New status</param>
    /// <param name="userId">User ID (for authorization)</param>
    /// <returns>Updated invoice or null if not found</returns>
    Task<InvoiceDto?> UpdateInvoiceStatusAsync(int invoiceId, Models.Enums.InvoiceStatus status, int userId);
}
