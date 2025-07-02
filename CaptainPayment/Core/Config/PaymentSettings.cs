namespace CaptainPayment.Core.Config;

public class PaymentSettings
{
    public const string SectionName = "Payment";

    public StripeSettings Stripe { get; set; } = new();
}