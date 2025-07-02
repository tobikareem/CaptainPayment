namespace CaptainPayment.Core.Models;

public class PriceListRequest
{
    public int? Limit { get; set; }
    public string? StartingAfter { get; set; }
    public string? EndingBefore { get; set; }
    public string? ProductId { get; set; }
    public string? Currency { get; set; }
    public string? Type { get; set; } // "one_time" or "recurring"
    public bool? Active { get; set; }
    public DateRangeRequest? Created { get; set; }
}
