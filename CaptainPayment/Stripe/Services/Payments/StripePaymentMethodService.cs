using CaptainPayment.Core.Config;
using CaptainPayment.Core.Exceptions;
using CaptainPayment.Core.Interfaces;
using CaptainPayment.Core.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Stripe;

namespace CaptainPayment.Stripe.Services.Payments;

public class StripePaymentMethodService : IPaymentMethodService
{
    private readonly ILogger<StripePaymentMethodService> _logger;
    private readonly StripeSettings _settings;

    public StripePaymentMethodService(IOptions<StripeSettings> options, ILogger<StripePaymentMethodService> logger)
    {
        _settings = options.Value ?? throw new ArgumentNullException(nameof(options));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        ValidateConfiguration();
        StripeConfiguration.ApiKey = _settings.SecretKey;
    }

    public async Task<PaymentMethodResult> CreatePaymentMethodAsync(CreatePaymentMethodRequest request, CancellationToken cancellationToken = default)
    {
        ValidateCreatePaymentMethodRequest(request);

        try
        {
            _logger.LogInformation("Creating payment method of type: {Type}", request.Type);

            var service = new PaymentMethodService();
            var options = new PaymentMethodCreateOptions
            {
                Type = request.Type,
                Card =  new PaymentMethodCardOptions
                {
                    Number = request.Card.Number,
                    ExpMonth = request.Card.ExpMonth,
                    ExpYear = request.Card.ExpYear,
                    Cvc = request.Card.Cvc
                },
                BillingDetails = new PaymentMethodBillingDetailsOptions
                {
                    Address = new AddressOptions
                    {
                        Line1 = request.BillingDetails.Address.Line1,
                        Line2 = request.BillingDetails.Address.Line2,
                        City = request.BillingDetails.Address.City,
                        State = request.BillingDetails.Address.State,
                        PostalCode = request.BillingDetails.Address.PostalCode,
                        Country = request.BillingDetails.Address.Country
                    },
                    Email = request.BillingDetails.Email,
                    Name = request.BillingDetails.Name,
                    Phone = request.BillingDetails.Phone
                },
                Metadata = request.Metadata 
            };

            var paymentMethod = await service.CreateAsync(options, cancellationToken: cancellationToken);

            _logger.LogInformation("Payment method created successfully: {PaymentMethodId}", paymentMethod.Id);
            return MapToPaymentMethodResult(paymentMethod);
        }
        catch (StripeException ex)
        {
            _logger.LogError(ex, "Failed to create payment method");
            throw new PaymentException($"Failed to create payment method: {ex.Message}",
                ex.HttpStatusCode.ToString(), _settings.ProviderName);
        }
    }

    public async Task<PaymentMethodResult> GetPaymentMethodAsync(string paymentMethodId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(paymentMethodId))
            throw new ArgumentException("Payment method ID cannot be null or empty", nameof(paymentMethodId));

