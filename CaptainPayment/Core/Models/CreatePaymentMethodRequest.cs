namespace CaptainPayment.Core.Models;

public class CreatePaymentMethodRequest
{
    public string Type { get; set; } = "card";
    public CardOptions Card { get; set; } = new CardOptions();
    public BillingDetailsOptions BillingDetails { get; set; } = new BillingDetailsOptions();
    public Dictionary<string, string> Metadata { get; set; } = new Dictionary<string, string>();
}