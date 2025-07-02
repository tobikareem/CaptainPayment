namespace CaptainPayment.Core.Models;

public class UpdatePriceRequest
{
    public bool? Active { get; set; }
    public string? Nickname { get; set; }
    public string? TaxBehavior { get; set; }
    public string? TransferLookupKey { get; set; }
    public Dictionary<string, string>? Metadata { get; set; }
}
