using invoice_backend.Models;

namespace invoice_backend.Services.Auth;

/// <summary>
/// Service for handling authentication and authorization operations
/// </summary>
public interface IAuthService
{
    /// <summary>
    /// Authenticate a user with email and password
    /// </summary>
    /// <param name="request">Login request with email and password</param>
    /// <returns>Login response with JWT token if successful</returns>
    Task<LoginResponse> LoginAsync(LoginRequest request);

    /// <summary>
    /// Register a new user
    /// </summary>
    /// <param name="request">Registration request with user details</param>
    /// <returns>Login response with JWT token for the newly created user</returns>
    Task<LoginResponse> RegisterAsync(RegisterRequest request);

    /// <summary>
    /// Verify a password against a user's password hash
    /// </summary>
    /// <param name="password">Plain text password</param>
    /// <param name="passwordHash">Hashed password</param>
    /// <returns>True if password matches, false otherwise</returns>
    bool VerifyPassword(string password, string passwordHash);

    /// <summary>
    /// Generate a JWT token for a user
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="email">User email</param>
    /// <returns>JWT token string</returns>
    string GenerateToken(int userId, string email);

    /// <summary>
    /// Login or register a user with Google OAuth
    /// </summary>
    /// <param name="request">Google login request with ID token</param>
    /// <returns>Login response with JWT token</returns>
    Task<LoginResponse> GoogleLoginAsync(GoogleLoginRequest request);
}
