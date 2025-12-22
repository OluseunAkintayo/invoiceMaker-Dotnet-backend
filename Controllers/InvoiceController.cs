using invoice_backend.Models;
using invoice_backend.Models.Enums;
using invoice_backend.Services.Invoice;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace invoice_backend.Controllers;

/// <summary>
/// Controller for managing invoices
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class InvoiceController(IInvoiceService invoiceService, ILogger<InvoiceController> logger) : ControllerBase
{
    private readonly IInvoiceService _invoiceService = invoiceService;
    private readonly ILogger<InvoiceController> _logger = logger;

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
    /// Get all invoices for the authenticated user
    /// </summary>
    /// <returns>List of invoices</returns>
    [HttpGet]
    public async Task<ActionResult<InvoiceListResponse>> GetAllInvoices()
    {
        int userId = GetUserIdFromClaims();
        if (userId == 0)
        {
            _logger.LogWarning("Unauthorized access attempt to invoices - no valid user ID in claims");
            return Unauthorized(new InvoiceListResponse(
                Success: false,
                Message: "Unauthorized - invalid or missing authentication",
                Data: new List<InvoiceDto>(),
                TotalCount: 0
            ));
        }

        _logger.LogInformation("Retrieving all invoices for user {UserId}", userId);

        try
        {
            var invoices = await _invoiceService.GetAllInvoicesByUserAsync(userId);
            _logger.LogInformation("Retrieved {InvoiceCount} invoices for user {UserId}", invoices.Count, userId);

            return Ok(new InvoiceListResponse(
                Success: true,
                Message: "Invoices retrieved successfully",
                Data: invoices,
                TotalCount: invoices.Count
            ));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving invoices for user {UserId}", userId);
            return StatusCode(StatusCodes.Status500InternalServerError, new InvoiceListResponse(
                Success: false,
                Message: "An error occurred while retrieving invoices",
                Data: new List<InvoiceDto>(),
                TotalCount: 0
            ));
        }
    }

    /// <summary>
    /// Get invoice by ID
    /// </summary>
    /// <param name="id">Invoice ID</param>
    /// <returns>Invoice details</returns>
    [HttpGet("{id}")]
    public async Task<ActionResult<InvoiceResponse>> GetInvoiceById(int id)
    {
        int userId = GetUserIdFromClaims();
        if (userId == 0)
        {
            _logger.LogWarning("Unauthorized access attempt to invoice {InvoiceId}", id);
            return Unauthorized(new InvoiceResponse(
                Success: false,
                Message: "Unauthorized - invalid or missing authentication"
            ));
        }

        _logger.LogInformation("Retrieving invoice {InvoiceId} for user {UserId}", id, userId);

        try
        {
            var invoice = await _invoiceService.GetInvoiceByIdAsync(id, userId);
            if (invoice == null)
            {
                _logger.LogWarning("Invoice {InvoiceId} not found for user {UserId}", id, userId);
                return NotFound(new InvoiceResponse(
                    Success: false,
                    Message: "Invoice not found"
                ));
            }

            return Ok(new InvoiceResponse(
                Success: true,
                Message: "Invoice retrieved successfully",
                Data: invoice
            ));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving invoice {InvoiceId}", id);
            return StatusCode(StatusCodes.Status500InternalServerError, new InvoiceResponse(
                Success: false,
                Message: "An error occurred while retrieving the invoice"
            ));
        }
    }

    /// <summary>
    /// Create a new invoice
    /// </summary>
    /// <param name="request">Invoice creation request</param>
    /// <returns>Created invoice</returns>
    [HttpPost]
    public async Task<ActionResult<InvoiceResponse>> CreateInvoice([FromBody] CreateInvoiceRequest request)
    {
        int userId = GetUserIdFromClaims();
        if (userId == 0)
        {
            _logger.LogWarning("Unauthorized access attempt to create invoice");
            return Unauthorized(new InvoiceResponse(
                Success: false,
                Message: "Unauthorized - invalid or missing authentication"
            ));
        }

        _logger.LogInformation("Creating invoice for user {UserId}", userId);

        try
        {
            var invoice = await _invoiceService.CreateInvoiceAsync(request, userId);
            _logger.LogInformation("Invoice {InvoiceNumber} created successfully with ID {InvoiceId}", invoice.InvoiceNumber, invoice.Id);

            return CreatedAtAction(nameof(GetInvoiceById), new { id = invoice.Id }, new InvoiceResponse(
                Success: true,
                Message: "Invoice created successfully",
                Data: invoice
            ));
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Validation error when creating invoice: {Message}", ex.Message);
            return BadRequest(new InvoiceResponse(
                Success: false,
                Message: ex.Message
            ));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating invoice for user {UserId}", userId);
            return StatusCode(StatusCodes.Status500InternalServerError, new InvoiceResponse(
                Success: false,
                Message: "An error occurred while creating the invoice"
            ));
        }
    }

    /// <summary>
    /// Update an invoice
    /// </summary>
    /// <param name="id">Invoice ID</param>
    /// <param name="request">Update request</param>
    /// <returns>Updated invoice</returns>
    [HttpPut("{id}")]
    public async Task<ActionResult<InvoiceResponse>> UpdateInvoice(int id, [FromBody] UpdateInvoiceRequest request)
    {
        int userId = GetUserIdFromClaims();
        if (userId == 0)
        {
            _logger.LogWarning("Unauthorized access attempt to update invoice {InvoiceId}", id);
            return Unauthorized(new InvoiceResponse(
                Success: false,
                Message: "Unauthorized - invalid or missing authentication"
            ));
        }

        _logger.LogInformation("Updating invoice {InvoiceId} for user {UserId}", id, userId);

        try
        {
            var invoice = await _invoiceService.UpdateInvoiceAsync(id, request, userId);
            if (invoice == null)
            {
                _logger.LogWarning("Invoice {InvoiceId} not found for update", id);
                return NotFound(new InvoiceResponse(
                    Success: false,
                    Message: "Invoice not found"
                ));
            }

            _logger.LogInformation("Invoice {InvoiceId} updated successfully", id);
            return Ok(new InvoiceResponse(
                Success: true,
                Message: "Invoice updated successfully",
                Data: invoice
            ));
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Validation error when updating invoice: {Message}", ex.Message);
            return BadRequest(new InvoiceResponse(
                Success: false,
                Message: ex.Message
            ));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating invoice {InvoiceId}", id);
            return StatusCode(StatusCodes.Status500InternalServerError, new InvoiceResponse(
                Success: false,
                Message: "An error occurred while updating the invoice"
            ));
        }
    }

    /// <summary>
    /// Delete an invoice
    /// </summary>
    /// <param name="id">Invoice ID</param>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteInvoice(int id)
    {
        int userId = GetUserIdFromClaims();
        if (userId == 0)
        {
            _logger.LogWarning("Unauthorized access attempt to delete invoice {InvoiceId}", id);
            return Unauthorized(new InvoiceResponse(
                Success: false,
                Message: "Unauthorized - invalid or missing authentication"
            ));
        }

        _logger.LogInformation("Deleting invoice {InvoiceId} for user {UserId}", id, userId);

        try
        {
            var deleted = await _invoiceService.DeleteInvoiceAsync(id, userId);
            if (!deleted)
            {
                _logger.LogWarning("Invoice {InvoiceId} not found for deletion", id);
                return NotFound(new InvoiceResponse(
                    Success: false,
                    Message: "Invoice not found"
                ));
            }

            _logger.LogInformation("Invoice {InvoiceId} deleted successfully", id);
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting invoice {InvoiceId}", id);
            return StatusCode(StatusCodes.Status500InternalServerError, new InvoiceResponse(
                Success: false,
                Message: "An error occurred while deleting the invoice"
            ));
        }
    }

    /// <summary>
    /// Get invoices by status
    /// </summary>
    /// <param name="status">Invoice status</param>
    /// <returns>List of invoices with specified status</returns>
    [HttpGet("status/{status}")]
    public async Task<ActionResult<InvoiceListResponse>> GetInvoicesByStatus(InvoiceStatus status)
    {
        int userId = GetUserIdFromClaims();
        if (userId == 0)
        {
            _logger.LogWarning("Unauthorized access attempt to get invoices by status");
            return Unauthorized(new InvoiceListResponse(
                Success: false,
                Message: "Unauthorized - invalid or missing authentication",
                Data: new List<InvoiceDto>(),
                TotalCount: 0
            ));
        }

        _logger.LogInformation("Retrieving invoices with status {Status} for user {UserId}", status, userId);

        try
        {
            var invoices = await _invoiceService.GetInvoicesByStatusAsync(userId, status);
            _logger.LogInformation("Retrieved {InvoiceCount} invoices with status {Status}", invoices.Count, status);

            return Ok(new InvoiceListResponse(
                Success: true,
                Message: $"Invoices with status {status} retrieved successfully",
                Data: invoices,
                TotalCount: invoices.Count
            ));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving invoices with status {Status}", status);
            return StatusCode(StatusCodes.Status500InternalServerError, new InvoiceListResponse(
                Success: false,
                Message: "An error occurred while retrieving invoices",
                Data: new List<InvoiceDto>(),
                TotalCount: 0
            ));
        }
    }

    /// <summary>
    /// Update invoice status
    /// </summary>
    /// <param name="id">Invoice ID</param>
    /// <param name="status">New status</param>
    /// <returns>Updated invoice</returns>
    [HttpPatch("{id}/status")]
    public async Task<ActionResult<InvoiceResponse>> UpdateInvoiceStatus(int id, [FromQuery] InvoiceStatus status)
    {
        int userId = GetUserIdFromClaims();
        if (userId == 0)
        {
            _logger.LogWarning("Unauthorized access attempt to update status of invoice {InvoiceId}", id);
            return Unauthorized(new InvoiceResponse(
                Success: false,
                Message: "Unauthorized - invalid or missing authentication"
            ));
        }

        _logger.LogInformation("Updating status of invoice {InvoiceId} to {Status}", id, status);

        try
        {
            var invoice = await _invoiceService.UpdateInvoiceStatusAsync(id, status, userId);
            if (invoice == null)
            {
                _logger.LogWarning("Invoice {InvoiceId} not found for status update", id);
                return NotFound(new InvoiceResponse(
                    Success: false,
                    Message: "Invoice not found"
                ));
            }

            _logger.LogInformation("Invoice {InvoiceId} status updated to {Status}", id, status);
            return Ok(new InvoiceResponse(
                Success: true,
                Message: "Invoice status updated successfully",
                Data: invoice
            ));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating status of invoice {InvoiceId}", id);
            return StatusCode(StatusCodes.Status500InternalServerError, new InvoiceResponse(
                Success: false,
                Message: "An error occurred while updating the invoice status"
            ));
        }
    }
}
