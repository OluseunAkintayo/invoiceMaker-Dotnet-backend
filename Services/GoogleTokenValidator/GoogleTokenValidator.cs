using Google.Apis.Auth;
using invoice_backend.Models;

namespace invoice_backend.Services.GoogleTokenValidator;

/// <summary>
/// Service for validating Google OAuth ID tokens
/// </summary>
public class GoogleTokenValidator : IGoogleTokenValidator
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<GoogleTokenValidator> _logger;
    private readonly string _googleClientId;

    public GoogleTokenValidator(IConfiguration configuration, ILogger<GoogleTokenValidator> logger)
    {
        _configuration = configuration;
        _logger = logger;
        _googleClientId = _configuration["GoogleAuth:ClientId"]
            ?? throw new InvalidOperationException("Google ClientId is not configured");
    }

    /// <summary>
    /// Validate a Google ID token and extract token information
    /// </summary>
    public async Task<GoogleTokenInfo> ValidateTokenAsync(string idToken)
    {
        try
        {
            _logger.LogDebug("Validating Google ID token");

            // Validate the token
            var payload = await GoogleJsonWebSignature.ValidateAsync(idToken);

            // Verify the audience (client ID)
            if (!string.Equals(payload.Audience?.ToString(), _googleClientId, StringComparison.Ordinal))
            {
                _logger.LogWarning("Google token audience mismatch. Expected: {ExpectedAudience}, Got: {ActualAudience}",
                    _googleClientId, payload.Audience);
                throw new InvalidOperationException("Token audience does not match the configured client ID");
            }

            // Check if token is expired
            if (payload.ExpirationTimeSeconds.HasValue)
            {
                var expirationTime = UnixTimeStampToDateTime(payload.ExpirationTimeSeconds.Value);
                if (DateTime.UtcNow > expirationTime)
                {
                    _logger.LogWarning("Google token has expired");
                    throw new InvalidOperationException("Token has expired");
                }
            }

            _logger.LogInformation("Google token validated successfully for email: {Email}", payload.Email);

            return new GoogleTokenInfo(
                Sub: payload.Subject,
                Email: payload.Email,
                Name: payload.Name ?? string.Empty,
                Picture: payload.Picture,
                GivenName: payload.GivenName,
                FamilyName: payload.FamilyName,
                EmailVerified: payload.EmailVerified == true
            );
        }
        catch (InvalidJwtException ex)
        {
            _logger.LogWarning(ex, "Invalid Google JWT token");
            throw new InvalidOperationException("Invalid Google token", ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating Google token");
            throw;
        }
    }

    /// <summary>
    /// Convert Unix timestamp to DateTime
    /// </summary>
    private static DateTime UnixTimeStampToDateTime(long unixTimeStamp)
    {
        var dateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
        dateTime = dateTime.AddSeconds(unixTimeStamp);
        return dateTime;
    }
}
