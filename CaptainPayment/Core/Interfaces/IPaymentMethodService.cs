using CaptainPayment.Core.Models;

namespace CaptainPayment.Core.Interfaces;

public interface IPaymentMethodService
{
    Task<PaymentMethodResult> CreatePaymentMethodAsync(CreatePaymentMethodRequest request, CancellationToken cancellationToken = default);
    Task<PaymentMethodResult> GetPaymentMethodAsync(string paymentMethodId, CancellationToken cancellationToken = default);
    Task<PaymentMethodResult> UpdatePaymentMethodAsync(string paymentMethodId, UpdatePaymentMethodRequest request, CancellationToken cancellationToken = default);
    Task<PaymentMethodResult> AttachPaymentMethodAsync(string paymentMethodId, string customerId, CancellationToken cancellationToken = default);
    Task<PaymentMethodResult> DetachPaymentMethodAsync(string paymentMethodId, CancellationToken cancellationToken = default);
    Task<PagedResult<PaymentMethodResult>> ListCustomerPaymentMethodsAsync(string customerId, string type = null, CancellationToken cancellationToken = default);
}