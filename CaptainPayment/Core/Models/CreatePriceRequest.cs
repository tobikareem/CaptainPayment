using System.ComponentModel.DataAnnotations;

namespace CaptainPayment.Core.Models;

/// <summary>
/// Request model for creating a new price in Stripe
/// </summary>
public class CreatePriceRequest
{
    /// <summary>
    /// The ID of the product that this price will be associated with.
    /// Must be an existing product ID from Stripe.
    /// </summary>
    [Required(ErrorMessage = "Product ID is required")]
    public string ProductId { get; set; } = string.Empty;

    /// <summary>
    /// Three-letter ISO currency code (e.g., "usd", "eur", "gbp").
    /// Must be a currency supported by Stripe.
    /// </summary>
    [Required(ErrorMessage = "Currency is required")]
    [StringLength(3, MinimumLength = 3, ErrorMessage = "Currency must be a 3-letter ISO code")]
    public string Currency { get; set; } = "usd";

    /// <summary>
    /// The unit amount for this price in the specified currency.
    /// For zero-decimal currencies (e.g., JPY), this is the amount in the currency.
    /// For other currencies, this should be in the smallest currency unit (e.g., cents for USD).
    /// </summary>
    [Range(0.01, double.MaxValue, ErrorMessage = "Unit amount must be greater than 0")]
    public decimal UnitAmount { get; set; }

    /// <summary>
    /// Whether the price can be used for new purchases. Defaults to true.
    /// Inactive prices cannot be used for new subscriptions or checkout sessions.
    /// </summary>
    public bool? Active { get; set; } = true;

    /// <summary>
    /// Set of key-value pairs for storing additional information about the price.
    /// Maximum of 50 keys, each key limited to 40 characters, values limited to 500 characters.
    /// </summary>
    public Dictionary<string, string> Metadata { get; set; } = new();

    /// <summary>
    /// A brief description of the price, hidden from customers.
    /// Used for internal identification and organization.
    /// </summary>
    [StringLength(100, ErrorMessage = "Nickname cannot exceed 100 characters")]
    public string Nickname { get; set; } = string.Empty;

    /// <summary>
    /// The recurring components of a price such as interval and usage_type.
    /// Required for subscription products, leave none for one-time purchases.
    /// </summary>
    public RecurringOptions Recurring { get; set; } = new() {Interval = StripeInterval.None};

    /// <summary>
    /// Specifies whether the price is considered inclusive of tax or exclusive of tax.
    /// Valid values: "exclusive", "inclusive", "unspecified"
    /// </summary>
    [StringLength(20)]
    public string TaxBehavior { get; set; } = "unspecified";

    /// <summary>
    /// Each element represents a pricing tier. This parameter requires billing_scheme to be set to "tiered".
    /// Maximum of 20 tiers allowed.
    /// </summary>
    public List<TierOptions> Tiers { get; set; } = new();

    /// <summary>
    /// Defines if the tiering price should be "graduated" or "volume" based.
    /// - graduated: The price per unit changes as quantity increases
    /// - volume: The same price per unit applies to the entire quantity
    /// </summary>
    [RegularExpression("^(graduated|volume)$", ErrorMessage = "TiersMode must be either 'graduated' or 'volume'")]
    public string TiersMode { get; set; } = "graduated";

    /// <summary>
    /// Apply a transformation to the reported usage or set quantity before computing the amount billed.
    /// Cannot be combined with tiers.
    /// </summary>
    public TransformQuantityOptions? TransformQuantity { get; set; }

    /// <summary>
    /// Indicates whether to copy the lookup_key from the existing price when creating a new price.
    /// Only applicable when updating prices.
    /// </summary>
    public bool? TransferLookupKey { get; set; }
}