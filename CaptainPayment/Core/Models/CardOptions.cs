namespace CaptainPayment.Core.Models;

public class CardOptions
{
    public string Number { get; set; } = string.Empty;
    public long ExpMonth { get; set; }
    public long ExpYear { get; set; }
    public string Cvc { get; set; } = string.Empty;
}