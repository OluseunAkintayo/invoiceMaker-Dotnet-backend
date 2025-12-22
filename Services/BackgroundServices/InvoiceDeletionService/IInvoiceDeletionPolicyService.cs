namespace invoice_backend.Services.BackgroundServices;

/// <summary>
/// Service for managing invoice deletion policies
/// </summary>
public interface IInvoiceDeletionPolicyService
{
    /// <summary>
    /// Process invoices that need status updates based on deletion policies
    /// </summary>
    /// <returns>Number of invoices processed</returns>
    Task<int> ProcessInvoiceDeletionsAsync();
}
