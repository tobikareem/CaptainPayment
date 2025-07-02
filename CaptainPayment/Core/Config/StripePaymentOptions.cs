namespace CaptainPayment.Core.Config;

public class StripePaymentOptions
{
    public List<string> PaymentMethodTypes { get; set; } = new() { "card" };
    public string DefaultCurrency { get; set; } = "usd";
    public bool SavePaymentMethod { get; set; } = true;
    public string PaymentBehavior { get; set; } = "default_incomplete";
}