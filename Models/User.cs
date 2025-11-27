namespace invoice_backend.Models;

public class User : BaseEntity
{
    public string Email { get; set; } = string.Empty;

    public string FirstName { get; set; } = string.Empty;

    public string LastName { get; set; } = string.Empty;

    public string PhoneNumber { get; set; } = string.Empty;

    public string CompanyName { get; set; } = string.Empty;

    public string CompanyAddress { get; set; } = string.Empty;

    public string CompanyCity { get; set; } = string.Empty;

    public string CompanyState { get; set; } = string.Empty;

    public string CompanyZipCode { get; set; } = string.Empty;

    public string CompanyCountry { get; set; } = string.Empty;

    public string PasswordHash { get; set; } = string.Empty;
}
