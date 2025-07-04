namespace CaptainPayment.Core.Models;

public class CreateSetupIntentRequest
{
    public string CustomerId { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public Dictionary<string, string> Metadata { get; set; } = new();
}