using invoice_backend.Data;
using invoice_backend.Models;
using invoice_backend.Models.Enums;
using InvoiceModel = invoice_backend.Models.Invoice;
using Microsoft.EntityFrameworkCore;

namespace invoice_backend.Services.Invoice;

/// <summary>
/// Service for managing invoices
/// </summary>
public class InvoiceService : IInvoiceService
{
    private readonly ApplicationDbContext _dbContext;
    private readonly ILogger<InvoiceService> _logger;

    public InvoiceService(ApplicationDbContext dbContext, ILogger<InvoiceService> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    /// <summary>
    /// Get all invoices for a user (excludes soft-deleted invoices)
    /// </summary>
    public async Task<List<InvoiceDto>> GetAllInvoicesByUserAsync(int userId)
    {
        _logger.LogDebug("Fetching all invoices for user {UserId}", userId);

        var invoices = await _dbContext.Invoices
            .Where(i => i.UserId == userId && i.Status != Status.SoftDeleted)
            .Include(i => i.InvoiceItems)
            .OrderByDescending(i => i.CreatedAt)
            .ToListAsync();

        _logger.LogDebug("Found {Count} invoices for user {UserId}", invoices.Count, userId);
        return invoices.Select(MapToInvoiceDto).ToList();
    }

    /// <summary>
    /// Get invoice by ID with authorization check (excludes soft-deleted invoices)
    /// </summary>
    public async Task<InvoiceDto?> GetInvoiceByIdAsync(int invoiceId, int userId)
    {
        _logger.LogDebug("Fetching invoice {InvoiceId} for user {UserId}", invoiceId, userId);

        var invoice = await _dbContext.Invoices
            .Where(i => i.Id == invoiceId && i.UserId == userId && i.Status != Status.SoftDeleted)
            .Include(i => i.InvoiceItems)
            .FirstOrDefaultAsync();

        if (invoice == null)
        {
            _logger.LogDebug("Invoice {InvoiceId} not found for user {UserId}", invoiceId, userId);
            return null;
        }

        return MapToInvoiceDto(invoice);
    }

    /// <summary>
    /// Create a new invoice with items
    /// </summary>
    public async Task<InvoiceDto> CreateInvoiceAsync(CreateInvoiceRequest request, int userId)
    {
        _logger.LogInformation("Creating invoice for user {UserId}", userId);

        // Validate request
        ValidateCreateInvoiceRequest(request);

        // Generate unique invoice number
        var invoiceNumber = await GenerateInvoiceNumberAsync();

        // Calculate totals
        var subTotal = request.InvoiceItems.Sum(item => item.Quantity * item.Rate);
        var taxAmount = subTotal * (request.TaxPercentage / 100);
        var total = subTotal + taxAmount - request.Discount;

        var invoice = new InvoiceModel
        {
            InvoiceNumber = invoiceNumber,
            UserId = userId,
            InvoiceStatus = InvoiceStatus.Draft,
            BillerName = request.BillerName,
            BillerAddress = request.BillerAddress,
            BillerEmail = request.BillerEmail,
            ClientName = request.ClientName,
            ClientAddress = request.ClientAddress,
            ClientEmail = request.ClientEmail,
            InvoiceDate = request.InvoiceDate,
            InvoiceDueDate = request.InvoiceDueDate,
            Currency = request.Currency,
            SubTotal = subTotal,
            TaxPercentage = request.TaxPercentage,
            TaxAmount = taxAmount,
            Discount = request.Discount,
            Total = total,
            Notes = request.Notes,
            LogoUrl = request.LogoUrl,
            CreatedAt = DateTime.UtcNow,
            ModifiedAt = DateTime.UtcNow,
            IsActive = true
        };

        // Add invoice items
        foreach (var itemRequest in request.InvoiceItems)
        {
            var itemTotal = itemRequest.Quantity * itemRequest.Rate;
            invoice.InvoiceItems.Add(new InvoiceItem
            {
                Description = itemRequest.Description,
                Quantity = itemRequest.Quantity,
                Rate = itemRequest.Rate,
                Total = itemTotal
            });
        }

        _dbContext.Invoices.Add(invoice);
        _logger.LogDebug("Saving invoice {InvoiceNumber} to database", invoiceNumber);
        await _dbContext.SaveChangesAsync();

        _logger.LogInformation("Invoice {InvoiceNumber} created successfully with ID {InvoiceId}", invoiceNumber, invoice.Id);
        return MapToInvoiceDto(invoice);
    }

    /// <summary>
    /// Update an existing invoice (excludes soft-deleted invoices)
    /// </summary>
    public async Task<InvoiceDto?> UpdateInvoiceAsync(int invoiceId, UpdateInvoiceRequest request, int userId)
    {
        _logger.LogInformation("Updating invoice {InvoiceId} for user {UserId}", invoiceId, userId);

        var invoice = await _dbContext.Invoices
            .Include(i => i.InvoiceItems)
            .FirstOrDefaultAsync(i => i.Id == invoiceId && i.UserId == userId && i.Status != Status.SoftDeleted);

        if (invoice == null)
        {
            _logger.LogWarning("Invoice {InvoiceId} not found for user {UserId}", invoiceId, userId);
            return null;
        }

        // Update basic fields
        if (!string.IsNullOrWhiteSpace(request.BillerName))
            invoice.BillerName = request.BillerName;

        if (!string.IsNullOrWhiteSpace(request.BillerAddress))
            invoice.BillerAddress = request.BillerAddress;

        if (!string.IsNullOrWhiteSpace(request.BillerEmail))
            invoice.BillerEmail = request.BillerEmail;

        if (!string.IsNullOrWhiteSpace(request.ClientName))
            invoice.ClientName = request.ClientName;

        if (!string.IsNullOrWhiteSpace(request.ClientAddress))
            invoice.ClientAddress = request.ClientAddress;

        if (!string.IsNullOrWhiteSpace(request.ClientEmail))
            invoice.ClientEmail = request.ClientEmail;

        if (request.InvoiceDate.HasValue)
            invoice.InvoiceDate = request.InvoiceDate.Value;

        if (request.InvoiceDueDate.HasValue)
            invoice.InvoiceDueDate = request.InvoiceDueDate.Value;

        if (!string.IsNullOrWhiteSpace(request.Currency))
            invoice.Currency = request.Currency;

        if (request.TaxPercentage.HasValue)
            invoice.TaxPercentage = request.TaxPercentage.Value;

        if (request.Discount.HasValue)
            invoice.Discount = request.Discount.Value;

        if (!string.IsNullOrWhiteSpace(request.Notes))
            invoice.Notes = request.Notes;

        if (request.LogoUrl != null)
            invoice.LogoUrl = request.LogoUrl;

        if (request.InvoiceStatus.HasValue)
            invoice.InvoiceStatus = request.InvoiceStatus.Value;

        // Update invoice items if provided
        if (request.InvoiceItems != null && request.InvoiceItems.Count > 0)
        {
            invoice.InvoiceItems.Clear();
            foreach (var itemRequest in request.InvoiceItems)
            {
                var itemTotal = itemRequest.Quantity * itemRequest.Rate;
                invoice.InvoiceItems.Add(new InvoiceItem
                {
                    Description = itemRequest.Description,
                    Quantity = itemRequest.Quantity,
                    Rate = itemRequest.Rate,
                    Total = itemTotal
                });
            }
        }

        // Recalculate totals
        var subTotal = invoice.InvoiceItems.Sum(item => item.Quantity * item.Rate);
        invoice.SubTotal = subTotal;
        invoice.TaxAmount = subTotal * (invoice.TaxPercentage / 100);
        invoice.Total = subTotal + invoice.TaxAmount - invoice.Discount;
        invoice.ModifiedAt = DateTime.UtcNow;

        _logger.LogDebug("Saving updates for invoice {InvoiceId}", invoiceId);
        await _dbContext.SaveChangesAsync();

        _logger.LogInformation("Invoice {InvoiceId} updated successfully", invoiceId);
        return MapToInvoiceDto(invoice);
    }

    /// <summary>
    /// Soft delete an invoice by setting status to SoftDeleted
    /// </summary>
    public async Task<bool> DeleteInvoiceAsync(int invoiceId, int userId)
    {
        _logger.LogInformation("Soft deleting invoice {InvoiceId} for user {UserId}", invoiceId, userId);

        var invoice = await _dbContext.Invoices
            .FirstOrDefaultAsync(i => i.Id == invoiceId && i.UserId == userId);

        if (invoice == null)
        {
            _logger.LogWarning("Invoice {InvoiceId} not found for soft deletion", invoiceId);
            return false;
        }

        invoice.Status = Status.SoftDeleted;
        invoice.SoftDeletedAt = DateTime.UtcNow;
        invoice.ModifiedAt = DateTime.UtcNow;

        _logger.LogDebug("Marking invoice {InvoiceId} as soft deleted", invoiceId);
        await _dbContext.SaveChangesAsync();

        _logger.LogInformation("Invoice {InvoiceId} soft deleted successfully", invoiceId);
        return true;
    }

    /// <summary>
    /// Get invoices by status (excludes soft-deleted invoices)
    /// </summary>
    public async Task<List<InvoiceDto>> GetInvoicesByStatusAsync(int userId, InvoiceStatus status)
    {
        _logger.LogDebug("Fetching invoices with status {Status} for user {UserId}", status, userId);

        var invoices = await _dbContext.Invoices
            .Where(i => i.UserId == userId && i.InvoiceStatus == status && i.Status != Status.SoftDeleted)
            .Include(i => i.InvoiceItems)
            .OrderByDescending(i => i.CreatedAt)
            .ToListAsync();

        _logger.LogDebug("Found {Count} {Status} invoices for user {UserId}", invoices.Count, status, userId);
        return invoices.Select(MapToInvoiceDto).ToList();
    }

    /// <summary>
    /// Update invoice status (excludes soft-deleted invoices)
    /// </summary>
    public async Task<InvoiceDto?> UpdateInvoiceStatusAsync(int invoiceId, InvoiceStatus status, int userId)
    {
        _logger.LogInformation("Updating status of invoice {InvoiceId} to {Status}", invoiceId, status);

        var invoice = await _dbContext.Invoices
            .Include(i => i.InvoiceItems)
            .FirstOrDefaultAsync(i => i.Id == invoiceId && i.UserId == userId && i.Status != Status.SoftDeleted);

        if (invoice == null)
        {
            _logger.LogWarning("Invoice {InvoiceId} not found for status update", invoiceId);
            return null;
        }

        invoice.InvoiceStatus = status;
        invoice.ModifiedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync();
        _logger.LogInformation("Invoice {InvoiceId} status updated to {Status}", invoiceId, status);

        return MapToInvoiceDto(invoice);
    }

    /// <summary>
    /// Map Invoice entity to InvoiceDto
    /// </summary>
    private static InvoiceDto MapToInvoiceDto(InvoiceModel invoice)
    {
        return new InvoiceDto(
            Id: invoice.Id,
            InvoiceNumber: invoice.InvoiceNumber,
            UserId: invoice.UserId,
            InvoiceStatus: invoice.InvoiceStatus,
            BillerName: invoice.BillerName,
            BillerAddress: invoice.BillerAddress,
            BillerEmail: invoice.BillerEmail,
            ClientName: invoice.ClientName,
            ClientAddress: invoice.ClientAddress,
            ClientEmail: invoice.ClientEmail,
            InvoiceDate: invoice.InvoiceDate,
            InvoiceDueDate: invoice.InvoiceDueDate,
            Currency: invoice.Currency,
            SubTotal: invoice.SubTotal,
            TaxPercentage: invoice.TaxPercentage,
            TaxAmount: invoice.TaxAmount,
            Discount: invoice.Discount,
            Total: invoice.Total,
            Notes: invoice.Notes,
            LogoUrl: invoice.LogoUrl,
            InvoiceItems: invoice.InvoiceItems
                .Select(ii => new InvoiceItemDto(
                    Id: ii.Id,
                    InvoiceId: ii.InvoiceId,
                    Description: ii.Description,
                    Quantity: ii.Quantity,
                    Rate: ii.Rate,
                    Total: ii.Total))
                .ToList(),
            CreatedAt: invoice.CreatedAt,
            ModifiedAt: invoice.ModifiedAt
        );
    }

    /// <summary>
    /// Validate create invoice request
    /// </summary>
    private void ValidateCreateInvoiceRequest(CreateInvoiceRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.BillerName))
            throw new ArgumentException("Biller name is required");

