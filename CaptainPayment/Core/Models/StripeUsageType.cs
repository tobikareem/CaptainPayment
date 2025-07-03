using System.ComponentModel.DataAnnotations;

namespace CaptainPayment.Core.Models;

public enum StripeUsageType
{
    /// <summary>
    /// Default - the quantity is set when creating the subscription
    /// </summary>
    [Display(Name = "licensed")]
    Licensed,

    /// <summary>
    /// Usage is recorded during the billing period and customers are charged at the end
    /// </summary>
    [Display(Name = "metered")]
    Metered,

    [Display(Name = "None")]
    None
}