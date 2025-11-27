namespace invoice_backend.Services.Email;

/// <summary>
/// Interface for email service operations
/// </summary>
public interface IEmailService
{
    /// <summary>
    /// Send a welcome email to a new user
    /// </summary>
    /// <param name="email">User email address</param>
    /// <param name="firstName">User first name</param>
    /// <param name="lastName">User last name</param>
    /// <returns>Task representing the async operation</returns>
    Task<bool> SendWelcomeEmailAsync(string email, string firstName, string lastName);

    /// <summary>
    /// Send a generic email
    /// </summary>
    /// <param name="to">Recipient email address</param>
    /// <param name="subject">Email subject</param>
    /// <param name="body">Email body (can be HTML or plain text)</param>
    /// <param name="isHtml">Whether the body is HTML formatted</param>
    /// <returns>Task representing the async operation</returns>
    Task<bool> SendEmailAsync(string to, string subject, string body, bool isHtml = true);
}
