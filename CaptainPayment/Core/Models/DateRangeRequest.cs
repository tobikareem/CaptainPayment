namespace CaptainPayment.Core.Models;

public class DateRangeRequest
{
    public DateTime? GreaterThan { get; set; }
    public DateTime? LessThan { get; set; }
}