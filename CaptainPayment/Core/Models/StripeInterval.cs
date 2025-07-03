using System.ComponentModel.DataAnnotations;

namespace CaptainPayment.Core.Models;

/// <summary>
/// Represents billing interval options for Stripe subscriptions
/// </summary>
public enum StripeInterval
{
    /// <summary>
    /// No recurring billing (one-time purchase)
    /// </summary>
    None,

    /// <summary>
    /// Bill every day
    /// </summary>
    [Display(Name = "day")]
    Day,

    /// <summary>
    /// Bill every week
    /// </summary>
    [Display(Name = "week")]
    Week,

    /// <summary>
    /// Bill every month
    /// </summary>
    [Display(Name = "month")]
    Month,

    /// <summary>
    /// Bill every year
    /// </summary>
    [Display(Name = "year")]
    Year
}