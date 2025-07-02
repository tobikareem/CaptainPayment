namespace CaptainPayment.Core.Config;

public class StripeSubscriptionDefaults
{
    public int? DefaultTrialDays { get; set; }
    public bool SaveDefaultPaymentMethod { get; set; } = true;
    public List<string> DefaultExpand { get; set; } = new() { "latest_invoice.payment_intent" };
}