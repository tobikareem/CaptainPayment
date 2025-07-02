using CaptainPayment.Core.Models;

namespace CaptainPayment.Core.Interfaces;

public interface IWebHookHandler
{
    Task<WebhookHandleResult> HandleWebhookAsync(string payload, string signature, CancellationToken cancellationToken = default);
    Task<bool> ValidateWebhookSignatureAsync(string payload, string signature);
}