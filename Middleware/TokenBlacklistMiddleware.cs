using invoice_backend.Services.Auth;

namespace invoice_backend.Middleware;

/// <summary>
/// Middleware to check if JWT token is blacklisted before processing the request
/// </summary>
public class TokenBlacklistMiddleware(RequestDelegate next, ILogger<TokenBlacklistMiddleware> logger)
{
    private readonly RequestDelegate _next = next;
    private readonly ILogger<TokenBlacklistMiddleware> _logger = logger;

    public async Task InvokeAsync(HttpContext context, IAuthService authService)
    {
        // Extract token from Authorization header
        var authHeader = context.Request.Headers.Authorization.FirstOrDefault();

        if (!string.IsNullOrWhiteSpace(authHeader) && authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
        {
            // Remove "Bearer " prefix (7 characters) using range operator
            var token = authHeader[7..];
            Console.WriteLine("Checking authHeader " + authHeader);
            // Check if token is blacklisted
            var isBlacklisted = await authService.IsTokenBlacklistedAsync(token);

            if (isBlacklisted)
            {
                _logger.LogWarning("Request rejected: Token is blacklisted");
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsJsonAsync(new
                {
                    success = false,
                    message = "Token is invalid. Please log in again."
                });
                return;
            }
        }

        await _next(context);
    }
}
