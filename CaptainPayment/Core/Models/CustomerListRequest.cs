namespace CaptainPayment.Core.Models;

public class CustomerListRequest
{
    public int? Limit { get; set; }
    public string? StartingAfter { get; set; }
    public string? EndingBefore { get; set; }
    public string? Email { get; set; }
    public DateRangeRequest? Created { get; set; }
}

public class DateRangeRequest
{
    public DateTime? GreaterThan { get; set; }
    public DateTime? LessThan { get; set; }
}