        if (string.IsNullOrWhiteSpace(request.BillerEmail))
            throw new ArgumentException("Biller email is required");

        if (string.IsNullOrWhiteSpace(request.ClientName))
            throw new ArgumentException("Client name is required");

        if (string.IsNullOrWhiteSpace(request.ClientEmail))
            throw new ArgumentException("Client email is required");

        if (string.IsNullOrWhiteSpace(request.Currency))
            throw new ArgumentException("Currency is required");

        if (request.InvoiceItems == null || request.InvoiceItems.Count == 0)
            throw new ArgumentException("At least one invoice item is required");

        foreach (var item in request.InvoiceItems)
        {
            if (string.IsNullOrWhiteSpace(item.Description))
                throw new ArgumentException("Invoice item description is required");

            if (item.Quantity <= 0)
                throw new ArgumentException("Invoice item quantity must be greater than 0");

            if (item.Rate < 0)
                throw new ArgumentException("Invoice item rate cannot be negative");
        }
    }

    /// <summary>
    /// Generate a unique invoice number
    /// </summary>
    private async Task<string> GenerateInvoiceNumberAsync()
    {
        var lastInvoice = await _dbContext.Invoices
            .OrderByDescending(i => i.CreatedAt)
            .FirstOrDefaultAsync();

        var nextNumber = lastInvoice == null ? 1 : int.Parse(lastInvoice.InvoiceNumber.Replace("INV-", "")) + 1;
        return $"INV-{nextNumber:D6}";
    }
}
