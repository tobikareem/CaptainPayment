namespace CaptainPayment.Core.Models;

public class BillingDetails
{
    public CustomerAddress Address { get; set; } = new CustomerAddress();
    public string Email { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
}