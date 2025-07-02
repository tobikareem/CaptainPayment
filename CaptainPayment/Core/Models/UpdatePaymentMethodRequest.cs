namespace CaptainPayment.Core.Models;

public class UpdatePaymentMethodRequest
{
    public BillingDetails? BillingDetails { get; set; }
    public Dictionary<string, string>? Metadata { get; set; }
}
