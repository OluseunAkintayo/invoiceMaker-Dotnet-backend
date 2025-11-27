using invoice_backend.Data;
using invoice_backend.Models;
using Microsoft.EntityFrameworkCore;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace invoice_backend.Services.Auth;

#nullable enable

/// <summary>
/// Service for authentication operations including login, registration, and token generation
/// </summary>
public class AuthService : IAuthService
{
    private readonly ApplicationDbContext _dbContext;
    private readonly ILogger<AuthService> _logger;
    private readonly IConfiguration _configuration;
    private readonly IGoogleTokenValidator _googleTokenValidator;
    private readonly IEmailService _emailService;

    public AuthService(
        ApplicationDbContext dbContext,
        ILogger<AuthService> logger,
        IConfiguration configuration,
        IGoogleTokenValidator googleTokenValidator,
        IEmailService emailService)
    {
        _dbContext = dbContext;
        _logger = logger;
        _configuration = configuration;
        _googleTokenValidator = googleTokenValidator;
        _emailService = emailService;
    }

    /// <summary>
    /// Authenticate a user with email and password
    /// </summary>
    public async Task<LoginResponse> LoginAsync(LoginRequest request)
    {
        _logger.LogInformation("Login attempt for email: {Email}", request.Email);

        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
        {
            _logger.LogWarning("Login validation failed: Email or password is empty");
            throw new ArgumentException("Email and password are required");
        }

        var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
        if (user == null)
        {
            _logger.LogWarning("Login failed: User with email {Email} not found", request.Email);
            throw new UnauthorizedAccessException("Invalid email or password");
        }

        if (!VerifyPassword(request.Password, user.PasswordHash))
        {
            _logger.LogWarning("Login failed: Invalid password for user {Email}", request.Email);
            throw new UnauthorizedAccessException("Invalid email or password");
        }

        if (!user.IsActive)
        {
            _logger.LogWarning("Login failed: User {Email} is not active", request.Email);
            throw new InvalidOperationException("User account is inactive");
        }

        _logger.LogInformation("Login successful for user: {UserId} with email: {Email}", user.Id, user.Email);

        var token = GenerateToken(user.Id, user.Email);
        var expiresAt = DateTime.UtcNow.AddHours(24);

        return new LoginResponse(
            user.Id,
            user.Email,
            user.FirstName,
            user.LastName,
            token,
            expiresAt
        );
    }

    /// <summary>
    /// Register a new user
    /// </summary>
    public async Task<LoginResponse> RegisterAsync(RegisterRequest request)
    {
        _logger.LogInformation("Registration attempt for email: {Email}", request.Email);

        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
        {
            _logger.LogWarning("Registration validation failed: Email or password is empty");
            throw new ArgumentException("Email and password are required");
        }

        var existingUser = await _dbContext.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
        if (existingUser != null)
        {
            _logger.LogWarning("Registration failed: User with email {Email} already exists", request.Email);
            throw new InvalidOperationException("User with this email already exists");
        }

        _logger.LogDebug("Hashing password for new user registration");
        string passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);

        var user = new User
        {
            Email = request.Email,
            FirstName = request.FirstName,
            LastName = request.LastName,
            PhoneNumber = request.PhoneNumber ?? string.Empty,
            CompanyName = request.CompanyName ?? string.Empty,
            CompanyAddress = request.CompanyAddress ?? string.Empty,
            CompanyCity = request.CompanyCity ?? string.Empty,
            CompanyState = request.CompanyState ?? string.Empty,
            CompanyZipCode = request.CompanyZipCode ?? string.Empty,
            CompanyCountry = request.CompanyCountry ?? string.Empty,
            PasswordHash = passwordHash,
            CreatedAt = DateTime.UtcNow,
            ModifiedAt = DateTime.UtcNow,
            IsActive = true
        };

        _dbContext.Users.Add(user);
        _logger.LogDebug("Saving new registered user to database");
        await _dbContext.SaveChangesAsync();
        _logger.LogInformation("Registration successful for user: {UserId} with email: {Email}", user.Id, user.Email);

        // Send welcome email
        _logger.LogDebug("Sending welcome email to {Email}", user.Email);
        var emailSent = await _emailService.SendWelcomeEmailAsync(user.Email, user.FirstName, user.LastName);
        if (!emailSent)
        {
            _logger.LogWarning("Failed to send welcome email to {Email}", user.Email);
        }
        else
        {
            _logger.LogInformation("Welcome email sent successfully to {Email}", user.Email);
        }

        // Generate token
        var token = GenerateToken(user.Id, user.Email);
        var expiresAt = DateTime.UtcNow.AddHours(24); // Token expires in 24 hours

