namespace CaptainPayment.Core.Models;

public class ProductListRequest
{
    public int? Limit { get; set; }
    public string? StartingAfter { get; set; }
    public string? EndingBefore { get; set; }
    public bool? Active { get; set; }
    public string? Url { get; set; }
    public List<string>? Ids { get; set; }
    public bool? Shippable { get; set; }
    public DateRangeRequest? Created { get; set; }
}
