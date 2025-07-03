using Stripe;
using System.ComponentModel.DataAnnotations;

namespace CaptainPayment.Core.Models;

/// <summary>
/// Request model for creating a new product in Stripe
/// </summary>
public class CreateProductRequest
{
    /// <summary>
    /// The product's name, meant to be displayable to the customer.
    /// Maximum length is 250 characters.
    /// </summary>
    [Required(ErrorMessage = "Product name is required")]
    [StringLength(250, ErrorMessage = "Product name cannot exceed 250 characters")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// The product's description, meant to be displayable to the customer.
    /// Use this field to optionally store a long form explanation of the product.
    /// Maximum length is 40,000 characters.
    /// </summary>
    [StringLength(40000, ErrorMessage = "Product description cannot exceed 40,000 characters")]
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Whether the product is currently available for purchase. Defaults to true.
    /// Inactive products cannot be used in new payment flows.
    /// </summary>
    public bool? Active { get; set; } = true;

    /// <summary>
    /// A list of up to 8 URLs of images for this product.
    /// Images should be publicly accessible and in JPEG, PNG, or GIF format.
    /// </summary>
    [MaxLength(8, ErrorMessage = "Maximum of 8 images allowed")]
    public List<string> Images { get; set; } = new();

    /// <summary>
    /// Set of key-value pairs for storing additional information about the product.
    /// Maximum of 50 keys, each key limited to 40 characters, values limited to 500 characters.
    /// </summary>
    public Dictionary<string, string> Metadata { get; set; } = new();

    /// <summary>
    /// The dimensions of this product for shipping purposes.
    /// Required if the product is shippable.
    /// </summary>
    public PackageDimensions PackageDimensions { get; set; } = new PackageDimensions();

    /// <summary>
    /// Whether this product is shipped (i.e., physical goods).
    /// Defaults to true for products with package dimensions.
    /// </summary>
    public bool? Shippable { get; set; }

    /// <summary>
    /// An arbitrary string to be displayed on your customer's credit card or bank statement.
    /// Maximum length is 22 characters. Only alphanumeric characters and certain special characters are allowed.
    /// </summary>
    [StringLength(22, ErrorMessage = "Statement descriptor cannot exceed 22 characters")]
    public string StatementDescriptor { get; set; } = string.Empty;

    /// <summary>
    /// A tax code ID that represents the tax classification for this product.
    /// Must be a valid tax code from Stripe's tax code list.
    /// </summary>
    public string TaxCode { get; set; } = string.Empty;

    /// <summary>
    /// A label that represents units of this product in Stripe and on customers' receipts and invoices.
    /// Examples: "hour", "seat", "GB", "license"
    /// </summary>
    [StringLength(20, ErrorMessage = "Unit label cannot exceed 20 characters")]
    public string UnitLabel { get; set; } = string.Empty;

    /// <summary>
    /// A URL of a publicly-accessible webpage for this product.
    /// Must be a valid, publicly accessible URL.
    /// </summary>
    [Url(ErrorMessage = "Invalid URL format")]
    public string Url { get; set; } = string.Empty;

    /// <summary>
    /// Three-letter ISO currency code for the default price.
    /// Only used when creating a default price alongside the product.
    /// </summary>
    [StringLength(3, MinimumLength = 3, ErrorMessage = "Currency must be a 3-letter ISO code")]
    public string Currency { get; set; } = string.Empty;

    /// <summary>
    /// The amount for the default price in the smallest currency unit (e.g., cents for USD).
    /// Only used when creating a default price alongside the product.
    /// Must be greater than 0 if currency is specified.
    /// </summary>
    [Range(1, long.MaxValue, ErrorMessage = "Amount must be greater than 0 when creating a default price")]
    public long AmountInCents { get; set; }

    /// <summary>
    /// The billing frequency for the default price (month, year, etc.).
    /// Only used when creating a subscription product with a default recurring price.
    /// </summary>
    public StripeInterval Interval { get; set; } = StripeInterval.None;
}