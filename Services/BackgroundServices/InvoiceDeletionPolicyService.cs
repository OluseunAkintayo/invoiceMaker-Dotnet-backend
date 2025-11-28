using invoice_backend.Data;
using invoice_backend.Models;
using invoice_backend.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace invoice_backend.Services.BackgroundServices;

/// <summary>
/// Service that enforces invoice deletion policies:
/// - Soft-deleted invoices become hard-deleted after 30 days
/// - Hard-deleted invoices are permanently removed after 90 days
/// </summary>
public class InvoiceDeletionPolicyService : IInvoiceDeletionPolicyService
{
    private readonly ApplicationDbContext _dbContext;
    private readonly ILogger<InvoiceDeletionPolicyService> _logger;

    // Policy configuration
    private const int SoftDeleteGracePeriodDays = 30;
    private const int HardDeleteGracePeriodDays = 120;

    public InvoiceDeletionPolicyService(
        ApplicationDbContext dbContext,
        ILogger<InvoiceDeletionPolicyService> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    /// <summary>
    /// Process invoices based on deletion policies
    /// </summary>
    public async Task<int> ProcessInvoiceDeletionsAsync()
    {
        try
        {
            _logger.LogInformation("Starting invoice deletion policy processing");

            int totalProcessed = 0;

            // Process soft-deleted invoices that are older than 30 days
            totalProcessed += await ProcessSoftDeletedInvoicesAsync();

            // Process hard-deleted invoices that are older than 90 days
            totalProcessed += await ProcessHardDeletedInvoicesAsync();

            _logger.LogInformation("Completed invoice deletion policy processing. Total invoices processed: {Count}", totalProcessed);
            return totalProcessed;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing invoice deletion policies");
            throw;
        }
    }

    /// <summary>
    /// Convert soft-deleted invoices to hard-deleted if they've been soft-deleted for 30+ days
    /// </summary>
    private async Task<int> ProcessSoftDeletedInvoicesAsync()
    {
        try
        {
            _logger.LogDebug("Processing soft-deleted invoices older than {Days} days", SoftDeleteGracePeriodDays);

            var cutoffDate = DateTime.UtcNow.AddDays(-SoftDeleteGracePeriodDays);

            // Find invoices that were soft-deleted before the cutoff date
            var invoicesToHardDelete = await _dbContext.Invoices
                .Where(i => i.Status == Status.SoftDeleted &&
                            i.SoftDeletedAt.HasValue &&
                            i.SoftDeletedAt < cutoffDate)
                .ToListAsync();

            if (invoicesToHardDelete.Count == 0)
            {
                _logger.LogDebug("No invoices found for soft-to-hard deletion conversion");
                return 0;
            }

            _logger.LogInformation("Found {Count} invoices to convert to hard-deleted status", invoicesToHardDelete.Count);

            foreach (var invoice in invoicesToHardDelete)
            {
                invoice.Status = Status.HardDeleted;
                invoice.ModifiedAt = DateTime.UtcNow;
                _logger.LogDebug("Marked invoice {InvoiceId} as hard-deleted", invoice.Id);
            }

            await _dbContext.SaveChangesAsync();
            _logger.LogInformation("Successfully converted {Count} invoices from soft-deleted to hard-deleted", invoicesToHardDelete.Count);

            return invoicesToHardDelete.Count;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing soft-deleted invoices");
            throw;
        }
    }

    /// <summary>
    /// Permanently delete invoices that have been hard-deleted for 90+ days
    /// </summary>
    private async Task<int> ProcessHardDeletedInvoicesAsync()
    {
        try
        {
            _logger.LogDebug("Processing hard-deleted invoices older than {Days} days", HardDeleteGracePeriodDays);

            var cutoffDate = DateTime.UtcNow.AddDays(-HardDeleteGracePeriodDays);

            // Find invoices that were hard-deleted before the cutoff date
            var invoicesToPermanentlyDelete = await _dbContext.Invoices
                .Include(i => i.InvoiceItems)
                .Where(i => i.Status == Status.HardDeleted &&
                            i.ModifiedAt < cutoffDate)
                .ToListAsync();

            if (invoicesToPermanentlyDelete.Count == 0)
            {
                _logger.LogDebug("No invoices found for permanent deletion");
                return 0;
            }

            _logger.LogInformation("Found {Count} invoices to permanently delete", invoicesToPermanentlyDelete.Count);

            // Delete invoice items first (foreign key constraint)
            foreach (var invoice in invoicesToPermanentlyDelete)
            {
                _logger.LogDebug("Removing {ItemCount} items from invoice {InvoiceId}", invoice.InvoiceItems.Count, invoice.Id);
                _dbContext.InvoiceItems.RemoveRange(invoice.InvoiceItems);
            }

            // Delete invoices
            _dbContext.Invoices.RemoveRange(invoicesToPermanentlyDelete);
            await _dbContext.SaveChangesAsync();

            _logger.LogInformation("Successfully permanently deleted {Count} invoices and their items", invoicesToPermanentlyDelete.Count);

            return invoicesToPermanentlyDelete.Count;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing hard-deleted invoices for permanent deletion");
            throw;
        }
    }
}
