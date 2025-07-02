namespace CaptainPayment.Core.Models;

public class UpdateCustomerRequest
{
    public string Email { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public CustomerAddress Address { get; set; } = new CustomerAddress();
    public CustomerAddress ShippingAddress { get; set; } = new CustomerAddress();
    public string DefaultPaymentMethodId { get; set; } = string.Empty;
    public Dictionary<string, string> Metadata { get; set; } = new();
}