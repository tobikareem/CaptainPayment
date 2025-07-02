namespace CaptainPayment.Core.Models;

public class UpdateProductRequest
{
    public string? Name { get; set; }
    public string? Description { get; set; }
    public bool? Active { get; set; }
    public List<string>? Images { get; set; }
    public string? Url { get; set; }
    public string? TaxCode { get; set; }
    public string? UnitLabel { get; set; }
    public string? StatementDescriptor { get; set; }
    public Dictionary<string, string>? Metadata { get; set; }
}
