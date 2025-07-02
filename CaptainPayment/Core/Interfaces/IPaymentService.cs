using CaptainPayment.Core.Models;
using Stripe;

namespace CaptainPayment.Core.Interfaces;

public interface IPaymentService
{
    Task<PaymentIntent> CreatePaymentIntentAsync(decimal amount, CreatePaymentIntentOptions options, CancellationToken cancellationToken = default);

    Task<PaymentIntent> GetPaymentIntentAsync(string paymentIntentId, CancellationToken cancellationToken = default);

    Task<PaymentIntent> ConfirmPaymentIntentAsync(string paymentIntentId, PaymentIntentConfirmOptions options, CancellationToken cancellationToken = default);
}