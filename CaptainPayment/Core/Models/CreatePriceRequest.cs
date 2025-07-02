namespace CaptainPayment.Core.Models;

public class CreatePriceRequest
{
    public string ProductId { get; set; } = string.Empty;
    public string Currency { get; set; } = "usd";
    public decimal UnitAmount { get; set; }
    public bool? Active { get; set; }
    public Dictionary<string, string> Metadata { get; set; } = new();
    public string Nickname { get; set; } = string.Empty;
    public RecurringOptions Recurring { get; set; } = new();
    public string TaxBehavior { get; set; } = "unspecified";
    public List<TierOptions> Tiers { get; set; } = new();
    public string TiersMode { get; set; } = "graduated";
    public TransformQuantityOptions TransformQuantity { get; set; } = new();
    public bool? TransferLookupKey { get; set; }
}