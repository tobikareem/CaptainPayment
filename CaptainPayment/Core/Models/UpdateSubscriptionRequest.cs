namespace CaptainPayment.Core.Models;

public class UpdateSubscriptionRequest
{
    public string? NewPriceId { get; set; }
    public int? Quantity { get; set; }
    public bool? CancelAtPeriodEnd { get; set; }
    public long? TrialEnd { get; set; } // Unix timestamp if extending trial
    public Dictionary<string, string>? Metadata { get; set; }
}