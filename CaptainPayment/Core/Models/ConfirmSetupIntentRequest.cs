namespace CaptainPayment.Core.Models;

public class ConfirmSetupIntentRequest
{
    public string PaymentMethodId { get; set; } = string.Empty;
    public string ReturnUrl { get; set; } = string.Empty;
}