using invoice_backend.Models;
using invoice_backend.Services.Auth;
using Microsoft.AspNetCore.Mvc;

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
}
