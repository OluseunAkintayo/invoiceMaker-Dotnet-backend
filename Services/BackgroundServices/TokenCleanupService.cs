using invoice_backend.Data;
using Microsoft.EntityFrameworkCore;

namespace invoice_backend.Services.BackgroundServices;

/// <summary>
/// Service implementation for cleaning up expired tokens from the blacklist
/// </summary>
public class TokenCleanupService : ITokenCleanupService
{
    private readonly ApplicationDbContext _dbContext;
    private readonly ILogger<TokenCleanupService> _logger;

    public TokenCleanupService(ApplicationDbContext dbContext, ILogger<TokenCleanupService> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    /// <summary>
    /// Remove expired tokens from the blacklist
    /// </summary>
    /// <returns>Number of tokens removed</returns>
    public async Task<int> CleanupExpiredTokensAsync()
    {
        _logger.LogDebug("Starting expired token cleanup");

        var now = DateTime.UtcNow;

        // Find all expired tokens
        var expiredTokens = await _dbContext.TokenBlacklists
            .Where(tb => tb.ExpiresAt < now)
            .ToListAsync();

        if (expiredTokens.Count == 0)
        {
            _logger.LogDebug("No expired tokens found to clean up");
            return 0;
        }

        // Remove expired tokens
        _dbContext.TokenBlacklists.RemoveRange(expiredTokens);
        await _dbContext.SaveChangesAsync();

        _logger.LogInformation("Cleaned up {Count} expired tokens from blacklist", expiredTokens.Count);
        return expiredTokens.Count;
    }
}
