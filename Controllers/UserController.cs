using invoice_backend.Models;
using invoice_backend.Services.UserService;
using Microsoft.AspNetCore.Mvc;

namespace invoice_backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UserController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly ILogger<UserController> _logger;

    public UserController(IUserService userService, ILogger<UserController> logger)
    {
        _userService = userService;
        _logger = logger;
    }

    /// <summary>
    /// Get all users
    /// </summary>
    /// <returns>List of all users</returns>
    [HttpGet]
    public async Task<ActionResult<List<UserDto>>> GetAllUsers()
    {
        _logger.LogInformation("Retrieving all users");
        var users = await _userService.GetAllUsersAsync();
        _logger.LogInformation("Retrieved {UserCount} users", users.Count);
        return Ok(users);
    }

    /// <summary>
    /// Get user by ID
    /// </summary>
    /// <param name="id">User ID</param>
    /// <returns>User details</returns>
    [HttpGet("{id}")]
    public async Task<ActionResult<UserDto>> GetUserById(int id)
    {
        _logger.LogInformation("Retrieving user with ID: {UserId}", id);
        var user = await _userService.GetUserByIdAsync(id);
        if (user == null)
        {
            _logger.LogWarning("User with ID: {UserId} not found", id);
            return NotFound(new { message = "User not found" });
        }

        _logger.LogInformation("Successfully retrieved user with ID: {UserId}", id);
        return Ok(user);
    }

    /// <summary>
    /// Create a new user
    /// </summary>
    /// <param name="request">User creation request</param>
    /// <returns>Created user</returns>
    [HttpPost]
    public async Task<ActionResult<UserDto>> CreateUser([FromBody] CreateUserRequest request)
    {
        _logger.LogInformation("Creating new user with email: {Email}", request.Email);
        try
        {
            var userDto = await _userService.CreateUserAsync(request);
            _logger.LogInformation("Successfully created user with ID: {UserId}", userDto.Id);
            return CreatedAtAction(nameof(GetUserById), new { id = userDto.Id }, userDto);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid argument when creating user: {Message}", ex.Message);
            return BadRequest(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Invalid operation when creating user: {Message}", ex.Message);
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Update user information
    /// </summary>
    /// <param name="id">User ID</param>
    /// <param name="request">Update request</param>
    /// <returns>Updated user</returns>
    [HttpPut("{id}")]
    public async Task<ActionResult<UserDto>> UpdateUser(int id, [FromBody] UpdateUserRequest request)
    {
        _logger.LogInformation("Updating user with ID: {UserId}", id);
        var userDto = await _userService.UpdateUserAsync(id, request);
        if (userDto == null)
        {
            _logger.LogWarning("User with ID: {UserId} not found for update", id);
            return NotFound(new { message = "User not found" });
        }

        _logger.LogInformation("Successfully updated user with ID: {UserId}", id);
        return Ok(userDto);
    }

    /// <summary>
    /// Delete user by ID
    /// </summary>
    /// <param name="id">User ID</param>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteUser(int id)
    {
        _logger.LogInformation("Deleting user with ID: {UserId}", id);
        var deleted = await _userService.DeleteUserAsync(id);
        if (!deleted)
        {
            _logger.LogWarning("User with ID: {UserId} not found for deletion", id);
            return NotFound(new { message = "User not found" });
        }

        _logger.LogInformation("Successfully deleted user with ID: {UserId}", id);
        return NoContent();
    }
}
