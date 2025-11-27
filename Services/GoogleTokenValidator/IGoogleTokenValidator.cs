using invoice_backend.Models;

namespace invoice_backend.Services.GoogleTokenValidator;

/// <summary>
/// Service for validating Google OAuth tokens
/// </summary>
public interface IGoogleTokenValidator
{
    /// <summary>
    /// Validate a Google ID token and extract token information
    /// </summary>
    /// <param name="idToken">Google ID token from client</param>
    /// <returns>Validated token information</returns>
    Task<GoogleTokenInfo> ValidateTokenAsync(string idToken);
}
