namespace invoice_backend.Models;

/// <summary>
/// Email send request DTO
/// </summary>
public record EmailRequest(
    string To,
    string Subject,
    string Body,
    bool IsHtml = true
);

/// <summary>
/// Welcome email request DTO
/// </summary>
public record WelcomeEmailRequest(
    string Email,
    string FirstName,
    string LastName
);

/// <summary>
/// Email response DTO
/// </summary>
public record EmailResponse(
    bool Success,
    string Message
);
