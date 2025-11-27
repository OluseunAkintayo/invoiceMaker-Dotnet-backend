using invoice_backend.Models;
using System.Net;
using System.Net.Mail;

namespace invoice_backend.Services.Email;

/// <summary>
/// Service for sending emails via SMTP
/// </summary>
public class EmailService : IEmailService
{
    private readonly EmailConfiguration _emailConfiguration;
    private readonly ILogger<EmailService> _logger;

    public EmailService(EmailConfiguration emailConfiguration, ILogger<EmailService> logger)
    {
        _emailConfiguration = emailConfiguration;
        _logger = logger;
    }

    /// <summary>
    /// Send a welcome email to a new user
    /// </summary>
    public async Task<bool> SendWelcomeEmailAsync(string email, string firstName, string lastName)
    {
        try
        {
            var subject = "Welcome to Invoice Backend";
            var body = GenerateWelcomeEmailBody(firstName, lastName);

            return await SendEmailAsync(email, subject, body, isHtml: true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending welcome email to {Email}", email);
            return false;
        }
    }

    /// <summary>
    /// Send a generic email
    /// </summary>
    public async Task<bool> SendEmailAsync(string to, string subject, string body, bool isHtml = true)
    {
        try
        {
            _logger.LogInformation("Sending email to {To} with subject {Subject}", to, subject);

            using (var smtpClient = new SmtpClient(_emailConfiguration.SmtpHost, _emailConfiguration.SmtpPort))
            {
                smtpClient.EnableSsl = _emailConfiguration.EnableSsl;
                smtpClient.Credentials = new NetworkCredential(
                    _emailConfiguration.SmtpUsername,
                    _emailConfiguration.SmtpPassword
                );

                using (var mailMessage = new MailMessage(
                    new MailAddress(_emailConfiguration.FromAddress, _emailConfiguration.FromName),
                    new MailAddress(to)))
                {
                    mailMessage.Subject = subject;
                    mailMessage.Body = body;
                    mailMessage.IsBodyHtml = isHtml;

                    await smtpClient.SendMailAsync(mailMessage);
                }
            }

            _logger.LogInformation("Email sent successfully to {To}", to);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending email to {To}", to);
            return false;
        }
    }

    /// <summary>
    /// Generate welcome email HTML body
    /// </summary>
    private string GenerateWelcomeEmailBody(string firstName, string lastName)
    {
        return $@"
        <!DOCTYPE html>
        <html lang=""en"">
        <head>
            <meta charset=""UTF-8"">
            <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
            <title>Welcome to Invoice Backend</title>
            <style>
                * {{ margin: 0; padding: 0; box-sizing: border-box; }}
                body {{
                    font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, 'Helvetica Neue', Arial, sans-serif;
                    line-height: 1.6;
                    color: #333;
                    background-color: #f5f5f5;
                }}
                .email-wrapper {{
                    width: 100%;
                    background-color: #f5f5f5;
                    padding: 40px 0;
                }}
                .container {{
                    max-width: 600px;
                    margin: 0 auto;
                    background-color: #ffffff;
                    border-radius: 8px;
                    box-shadow: 0 2px 8px rgba(0, 0, 0, 0.1);
                    overflow: hidden;
                }}
                .header {{
                    background: linear-gradient(135deg, #007bff 0%, #0056b3 100%);
                    color: white;
                    padding: 60px 30px;
                    text-align: center;
                }}
                .header h1 {{
                    font-size: 32px;
                    margin-bottom: 10px;
                    font-weight: 700;
                }}
                .header p {{
                    font-size: 16px;
                    opacity: 0.9;
                }}
                .content {{
                    padding: 40px 30px;
                }}
                .greeting {{
                    font-size: 18px;
                    color: #333;
                    margin-bottom: 20px;
                }}
                .greeting strong {{
                    color: #007bff;
                }}
                .intro-text {{
                    color: #666;
                    margin-bottom: 30px;
                    font-size: 15px;
                }}
                .features {{
                    margin: 30px 0;
                    padding: 20px;
                    background-color: #f9f9f9;
                    border-left: 4px solid #007bff;
                    border-radius: 4px;
                }}
                .features-title {{
                    font-size: 16px;
                    font-weight: 600;
                    color: #333;
                    margin-bottom: 15px;
                }}
                .feature-item {{
                    display: flex;
                    align-items: flex-start;
                    margin-bottom: 12px;
                    color: #555;
                    font-size: 14px;
                }}
                .feature-item:last-child {{
                    margin-bottom: 0;
                }}
                .feature-icon {{
                    color: #007bff;
                    font-weight: bold;
                    margin-right: 10px;
                    font-size: 16px;
                }}
                .cta-section {{
                    margin: 30px 0;
                    text-align: center;
                }}
                .cta-button {{
                    display: inline-block;
                    background-color: #007bff;
                    color: white;
                    padding: 14px 40px;
                    text-decoration: none;
                    border-radius: 6px;
                    font-weight: 600;
                    font-size: 15px;
                    transition: background-color 0.3s ease;
                }}
                .cta-button:hover {{
                    background-color: #0056b3;
                }}
                .support-section {{
                    margin-top: 30px;
                    padding: 20px;
                    background-color: #f0f8ff;
                    border-radius: 4px;
                    text-align: center;
                }}
                .support-text {{
                    color: #666;
                    font-size: 14px;
                }}
                .support-text a {{
                    color: #007bff;
                    text-decoration: none;
                }}
                .divider {{
                    height: 1px;
                    background-color: #e0e0e0;
                    margin: 30px 0;
                }}
                .footer {{
                    background-color: #f9f9f9;
                    padding: 30px;
                    text-align: center;
                    border-top: 1px solid #e0e0e0;
                }}
                .footer-text {{
                    color: #999;
                    font-size: 13px;
                    line-height: 1.8;
                }}
                .footer-links {{
                    margin-top: 15px;
                    font-size: 12px;
                }}
                .footer-links a {{
                    color: #007bff;
                    text-decoration: none;
                    margin: 0 10px;
                }}
            </style>
        </head>
        <body>
            <div class=""email-wrapper"">
                <div class=""container"">
                    <!-- Header -->
                    <div class=""header"">
                        <h1>🎉 Welcome to Invoice Backend!</h1>
                        <p>Your professional invoicing solution awaits</p>
                    </div>

                    <!-- Main Content -->
                    <div class=""content"">
                        <div class=""greeting"">
                            Hello <strong>{firstName} {lastName}</strong>,
                        </div>

                        <p class=""intro-text"">
                            Thank you for joining our community! We're thrilled to have you on board. Your account is now active and ready to use. Let's get you started with Invoice Backend—the platform designed to simplify your invoicing process.
                        </p>

                        <!-- Features Section -->
                        <div class=""features"">
                            <div class=""features-title"">What You Can Do:</div>
                            <div class=""feature-item"">
                                <span class=""feature-icon"">✓</span>
                                <span>Create and manage professional invoices in minutes</span>
                            </div>
                            <div class=""feature-item"">
                                <span class=""feature-icon"">✓</span>
                                <span>Track invoice status and monitor payment progress</span>
                            </div>
                            <div class=""feature-item"">
                                <span class=""feature-icon"">✓</span>
                                <span>Organize and manage your client information seamlessly</span>
                            </div>
                            <div class=""feature-item"">
                                <span class=""feature-icon"">✓</span>
                                <span>Generate insightful reports to grow your business</span>
                            </div>
                        </div>

                        <!-- Call-to-Action -->
                        <div class=""cta-section"">
                            <a href=""https://invoice-backend.com/dashboard"" class=""cta-button"">Get Started Now</a>
                        </div>

                        <!-- Support Section -->
                        <div class=""support-section"">
                            <p class=""support-text"">
                                Questions or need help getting started? Our support team is here for you!
                                <a href=""mailto:support@invoice-backend.com"">Contact support</a>
                            </p>
                        </div>

                        <div class=""divider""></div>

                        <p style=""color: #666; font-size: 14px;"">
                            Happy invoicing!<br>
                            <strong>The Invoice Backend Team</strong>
                        </p>
                    </div>

                    <!-- Footer -->
                    <div class=""footer"">
                        <p class=""footer-text"">
                            Invoice Backend — Your Professional Invoicing Solution<br>
                            Designed to make invoicing simple, fast, and professional.
                        </p>
                        <div class=""footer-links"">
                            <a href=""https://invoice-backend.com"">Website</a>
                            <a href=""https://invoice-backend.com/docs"">Documentation</a>
                            <a href=""https://invoice-backend.com/contact"">Contact Us</a>
                        </div>
                        <p class=""footer-text"" style=""margin-top: 15px; font-size: 11px;"">
                            &copy; 2025 Invoice Backend. All rights reserved.<br>
                            You received this email because you signed up for an Invoice Backend account.
                        </p>
                    </div>
                </div>
            </div>
        </body>
        </html>";
    }
}
