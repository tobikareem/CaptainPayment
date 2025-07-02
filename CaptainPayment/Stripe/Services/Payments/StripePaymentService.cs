using CaptainPayment.Core.Config;
using CaptainPayment.Core.Exceptions;
using CaptainPayment.Core.Interfaces;
using CaptainPayment.Core.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Stripe;

namespace CaptainPayment.Stripe.Services.Payments;

public class StripePaymentService : IPaymentService
{
    private readonly ILogger<StripePaymentService> _logger;
    private readonly StripeSettings _settings;

    public StripePaymentService(IOptions<StripeSettings> options, ILogger<StripePaymentService> logger)
    {
        _settings = options.Value ?? throw new ArgumentNullException(nameof(options));
        _logger = logger;

        ValidateConfiguration();
        StripeConfiguration.ApiKey = _settings.SecretKey;
    }

    public async Task<PaymentIntent> CreatePaymentIntentAsync(decimal amount, CreatePaymentIntentOptions options, CancellationToken cancellationToken = default)
    {
        if (amount <= 0)
            throw new ArgumentException("Amount must be greater than zero", nameof(amount));

        var currency = _settings.PaymentOptions.DefaultCurrency;

        _logger.LogInformation("Creating payment intent for amount: {Amount}, currency: {Currency}, customerId: {CustomerId}",
            amount, currency, options.CustomerId);

        try
        {
            var service = new PaymentIntentService();
            var createOptions = BuildPaymentIntentOptions(amount, currency, options);

            var intent = await service.CreateAsync(createOptions, cancellationToken: cancellationToken);

            _logger.LogInformation("Payment intent created with ID: {IntentId}", intent.Id);
            return intent;
        }
        catch (StripeException ex)
        {
            _logger.LogError(ex, "Failed to create payment intent");
            throw new PaymentException($"Failed to create payment intent: {ex.Message}",
                ex.HttpStatusCode.ToString(), _settings.ProviderName);
        }
    }
    public async Task<PaymentIntent> GetPaymentIntentAsync(string paymentIntentId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(paymentIntentId))
            throw new ArgumentException("Payment intent ID cannot be null or empty", nameof(paymentIntentId));

        try
        {
            var service = new PaymentIntentService();
            return await service.GetAsync(paymentIntentId, cancellationToken: cancellationToken);
        }
        catch (StripeException ex)
        {
            _logger.LogError(ex, "Failed to retrieve payment intent {PaymentIntentId}", paymentIntentId);
            throw new PaymentException($"Failed to retrieve payment intent: {ex.Message}",
                ex.HttpStatusCode.ToString(), _settings.ProviderName);
        }
    }


    public async Task<PaymentIntent> ConfirmPaymentIntentAsync(string paymentIntentId, PaymentIntentConfirmOptions options, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(paymentIntentId))
            throw new ArgumentException("Payment intent ID cannot be null or empty", nameof(paymentIntentId));

        try
        {
            var service = new PaymentIntentService();
            return await service.ConfirmAsync(paymentIntentId, options, cancellationToken: cancellationToken);
        }
        catch (StripeException ex)
        {
            _logger.LogError(ex, "Failed to confirm payment intent {PaymentIntentId}", paymentIntentId);
            throw new PaymentException($"Failed to confirm payment intent: {ex.Message}",
                ex.HttpStatusCode.ToString(), _settings.ProviderName);
        }
    }

    private PaymentIntentCreateOptions BuildPaymentIntentOptions(decimal amount, string currency, CreatePaymentIntentOptions options)
    {
        var createOptions = new PaymentIntentCreateOptions
        {
            Amount = (long)(amount * 100),
            Currency = currency,
            PaymentMethodTypes = _settings.PaymentOptions.PaymentMethodTypes,
            Customer = options.CustomerId,
            Metadata = options.Metadata,
            Description = options.Description,
            ReceiptEmail = options.ReceiptEmail,
            SetupFutureUsage = _settings.PaymentOptions.SavePaymentMethod ? "on_session" : null
        };

        return createOptions;
    }

    private void ValidateConfiguration()
    {
        if (string.IsNullOrWhiteSpace(_settings.SecretKey))
            throw new InvalidOperationException("Stripe SecretKey is required");

        if (string.IsNullOrWhiteSpace(_settings.ProviderName))
            throw new InvalidOperationException("ProviderName is required");
    }
}