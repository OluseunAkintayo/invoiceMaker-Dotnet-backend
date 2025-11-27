using invoice_backend.Data;
using invoice_backend.Models;
using Microsoft.EntityFrameworkCore;

namespace invoice_backend.Services.UserService;

public class UserService : IUserService
{
    private readonly ApplicationDbContext _dbContext;
    private readonly ILogger<UserService> _logger;

    public UserService(ApplicationDbContext dbContext, ILogger<UserService> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<List<UserDto>> GetAllUsersAsync()
    {
        _logger.LogDebug("Fetching all users from database");
        var users = await _dbContext.Users
            .Select(u => MapToUserDto(u))
            .ToListAsync();
        _logger.LogDebug("Fetched {Count} users from database", users.Count);

        return users;
    }

    public async Task<UserDto?> GetUserByIdAsync(int id)
    {
        _logger.LogDebug("Fetching user with ID: {UserId} from database", id);
        var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == id);
        if (user == null)
        {
            _logger.LogDebug("User with ID: {UserId} not found in database", id);
            return null;
        }

        return MapToUserDto(user);
    }

    /// <summary>
    /// Create a new user through the admin interface (without authentication flow)
    /// For user registration with authentication, use AuthService.RegisterAsync instead
    /// </summary>
    public async Task<UserDto> CreateUserAsync(CreateUserRequest request)
    {
        // Validate request
        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
        {
            _logger.LogWarning("CreateUser validation failed: Email or password is empty");
            throw new ArgumentException("Email and password are required");
        }

        _logger.LogDebug("Checking if user with email {Email} already exists", request.Email);
        // Check if user already exists
        var existingUser = await _dbContext.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
        if (existingUser != null)
        {
            _logger.LogWarning("User creation failed: User with email {Email} already exists", request.Email);
            throw new InvalidOperationException("User with this email already exists");
        }

        _logger.LogDebug("Hashing password for new user");
        // Hash password using BCrypt
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
        _logger.LogDebug("Saving new user to database");
        await _dbContext.SaveChangesAsync();
        _logger.LogInformation("Successfully created user with ID: {UserId} and email: {Email}", user.Id, user.Email);

        return MapToUserDto(user);
    }

    public async Task<UserDto?> UpdateUserAsync(int id, UpdateUserRequest request)
    {
        _logger.LogDebug("Fetching user with ID: {UserId} for update", id);
        var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == id);
        if (user == null)
        {
            _logger.LogDebug("User with ID: {UserId} not found for update", id);
            return null;
        }

        // Update fields
        if (!string.IsNullOrWhiteSpace(request.FirstName))
            user.FirstName = request.FirstName;

        if (!string.IsNullOrWhiteSpace(request.LastName))
            user.LastName = request.LastName;

        if (!string.IsNullOrWhiteSpace(request.PhoneNumber))
            user.PhoneNumber = request.PhoneNumber;

        if (!string.IsNullOrWhiteSpace(request.CompanyName))
            user.CompanyName = request.CompanyName;

        if (!string.IsNullOrWhiteSpace(request.CompanyAddress))
            user.CompanyAddress = request.CompanyAddress;

        if (!string.IsNullOrWhiteSpace(request.CompanyCity))
            user.CompanyCity = request.CompanyCity;

        if (!string.IsNullOrWhiteSpace(request.CompanyState))
            user.CompanyState = request.CompanyState;

        if (!string.IsNullOrWhiteSpace(request.CompanyZipCode))
            user.CompanyZipCode = request.CompanyZipCode;

        if (!string.IsNullOrWhiteSpace(request.CompanyCountry))
            user.CompanyCountry = request.CompanyCountry;

        user.ModifiedAt = DateTime.UtcNow;

        _logger.LogDebug("Saving updates for user with ID: {UserId}", id);
        await _dbContext.SaveChangesAsync();
        _logger.LogInformation("Successfully updated user with ID: {UserId}", id);

        return MapToUserDto(user);
    }

    public async Task<bool> DeleteUserAsync(int id)
    {
        _logger.LogDebug("Fetching user with ID: {UserId} for deletion", id);
        var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == id);
        if (user == null)
        {
            _logger.LogDebug("User with ID: {UserId} not found for deletion", id);
            return false;
        }

        _dbContext.Users.Remove(user);
        _logger.LogDebug("Deleting user with ID: {UserId} from database", id);
        await _dbContext.SaveChangesAsync();
        _logger.LogInformation("Successfully deleted user with ID: {UserId}", id);

        return true;
    }

    /// <summary>
    /// Maps User entity to UserDto
    /// </summary>
    private static UserDto MapToUserDto(User user)
    {
        return new UserDto(
            user.Id,
            user.Email,
            user.FirstName,
            user.LastName,
            user.PhoneNumber,
            user.CompanyName,
            user.CompanyAddress,
            user.CompanyCity,
            user.CompanyState,
            user.CompanyZipCode,
            user.CompanyCountry,
            user.IsActive,
            user.CreatedAt,
            user.ModifiedAt
        );
    }
}
