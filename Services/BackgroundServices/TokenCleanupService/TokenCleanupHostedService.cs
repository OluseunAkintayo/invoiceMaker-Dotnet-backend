namespace invoice_backend.Services.BackgroundServices;

/// <summary>
/// Hosted service that runs token cleanup on a schedule (every 30 minutes)
/// </summary>
public class TokenCleanupHostedService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<TokenCleanupHostedService> _logger;

    // Run cleanup every 30 minutes
    private readonly TimeSpan _checkInterval = TimeSpan.FromMinutes(30);

    public TokenCleanupHostedService(
        IServiceProvider serviceProvider,
        ILogger<TokenCleanupHostedService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Token Cleanup Hosted Service starting");

        // Delay the first execution to allow the application to fully start
        await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                _logger.LogDebug("Running token cleanup");

                using (var scope = _serviceProvider.CreateScope())
                {
                    var tokenCleanupService = scope.ServiceProvider.GetRequiredService<ITokenCleanupService>();
                    int cleaned = await tokenCleanupService.CleanupExpiredTokensAsync();

                    if (cleaned > 0)
                    {
                        _logger.LogInformation("Token cleanup removed {Count} expired tokens", cleaned);
                    }
                    else
                    {
                        _logger.LogDebug("No expired tokens to clean up");
                    }
                }

                // Wait for the next check interval
                await Task.Delay(_checkInterval, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("Token Cleanup Hosted Service cancellation requested");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in Token Cleanup Hosted Service");

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

        _logger.LogInformation("Token Cleanup Hosted Service stopped");
    }

    public override Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Token Cleanup Hosted Service is stopping");
        return base.StopAsync(cancellationToken);
    }
}
