namespace CaptainPayment.Core.Models;

public class CreatePaymentIntentOptions
{
    public string CustomerId { get; set; } = string.Empty;
    public Dictionary<string, string> Metadata { get; set; } = new();
    public string Description { get; set; } = string.Empty;
    public string ReceiptEmail { get; set; } = string.Empty;
}