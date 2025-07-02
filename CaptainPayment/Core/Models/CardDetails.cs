namespace CaptainPayment.Core.Models;

public class CardDetails
{
    public string Brand { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public long ExpMonth { get; set; }
    public long ExpYear { get; set; }
    public string Fingerprint { get; set; } = string.Empty;
    public string Funding { get; set; } = string.Empty;
    public string Last4 { get; set; } = string.Empty;
    public bool? ThreeDSecureUsage { get; set; }
    public string Wallet { get; set; } = string.Empty;
}