        try
        {
            var service = new PaymentMethodService();
            var paymentMethod = await service.GetAsync(paymentMethodId, cancellationToken: cancellationToken);
            return MapToPaymentMethodResult(paymentMethod);
        }
        catch (StripeException ex)
        {
            _logger.LogError(ex, "Failed to retrieve payment method {PaymentMethodId}", paymentMethodId);
            throw new PaymentException($"Failed to retrieve payment method: {ex.Message}",
                ex.HttpStatusCode.ToString(), _settings.ProviderName);
        }
    }

    public async Task<PaymentMethodResult> UpdatePaymentMethodAsync(string paymentMethodId, UpdatePaymentMethodRequest request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(paymentMethodId))
            throw new ArgumentException("Payment method ID cannot be null or empty", nameof(paymentMethodId));

        if (request == null)
            throw new ArgumentNullException(nameof(request));

        try
        {
            _logger.LogInformation("Updating payment method {PaymentMethodId}", paymentMethodId);

            var service = new PaymentMethodService();
            var options = new PaymentMethodUpdateOptions
            {
                Metadata = request.Metadata,
                BillingDetails = new PaymentMethodBillingDetailsOptions
                {
                    Name = request.BillingDetails?.Name,
                    Email = request.BillingDetails?.Email,
                    Phone = request.BillingDetails?.Phone,
                    Address = new AddressOptions
                    {
                        Line1 = request.BillingDetails?.Address.Line1,
                        Line2 = request.BillingDetails?.Address.Line2,
                        City = request.BillingDetails?.Address.City,
                        State = request.BillingDetails?.Address.State,
                        PostalCode = request.BillingDetails?.Address.PostalCode,
                        Country = request.BillingDetails?.Address.Country
                    } 
                } 
            };

            var updated = await service.UpdateAsync(paymentMethodId, options, cancellationToken: cancellationToken);

            _logger.LogInformation("Payment method {PaymentMethodId} updated", paymentMethodId);
            return MapToPaymentMethodResult(updated);
        }
        catch (StripeException ex)
        {
            _logger.LogError(ex, "Failed to update payment method {PaymentMethodId}", paymentMethodId);
            throw new PaymentException($"Failed to update payment method: {ex.Message}",
                ex.HttpStatusCode.ToString(), _settings.ProviderName);
        }
    }

    public async Task<PaymentMethodResult> AttachPaymentMethodAsync(string paymentMethodId, string customerId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(paymentMethodId))
            throw new ArgumentException("Payment method ID cannot be null or empty", nameof(paymentMethodId));
        if (string.IsNullOrWhiteSpace(customerId))
            throw new ArgumentException("Customer ID cannot be null or empty", nameof(customerId));

        try
        {
            var service = new PaymentMethodService();
            var paymentMethod = await service.AttachAsync(paymentMethodId, new PaymentMethodAttachOptions
            {
                Customer = customerId
            }, cancellationToken: cancellationToken);

            _logger.LogInformation("Payment method {PaymentMethodId} attached to customer {CustomerId}", paymentMethodId, customerId);
            return MapToPaymentMethodResult(paymentMethod);
        }
        catch (StripeException ex)
        {
            _logger.LogError(ex, "Failed to attach payment method {PaymentMethodId} to customer {CustomerId}", paymentMethodId, customerId);
            throw new PaymentException($"Failed to attach payment method: {ex.Message}",
                ex.HttpStatusCode.ToString(), _settings.ProviderName);
        }
    }

    public async Task<PaymentMethodResult> DetachPaymentMethodAsync(string paymentMethodId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(paymentMethodId))
            throw new ArgumentException("Payment method ID cannot be null or empty", nameof(paymentMethodId));

        try
        {
            var service = new PaymentMethodService();
            var paymentMethod = await service.DetachAsync(paymentMethodId, cancellationToken: cancellationToken);

            _logger.LogInformation("Payment method {PaymentMethodId} detached from customer", paymentMethodId);
            return MapToPaymentMethodResult(paymentMethod);
        }
        catch (StripeException ex)
        {
            _logger.LogError(ex, "Failed to detach payment method {PaymentMethodId}", paymentMethodId);
            throw new PaymentException($"Failed to detach payment method: {ex.Message}",
                ex.HttpStatusCode.ToString(), _settings.ProviderName);
        }
    }

    public async Task<PagedResult<PaymentMethodResult>> ListCustomerPaymentMethodsAsync(string customerId, string type, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(customerId))
            throw new ArgumentException("Customer ID cannot be null or empty", nameof(customerId));

        try
        {
            var service = new PaymentMethodService();
            var options = new PaymentMethodListOptions
            {
                Customer = customerId,
                Type = type
            };

            var paymentMethods = await service.ListAsync(options, cancellationToken: cancellationToken);

            return new PagedResult<PaymentMethodResult>
            {
                Data = paymentMethods.Data.Select(MapToPaymentMethodResult).ToList(),
                HasMore = paymentMethods.HasMore,
                TotalCount = paymentMethods.Data.Count
            };
        }
        catch (StripeException ex)
        {
            _logger.LogError(ex, "Failed to list payment methods for customer {CustomerId}", customerId);
            throw new PaymentException($"Failed to list payment methods: {ex.Message}",
                ex.HttpStatusCode.ToString(), _settings.ProviderName);
        }
    }

    private static void ValidateCreatePaymentMethodRequest(CreatePaymentMethodRequest request)
    {
        if (request == null)
            throw new ArgumentNullException(nameof(request));

        if (string.IsNullOrWhiteSpace(request.Type))
            throw new ArgumentException("Payment method type is required", nameof(request));
    }

    private static PaymentMethodResult MapToPaymentMethodResult(PaymentMethod paymentMethod)
    {
        if (paymentMethod == null)
            throw new ArgumentNullException(nameof(paymentMethod));

        return new PaymentMethodResult
        {
            Id = paymentMethod.Id,
            Type = paymentMethod.Type,
            Created = paymentMethod.Created,
            CustomerId = paymentMethod.CustomerId,
            Card = new CardDetails
            {
                Brand = paymentMethod.Card.Brand,
                Country = paymentMethod.Card.Country,
                ExpMonth = paymentMethod.Card.ExpMonth,
                ExpYear = paymentMethod.Card.ExpYear,
                Fingerprint = paymentMethod.Card.Fingerprint,
                Funding = paymentMethod.Card.Funding,
                Last4 = paymentMethod.Card.Last4,
                ThreeDSecureUsage = paymentMethod.Card.ThreeDSecureUsage?.Supported,
                Wallet = paymentMethod.Card.Wallet.Type
            },
            BillingDetails =  new BillingDetails
            {
                Address =  new CustomerAddress
                {
                    Line1 = paymentMethod.BillingDetails.Address.Line1,
                    Line2 = paymentMethod.BillingDetails.Address.Line2,
                    City = paymentMethod.BillingDetails.Address.City,
                    State = paymentMethod.BillingDetails.Address.State,
                    PostalCode = paymentMethod.BillingDetails.Address.PostalCode,
                    Country = paymentMethod.BillingDetails.Address.Country
                },
                Email = paymentMethod.BillingDetails.Email,
                Name = paymentMethod.BillingDetails.Name,
                Phone = paymentMethod.BillingDetails.Phone
            },
            Metadata = paymentMethod.Metadata,
            LiveMode = paymentMethod.Livemode
        };
    }

    private void ValidateConfiguration()
    {
        if (string.IsNullOrWhiteSpace(_settings.SecretKey))
            throw new InvalidOperationException("Stripe SecretKey is required");

        if (string.IsNullOrWhiteSpace(_settings.ProviderName))
            throw new InvalidOperationException("ProviderName is required");
    }
}