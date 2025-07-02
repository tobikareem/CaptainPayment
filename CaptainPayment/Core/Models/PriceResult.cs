namespace CaptainPayment.Core.Models;

public class PriceResult
{
    public string Id { get; set; }
    public string ProductId { get; set; } = string.Empty;
    public bool Active { get; set; }
    public string BillingScheme { get; set; } = string.Empty;
    public DateTime Created { get; set; }
    public string Currency { get; set; } = string.Empty;
    public string Nickname { get; set; } = string.Empty;
    public decimal UnitAmount { get; set; }
    public decimal? UnitAmountDecimal { get; set; }
    public string Type { get; set; } = string.Empty;
    public string TaxBehavior { get; set; } = string.Empty;
    public Dictionary<string, string> Metadata { get; set; } = new();
    public RecurringDetails Recurring { get; set; } = new();
    public bool LiveMode { get; set; }
}