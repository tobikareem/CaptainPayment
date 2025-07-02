namespace CaptainPayment.Core.Models;

public class RecurringOptions
{
    public string Interval { get; set; }  = string.Empty;
    public long? IntervalCount { get; set; }
    public string AggregateUsage { get; set; } = string.Empty;
    public string UsageType { get; set; } = string.Empty;
}