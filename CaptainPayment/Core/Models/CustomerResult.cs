namespace CaptainPayment.Core.Models;

public class CustomerResult
{
    public string Id { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime Created { get; set; }   
    public string Currency { get; set; } = string.Empty;
    public long Balance { get; set; }
    public bool Delinquent { get; set; }
    public string DefaultSource { get; set; } = string.Empty;
    public string InvoicePrefix { get; set; } = string.Empty;
    public CustomerAddress Address { get; set; }  = new CustomerAddress();
    public CustomerAddress ShippingAddress { get; set; } = new CustomerAddress();
    public string TaxExempt { get; set; } = string.Empty;
    public Dictionary<string, string> Metadata { get; set; } = new();
    public bool LiveMode { get; set; }
}