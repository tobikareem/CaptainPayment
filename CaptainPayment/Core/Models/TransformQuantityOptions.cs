namespace CaptainPayment.Core.Models;

public class TransformQuantityOptions
{
    public long DivideBy { get; set; }
    public string Round { get; set; } = string.Empty;
}