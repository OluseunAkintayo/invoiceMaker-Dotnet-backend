using invoice_backend.Services.BackgroundServices;

namespace invoice_backend.Services.BackgroundServices;

/// <summary>
/// Hosted service that runs invoice deletion policy processing on a schedule (hourly)
/// </summary>
public class InvoiceDeletionPolicyHostedService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<InvoiceDeletionPolicyHostedService> _logger;

    // Run the deletion policy check every hour
    private readonly TimeSpan _checkInterval = TimeSpan.FromHours(1);

    public InvoiceDeletionPolicyHostedService(
        IServiceProvider serviceProvider,
        ILogger<InvoiceDeletionPolicyHostedService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Invoice Deletion Policy Hosted Service starting");

        // Delay the first execution to allow the application to fully start
        await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                _logger.LogDebug("Running invoice deletion policy check");

                using (var scope = _serviceProvider.CreateScope())
                {
                    var deletionPolicyService = scope.ServiceProvider.GetRequiredService<IInvoiceDeletionPolicyService>();
                    int processed = await deletionPolicyService.ProcessInvoiceDeletionsAsync();

                    if (processed > 0)
                    {
                        _logger.LogInformation("Invoice deletion policy processed {Count} invoices", processed);
                    }
                    else
                    {
                        _logger.LogDebug("No invoices required processing by deletion policy");
                    }
                }

                // Wait for the next check interval
                await Task.Delay(_checkInterval, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("Invoice Deletion Policy Hosted Service cancellation requested");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in Invoice Deletion Policy Hosted Service");

                // Wait before retrying to avoid hammering the database
                try
                {
                    await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
            }
        }

        _logger.LogInformation("Invoice Deletion Policy Hosted Service stopped");
    }

    public override Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Invoice Deletion Policy Hosted Service is stopping");
        return base.StopAsync(cancellationToken);
    }
}