        return new LoginResponse(
            user.Id,
            user.Email,
            user.FirstName,
            user.LastName,
            token,
            expiresAt
        );
    }

    /// <summary>
    /// Verify a password against a hashed password
    /// </summary>
    public bool VerifyPassword(string password, string passwordHash)
    {
        try
        {
            return BCrypt.Net.BCrypt.Verify(password, passwordHash);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error verifying password");
            return false;
        }
    }

    /// <summary>
    /// Generate a JWT token for a user
    /// </summary>
    public string GenerateToken(int userId, string email)
    {
        _logger.LogDebug("Generating JWT token for user: {UserId}", userId);

        var jwtSettings = _configuration.GetSection("JwtSettings");
        var secretKey = jwtSettings["SecretKey"];
        var issuer = jwtSettings["Issuer"] ?? "InvoiceBackendAPI";
        var audience = jwtSettings["Audience"] ?? "InvoiceBackendClients";
        var expirationMinutes = int.Parse(jwtSettings["ExpirationMinutes"] ?? "1440");

        if (string.IsNullOrWhiteSpace(secretKey))
        {
            _logger.LogError("JWT secret key not configured");
            throw new InvalidOperationException("JWT secret key is not configured");
        }

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
            new Claim(ClaimTypes.Email, email),
            // new Claim("sub", userId.ToString()),
        };

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(expirationMinutes),
            signingCredentials: credentials
        );

        var tokenHandler = new JwtSecurityTokenHandler();
        var tokenString = tokenHandler.WriteToken(token);

        _logger.LogDebug("JWT token generated successfully for user: {UserId}", userId);
        return tokenString;
    }

    /// <summary>
    /// Login or register a user with Google OAuth
    /// </summary>
    public async Task<LoginResponse> GoogleLoginAsync(GoogleLoginRequest request)
    {
        _logger.LogInformation("Google login attempt");

        // Validate request
        if (string.IsNullOrWhiteSpace(request.IdToken))
        {
            _logger.LogWarning("Google login validation failed: ID token is empty");
            throw new ArgumentException("Google ID token is required");
        }

        // Validate Google token
        var tokenInfo = await _googleTokenValidator.ValidateTokenAsync(request.IdToken);

        _logger.LogDebug("Google token validated for email: {Email}", tokenInfo.Email);

        // Check if user exists
        var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Email == tokenInfo.Email);

        if (user == null)
        {
            _logger.LogInformation("Creating new user from Google login: {Email}", tokenInfo.Email);

            // Generate a random password for Google OAuth users (they won't use it)
            string randomPassword = BCrypt.Net.BCrypt.HashPassword(Guid.NewGuid().ToString());
            DateTime now = DateTime.UtcNow;
            user = new User
            {
                Email = tokenInfo.Email,
                FirstName = tokenInfo.GivenName ?? tokenInfo.Name?.Split(' ').FirstOrDefault() ?? "",
                LastName = tokenInfo.FamilyName ?? tokenInfo.Name?.Split(' ').Skip(1).FirstOrDefault() ?? "",
                PhoneNumber = string.Empty,
                CompanyName = string.Empty,
                CompanyAddress = string.Empty,
                CompanyCity = string.Empty,
                CompanyState = string.Empty,
                CompanyZipCode = string.Empty,
                CompanyCountry = string.Empty,
                PasswordHash = randomPassword,
                CreatedAt = now,
                ModifiedAt = now,
                IsActive = true
            };

            _dbContext.Users.Add(user);
            await _dbContext.SaveChangesAsync();
            _logger.LogInformation("New Google user created: {UserId} with email: {Email}", user.Id, user.Email);

            // Send welcome email to new Google user
            _logger.LogDebug("Sending welcome email to new Google user {Email}", user.Email);
            var emailSent = await _emailService.SendWelcomeEmailAsync(user.Email, user.FirstName, user.LastName);
            if (!emailSent)
            {
                _logger.LogWarning("Failed to send welcome email to {Email}", user.Email);
            }
            else
            {
                _logger.LogInformation("Welcome email sent successfully to {Email}", user.Email);
            }
        }
        else
        {
            // Check if user is active
            if (!user.IsActive)
            {
                _logger.LogWarning("Google login failed: User {Email} is not active", user.Email);
                throw new InvalidOperationException("User account is inactive");
            }

            _logger.LogInformation("Existing user logged in via Google: {UserId} with email: {Email}", user.Id, user.Email);
        }

        // Generate JWT token
        var token = GenerateToken(user.Id, user.Email);
        var expiresAt = DateTime.UtcNow.AddHours(24); // Token expires in 24 hours

        return new LoginResponse(
            user.Id,
            user.Email,
            user.FirstName,
            user.LastName,
            token,
            expiresAt
        );
    }
}
