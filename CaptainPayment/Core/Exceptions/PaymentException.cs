namespace CaptainPayment.Core.Exceptions;

public class PaymentException : Exception
{
    public string? ErrorCode { get; }
    public string? Provider { get; }

    public PaymentException(string message) : base(message) { }

    public PaymentException(string message, Exception innerException) : base(message, innerException) { }

    public PaymentException(string message, string? errorCode = null, string? provider = null) : base(message)
    {
        ErrorCode = errorCode;
        Provider = provider;
    }
}