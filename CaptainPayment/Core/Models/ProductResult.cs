using Stripe;

namespace CaptainPayment.Core.Models;

public class ProductResult
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool Active { get; set; }
    public DateTime Created { get; set; }
    public DateTime Updated { get; set; }
    public List<string> Images { get; set; } = new();
    public Dictionary<string, string> Metadata { get; set; } = new();
    public PackageDimensions PackageDimensions { get; set; } = new();
    public bool? Shippable { get; set; }
    public string StatementDescriptor { get; set; } = string.Empty;
    public string TaxCode { get; set; } = string.Empty;
    public string UnitLabel { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public bool LiveMode { get; set; }
}