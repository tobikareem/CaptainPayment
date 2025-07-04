using CaptainPayment.Core.Config;
using CaptainPayment.Core.Exceptions;
using CaptainPayment.Core.Interfaces;
using CaptainPayment.Core.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Stripe;

namespace CaptainPayment.Stripe.Services.Payments;

public class StripeSetupIntentService : ISetupIntentService
{
    private readonly ILogger<StripeSetupIntentService> _logger;
    private readonly StripeSettings _settings;

    public StripeSetupIntentService(IOptions<StripeSettings> options, ILogger<StripeSetupIntentService> logger)
    {
        _settings = options.Value ?? throw new ArgumentNullException(nameof(options));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        StripeConfiguration.ApiKey = _settings.SecretKey;
    }

    public async Task<SetupIntentResult> CreateSetupIntentAsync(CreateSetupIntentRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Creating Setup Intent for customer: {CustomerId}", request.CustomerId);

            var service = new SetupIntentService();
            var options = new SetupIntentCreateOptions
            {
                Customer = request.CustomerId,
                PaymentMethodTypes = new List<string> { "card" },
                Usage = "off_session",
                Metadata = request.Metadata,
                Description = request.Description
            };

            var setupIntent = await service.CreateAsync(options, cancellationToken: cancellationToken);

            _logger.LogInformation("Setup Intent created: {SetupIntentId}", setupIntent.Id);

            return new SetupIntentResult
            {
                Id = setupIntent.Id,
                ClientSecret = setupIntent.ClientSecret,
                Status = setupIntent.Status,
                PaymentMethodId = setupIntent.PaymentMethodId,
                CustomerId = setupIntent.CustomerId
            };
        }
        catch (StripeException ex)
        {
            _logger.LogError(ex, "Failed to create Setup Intent for customer: {CustomerId}", request.CustomerId);
            throw new PaymentException($"Failed to create Setup Intent: {ex.Message}",
                ex.HttpStatusCode.ToString(), _settings.ProviderName);
        }
    }

    public async Task<SetupIntentResult> GetSetupIntentAsync(string setupIntentId, CancellationToken cancellationToken = default)
    {
        try
        {
            var service = new SetupIntentService();
            var setupIntent = await service.GetAsync(setupIntentId, cancellationToken: cancellationToken);

            return new SetupIntentResult
            {
                Id = setupIntent.Id,
                ClientSecret = setupIntent.ClientSecret,
                Status = setupIntent.Status,
                PaymentMethodId = setupIntent.PaymentMethodId,
                CustomerId = setupIntent.CustomerId
            };
        }
        catch (StripeException ex)
        {
            _logger.LogError(ex, "Failed to retrieve Setup Intent: {SetupIntentId}", setupIntentId);
            throw new PaymentException($"Failed to retrieve Setup Intent: {ex.Message}",
                ex.HttpStatusCode.ToString(), _settings.ProviderName);
        }
    }

    public async Task<SetupIntentResult> ConfirmSetupIntentAsync(string setupIntentId, ConfirmSetupIntentRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var service = new SetupIntentService();
            var options = new SetupIntentConfirmOptions
            {
                PaymentMethod = request.PaymentMethodId,
                ReturnUrl = request.ReturnUrl
            };

            var setupIntent = await service.ConfirmAsync(setupIntentId, options, cancellationToken: cancellationToken);

            return new SetupIntentResult
            {
                Id = setupIntent.Id,
                ClientSecret = setupIntent.ClientSecret,
                Status = setupIntent.Status,
                PaymentMethodId = setupIntent.PaymentMethodId,
                CustomerId = setupIntent.CustomerId
            };
        }
        catch (StripeException ex)
        {
            _logger.LogError(ex, "Failed to confirm Setup Intent: {SetupIntentId}", setupIntentId);
            throw new PaymentException($"Failed to confirm Setup Intent: {ex.Message}",
                ex.HttpStatusCode.ToString(), _settings.ProviderName);
        }
    }
}