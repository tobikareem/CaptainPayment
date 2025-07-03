namespace CaptainPayment.Core.Models;

/// <summary>
/// Configuration for tiered pricing
/// </summary>
public class TierOptions
{
    /// <summary>
    /// Up to and including to this quantity will be contained in the tier.
    /// Use "inf" for the last tier to indicate unlimited quantity.
    /// </summary>
    public long? UpTo { get; set; }

    /// <summary>
    /// The per unit amount for this tier, in the smallest currency unit.
    /// Cannot be used with flat_amount.
    /// </summary>
    public decimal? UnitAmount { get; set; }

    /// <summary>
    /// The flat billing amount for this tier, in the smallest currency unit.
    /// Cannot be used with unit_amount.
    /// </summary>
    public decimal? FlatAmount { get; set; }
}