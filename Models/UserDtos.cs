namespace invoice_backend.Models;

public record UserDto(
    int Id,
    string Email,
    string FirstName,
    string LastName,
    string PhoneNumber,
    string CompanyName,
    string CompanyAddress,
    string CompanyCity,
    string CompanyState,
    string CompanyZipCode,
    string CompanyCountry,
    bool IsActive,
    DateTime CreatedAt,
    DateTime ModifiedAt
);

public record CreateUserRequest(
    string Email,
    string Password,
    string FirstName,
    string LastName,
    string? PhoneNumber,
    string? CompanyName,
    string? CompanyAddress,
    string? CompanyCity,
    string? CompanyState,
    string? CompanyZipCode,
    string? CompanyCountry
);

public record UpdateUserRequest(
    string? FirstName,
    string? LastName,
    string? PhoneNumber,
    string? CompanyName,
    string? CompanyAddress,
    string? CompanyCity,
    string? CompanyState,
    string? CompanyZipCode,
    string? CompanyCountry
);

public record LoginRequest(
    string Email,
    string Password
);

public record LoginResponse(
    int Id,
    string Email,
    string FirstName,
    string LastName,
    string Token,
    DateTime ExpiresAt
);

public record RegisterRequest(
    string Email,
    string Password,
    string FirstName,
    string LastName,
    string? PhoneNumber,
    string? CompanyName,
    string? CompanyAddress,
    string? CompanyCity,
    string? CompanyState,
    string? CompanyZipCode,
    string? CompanyCountry
);

public record AuthResponse(
    bool Success,
    string Message,
    LoginResponse? Data = null
);

public record GoogleLoginRequest(
    string IdToken
);

public record GoogleTokenInfo(
    string Sub,
    string Email,
    string Name,
    string? Picture,
    string? GivenName,
    string? FamilyName,
    bool EmailVerified
);
