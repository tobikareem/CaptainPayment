namespace CaptainPayment.Core.Config;

public class StripeSettings
{
    public string PublishableKey { get; set; } = string.Empty;
    public string SecretKey { get; set; } = string.Empty;
    public string WebhookSecret { get; set; } = string.Empty;

    public string ProviderName { get; set; } = "Stripe";
    
    public StripePaymentOptions PaymentOptions { get; set; } = new();
    
    public StripeSubscriptionDefaults SubscriptionDefaults { get; set; } = new();

}