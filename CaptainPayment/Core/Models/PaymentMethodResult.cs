namespace CaptainPayment.Core.Models;

public class PaymentMethodResult
{
    public string Id { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public DateTime Created { get; set; } = new DateTime();
    public string CustomerId { get; set; } = string.Empty;
    public CardDetails Card { get; set; } = new CardDetails();
    public BillingDetails BillingDetails { get; set; } = new BillingDetails();
    public Dictionary<string, string> Metadata { get; set; } = new Dictionary<string, string>();
    public bool LiveMode { get; set; }
}