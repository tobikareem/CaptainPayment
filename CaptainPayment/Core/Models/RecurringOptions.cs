using System.ComponentModel.DataAnnotations;

namespace CaptainPayment.Core.Models;

/// <summary>
/// Options for recurring billing configuration
/// </summary>
public class RecurringOptions
{
    /// <summary>
    /// Specifies billing frequency. One of day, week, month or year.
    /// </summary>
    [Required(ErrorMessage = "Interval is required for recurring prices")]
    public StripeInterval Interval { get; set; }

    /// <summary>
    /// The number of intervals (specified in the interval attribute) between subscription billings.
    /// For example, interval=month and interval_count=3 bills every 3 months.
    /// Maximum of 12 for yearly intervals, 12 for monthly, 52 for weekly, 365 for daily.
    /// </summary>
    [Range(1, 365, ErrorMessage = "Interval count must be between 1 and 365")]
    public int? IntervalCount { get; set; } = 1;

    /// <summary>
    /// Configures how the quantity per period should be determined for metered billing.
    /// Either "metered" or "licensed". Defaults to "licensed".
    /// </summary>
    public StripeUsageType UsageType { get; set; } = StripeUsageType.Licensed;

    /// <summary>
    /// Specifies a usage aggregation strategy for prices with usage_type=metered.
    /// Valid values: "sum", "last_during_period", "last_ever", "max"
    /// </summary>
    public string AggregateUsage { get; set; } = string.Empty;
}