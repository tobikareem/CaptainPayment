using System.ComponentModel.DataAnnotations;

namespace CaptainPayment.Core.Models;

/// <summary>
/// Create subscription request model.
/// </summary>
public class CreateSubscriptionRequest
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string FullName { get; set; } = string.Empty;

    [Required]
    public string PaymentMethodId { get; set; } = string.Empty;

    [Required]
    public string PriceId { get; set; } = string.Empty;

    public int? TrialPeriodDays { get; set; }
    public Dictionary<string, string>? Metadata { get; set; }
    public string? CustomerId { get; set; }
    public long? Quantity { get; set; }
}