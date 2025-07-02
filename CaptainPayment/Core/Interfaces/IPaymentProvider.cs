using CaptainPayment.Core.Models;

namespace CaptainPayment.Core.Interfaces;

public interface IPaymentProvider
{
    string ProviderName { get; }
}