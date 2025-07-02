namespace CaptainPayment.Core.Models;

public class CreateCustomerRequest
{
    public string Email { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public CustomerAddress Address { get; set; } = new CustomerAddress();
    public CustomerAddress ShippingAddress { get; set; } = new CustomerAddress();
    public string DefaultPaymentMethodId { get; set; } = string.Empty;
    public string CouponId { get; set; } = string.Empty;
    public string TaxExempt { get; set; } = string.Empty;
    public Dictionary<string, string> Metadata { get; set; } = new();
    public List<InvoiceCustomField> InvoiceCustomFields { get; set; } = new();
}