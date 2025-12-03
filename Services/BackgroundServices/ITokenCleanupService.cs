namespace invoice_backend.Services.BackgroundServices;

/// <summary>
/// Service for cleaning up expired tokens from the blacklist
/// </summary>
public interface ITokenCleanupService
{
    /// <summary>
    /// Remove expired tokens from the blacklist
    /// </summary>
    /// <returns>Number of tokens removed</returns>
    Task<int> CleanupExpiredTokensAsync();
}
