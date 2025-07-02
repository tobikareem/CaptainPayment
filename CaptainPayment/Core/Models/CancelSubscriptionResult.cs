namespace CaptainPayment.Core.Models;

public class CancelSubscriptionResult
{
    public string SubscriptionId { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime? CanceledAt { get; set; }
    public DateTime? PeriodEnd { get; set; }
    public bool CancelAtPeriodEnd { get; set; }
}