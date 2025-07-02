namespace CaptainPayment.Core.Models;

public class WebhookHandleResult
{
    public bool Success { get; set; }
    public string EventType { get; set; } = string.Empty;
    public string EventId { get; set; } = string.Empty;
    public string? ErrorMessage { get; set; }
    public Dictionary<string, object>? Data { get; set; }
}