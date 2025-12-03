using invoice_backend.Models;
using invoice_backend.Services.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace invoice_backend.Controllers;

/// <summary>
/// Controller for authentication operations (login, registration)
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IAuthService authService, ILogger<AuthController> logger)
    {
        _authService = authService;
        _logger = logger;
    }

    /// <summary>
    /// Extract userId from JWT claims
    /// </summary>
    /// <returns>User ID from claims or 0 if not found</returns>
    private int GetUserIdFromClaims()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)
            ?? User.FindFirst("sub");

        if (userIdClaim != null && int.TryParse(userIdClaim.Value, out var userId))
        {
            return userId;
        }

        _logger.LogWarning("Unable to extract userId from JWT claims");
        return 0;
    }

    /// <summary>
    /// Login with email and password
    /// </summary>
    /// <param name="request">Login request containing email and password</param>
    /// <returns>Login response with JWT token</returns>
    [HttpPost("login")]
    public async Task<ActionResult<AuthResponse>> Login([FromBody] LoginRequest request)
    {
        _logger.LogInformation("Login request received for email: {Email}", request.Email);

        try
        {
            var loginResponse = await _authService.LoginAsync(request);
            _logger.LogInformation("Login successful for user: {UserId}", loginResponse.Id);

            return Ok(new AuthResponse(
                Success: true,
                Message: "Login successful",
                Data: loginResponse
            ));
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Login validation failed: {Message}", ex.Message);
            return BadRequest(new AuthResponse(
                Success: false,
                Message: ex.Message
            ));
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, "Login failed: Invalid credentials");
            return Unauthorized(new AuthResponse(
                Success: false,
                Message: ex.Message
            ));
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Login failed: {Message}", ex.Message);
            return BadRequest(new AuthResponse(
                Success: false,
                Message: ex.Message
            ));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during login");
            return StatusCode(StatusCodes.Status500InternalServerError, new AuthResponse(
                Success: false,
                Message: "An unexpected error occurred during login"
            ));
        }
    }

    /// <summary>
    /// Register a new user
    /// </summary>
    /// <param name="request">Registration request with user details</param>
    /// <returns>Login response with JWT token for the newly created user</returns>
    [HttpPost("register")]
    public async Task<ActionResult<AuthResponse>> Register([FromBody] RegisterRequest request)
    {
        _logger.LogInformation("Registration request received for email: {Email}", request.Email);

        try
        {
            var loginResponse = await _authService.RegisterAsync(request);
            _logger.LogInformation("Registration successful for user: {UserId}", loginResponse.Id);

            return CreatedAtAction(nameof(Register), new AuthResponse(
                Success: true,
                Message: "Registration successful",
                Data: loginResponse
            ));
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Registration validation failed: {Message}", ex.Message);
            return BadRequest(new AuthResponse(
                Success: false,
                Message: ex.Message
            ));
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Registration failed: {Message}", ex.Message);
            return BadRequest(new AuthResponse(
                Success: false,
                Message: ex.Message
            ));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during registration");
            return StatusCode(StatusCodes.Status500InternalServerError, new AuthResponse(
                Success: false,
                Message: "An unexpected error occurred during registration"
            ));
        }
    }

    /// <summary>
    /// Login or register with Google OAuth
    /// </summary>
    /// <param name="request">Google login request containing ID token from Google Sign-In</param>
    /// <returns>Login response with JWT token</returns>
    [HttpPost("google")]
    public async Task<ActionResult<AuthResponse>> GoogleLogin([FromBody] GoogleLoginRequest request)
    {
        _logger.LogInformation("Google login request received");

        try
        {
            var loginResponse = await _authService.GoogleLoginAsync(request);
            _logger.LogInformation("Google login successful for user: {UserId}", loginResponse.Id);

            return Ok(new AuthResponse(
                Success: true,
                Message: "Google login successful",
                Data: loginResponse
            ));
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Google login validation failed: {Message}", ex.Message);
            return BadRequest(new AuthResponse(
                Success: false,
                Message: ex.Message
            ));
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Google login failed: {Message}", ex.Message);
            return BadRequest(new AuthResponse(
                Success: false,
                Message: ex.Message
            ));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during Google login");
            return StatusCode(StatusCodes.Status500InternalServerError, new AuthResponse(
                Success: false,
                Message: "An unexpected error occurred during Google login"
            ));
        }
    }

    /// <summary>
    /// Logout the current user by invalidating their JWT token
    /// </summary>
    /// <returns>Success response</returns>
    [HttpPost("logout")]
    [Authorize]
    public async Task<ActionResult<AuthResponse>> Logout()
    {
        _logger.LogInformation("Logout request received");

        try
        {
            // Extract token from Authorization header
            var authHeader = Request.Headers["Authorization"].ToString();
            if (string.IsNullOrWhiteSpace(authHeader) || !authHeader.StartsWith("Bearer "))
            {
                _logger.LogWarning("Logout failed: No valid authorization header");
                return Unauthorized(new AuthResponse(
                    Success: false,
                    Message: "No valid authorization token found"
                ));
            }

            var token = authHeader.Substring("Bearer ".Length).Trim();

            // Get user ID from claims
            var userId = GetUserIdFromClaims();
            if (userId == 0)
            {
                _logger.LogWarning("Logout failed: Unable to extract user ID from token");
                return Unauthorized(new AuthResponse(
                    Success: false,
                    Message: "Invalid token"
                ));
            }

            // Invalidate the token
            await _authService.LogoutAsync(token, userId);

            _logger.LogInformation("Logout successful for user: {UserId}", userId);

            return Ok(new AuthResponse(
                Success: true,
                Message: "Logout successful"
            ));
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Logout validation failed: {Message}", ex.Message);
            return BadRequest(new AuthResponse(
                Success: false,
                Message: ex.Message
            ));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during logout");
            return StatusCode(StatusCodes.Status500InternalServerError, new AuthResponse(
                Success: false,
                Message: "An unexpected error occurred during logout"
            ));
        }
    }
}
