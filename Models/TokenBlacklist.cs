namespace invoice_backend.Models;

/// <summary>
/// Represents a blacklisted JWT token that has been invalidated (e.g., after logout)
/// </summary>
public class TokenBlacklist : BaseEntity
{
    /// <summary>
    /// The JWT token string that has been invalidated
    /// </summary>
    public string Token { get; set; } = string.Empty;

    /// <summary>
    /// The user ID associated with this token
    /// </summary>
    public int UserId { get; set; }

    /// <summary>
    /// The expiration date of the token
    /// </summary>
    public DateTime ExpiresAt { get; set; }

    /// <summary>
    /// The date and time when the token was blacklisted
    /// </summary>
    public DateTime BlacklistedAt { get; set; }
}
