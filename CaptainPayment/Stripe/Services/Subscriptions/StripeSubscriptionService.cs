using CaptainPayment.Core.Config;
using CaptainPayment.Core.Exceptions;
using CaptainPayment.Core.Interfaces;
using CaptainPayment.Core.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Stripe;

namespace CaptainPayment.Stripe.Services.Subscriptions;

public class StripeSubscriptionService : IPaymentProvider, ISubscriptionService
{
    private readonly ILogger<StripeSubscriptionService> _logger;
    private readonly StripeSettings _settings;
    
    private const decimal CurrencyMultiplier = 100m;

    public string ProviderName => _settings.ProviderName;

    public StripeSubscriptionService(IOptions<StripeSettings> options, ILogger<StripeSubscriptionService> logger)
    {
        _settings = options.Value ?? throw new ArgumentNullException(nameof(options));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        ValidateConfiguration();
        StripeConfiguration.ApiKey = _settings.SecretKey;
    }

    public async Task<CreateSubscriptionResult> CreateSubscriptionAsync(CreateSubscriptionRequest request, CancellationToken cancellationToken = default)
    {
        ValidateCreateSubscriptionRequest(request);

        try
        {
            _logger.LogInformation("Creating Stripe subscription for {Email}", request.Email);

            var customer = await GetOrCreateCustomerAsync(request, cancellationToken);
            await AttachPaymentMethodAsync(customer.Id, request.PaymentMethodId, cancellationToken);

            var options = BuildSubscriptionOptions(customer.Id, request);
            var service = new SubscriptionService();
            var subscription = await service.CreateAsync(options, cancellationToken: cancellationToken);

            _logger.LogInformation("Subscription created successfully: {SubscriptionId}", subscription.Id);
            return MapToCreateSubscriptionResult(subscription);
        }
        catch (StripeException ex)
        {
            _logger.LogError(ex, "Stripe error creating subscription for {Email}", request.Email);
            throw new PaymentException($"Payment processing failed: {ex.Message}", ex.HttpStatusCode.ToString(), ProviderName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error creating subscription for {Email}", request.Email);
            throw;
        }
    }

    public async Task<SubscriptionDetails> GetSubscriptionAsync(string subscriptionId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(subscriptionId))
            throw new ArgumentException("Subscription ID cannot be null or empty", nameof(subscriptionId));

        try
        {
            var subscriptionService = new SubscriptionService();
            var subscription = await subscriptionService.GetAsync(subscriptionId, new SubscriptionGetOptions
            {
                Expand = _settings.SubscriptionDefaults.DefaultExpand
            }, cancellationToken: cancellationToken);

            return MapToSubscriptionDetails(subscription);
        }
        catch (StripeException ex)
        {
            _logger.LogError(ex, "Stripe error getting subscription {SubscriptionId}", subscriptionId);
            throw new PaymentException($"Failed to retrieve subscription: {ex.Message}", ex.HttpStatusCode.ToString(), ProviderName);
        }
    }

    public async Task<CancelSubscriptionResult> CancelSubscriptionAsync(string subscriptionId, bool cancelImmediately = false, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(subscriptionId))
            throw new ArgumentException("Subscription ID cannot be null or empty", nameof(subscriptionId));

        try
        {
            var service = new SubscriptionService();
            var options = new SubscriptionCancelOptions();

            Subscription subscription;
            if (cancelImmediately)
            {
                subscription = await service.CancelAsync(subscriptionId, options, cancellationToken: cancellationToken);
            }
            else
            {
                subscription = await service.UpdateAsync(subscriptionId, new SubscriptionUpdateOptions
                {
                    CancelAtPeriodEnd = true
                }, cancellationToken: cancellationToken);
            }

            return new CancelSubscriptionResult
            {
                SubscriptionId = subscription.Id,
                Status = subscription.Status,
                CanceledAt = subscription.CanceledAt,
                CancelAtPeriodEnd = subscription.CancelAtPeriodEnd
            };
        }
        catch (StripeException ex)
        {
            _logger.LogError(ex, "Error cancelling subscription: {Id}", subscriptionId);
            throw new PaymentException("Failed to cancel subscription", ex.HttpStatusCode.ToString(), ProviderName);
        }
    }

    public async Task<IEnumerable<SubscriptionDetails>> GetCustomerSubscriptionsAsync(string customerId, CancellationToken cancellationToken = default)
    {
        var service = new SubscriptionService();
        var subscriptions = await service.ListAsync(new SubscriptionListOptions
        {
            Customer = customerId
        }, cancellationToken: cancellationToken);

        return subscriptions.Data.Select(MapToSubscriptionDetails);
    }


    private async Task<Customer> GetOrCreateCustomerAsync(CreateSubscriptionRequest request, CancellationToken cancellationToken)
    {
        var service = new CustomerService();

        // Try to get existing customer by ID first
        if (!string.IsNullOrEmpty(request.CustomerId))
        {
            try
            {
                var existingCustomer = await service.GetAsync(request.CustomerId, cancellationToken: cancellationToken);
                _logger.LogInformation("Found existing customer: {CustomerId}", existingCustomer.Id);
                return existingCustomer;
            }
            catch (StripeException ex) when (ex.HttpStatusCode == System.Net.HttpStatusCode.NotFound)
            {
                _logger.LogWarning("Customer {CustomerId} not found, will search by email or create new", request.CustomerId);
            }
        }
        
        if (!string.IsNullOrEmpty(request.Email))
        {
            var existing = await service.ListAsync(new CustomerListOptions
            {
                Email = request.Email,
                Limit = 1
            }, cancellationToken: cancellationToken);

            if (existing.Data.Any())
            {
                var existingCustomer = existing.Data.First();
                _logger.LogInformation("Found existing customer by email: {CustomerId}", existingCustomer.Id);
                return existingCustomer;
            }
        }
        
        var createOptions = new CustomerCreateOptions
        {
            Email = request.Email,
            Name = request.FullName,
            Metadata = request.Metadata ?? new Dictionary<string, string>()
        };

        var newCustomer = await service.CreateAsync(createOptions, cancellationToken: cancellationToken);
        _logger.LogInformation("Created new customer: {CustomerId}", newCustomer.Id);
        return newCustomer;
    }

    private async Task AttachPaymentMethodAsync(string customerId, string paymentMethodId, CancellationToken cancellationToken)
    {
        var methodService = new PaymentMethodService();
        await methodService.AttachAsync(paymentMethodId, new PaymentMethodAttachOptions
        {
            Customer = customerId
        }, cancellationToken: cancellationToken);
    }

    private SubscriptionCreateOptions BuildSubscriptionOptions(string customerId, CreateSubscriptionRequest request)
    {
        var options = new SubscriptionCreateOptions
        {
            Customer = customerId,
            Items = new List<SubscriptionItemOptions>
            {
                new()
                {
                    Price = request.PriceId,
                    Quantity = request.Quantity ?? 1
                }
            },
            DefaultPaymentMethod = request.PaymentMethodId,
            PaymentBehavior = _settings.PaymentOptions.PaymentBehavior,
            PaymentSettings = new SubscriptionPaymentSettingsOptions
            {
                SaveDefaultPaymentMethod = _settings.SubscriptionDefaults.SaveDefaultPaymentMethod ? "on_subscription" : "off"
            },
            TrialPeriodDays = request.TrialPeriodDays ?? _settings.SubscriptionDefaults.DefaultTrialDays,
            Metadata = request.Metadata ?? new Dictionary<string, string>(),
            Expand = _settings.SubscriptionDefaults.DefaultExpand
        };

        return options;
    }

    private static CreateSubscriptionResult MapToCreateSubscriptionResult(Subscription subscription)
    {
        // var paymentIntent = subscription.LatestInvoice?.PaymentIntent;

        return new CreateSubscriptionResult
        {
            SubscriptionId = subscription.Id,
            CustomerId = subscription.CustomerId,
            TrialEnd = subscription.TrialEnd,
            Status = subscription.Status,
            Amount = (subscription.Items?.Data.FirstOrDefault()?.Price?.UnitAmount ?? 0) / CurrencyMultiplier,
            Currency = subscription.Items?.Data.FirstOrDefault()?.Price?.Currency ?? "usd",
            //ClientSecret = paymentIntent?.ClientSecret,
            //RequiresAction = paymentIntent?.Status == "requires_action",
            CurrentPeriodStart = subscription.StartDate,
            CurrentPeriodEnd = subscription.CanceledAt.GetValueOrDefault()
        };
    }

    private static SubscriptionDetails MapToSubscriptionDetails(Subscription sub)
    {
        var item = sub.Items?.Data.FirstOrDefault();
        return new SubscriptionDetails
        {
            Id = sub.Id,
            CustomerId = sub.CustomerId,
            Status = sub.Status,
            Amount = (item?.Price?.UnitAmount ?? 0) / CurrencyMultiplier,
            Currency = item?.Price?.Currency ?? "usd",
            Interval = item.Price?.Recurring?.Interval,
            TrialStart = sub.TrialStart,
            TrialEnd = sub.TrialEnd,
            CanceledAt = sub.CanceledAt,
            SubscriptionEndDate = sub.CancelAt,
            CreatedAt = sub.Created,
            PriceId = item?.Price?.Id,
            ProductId = item?.Price?.ProductId,
            CurrentPeriodStart = sub.Created,
            CurrentPeriodEnd = sub.CanceledAt.GetValueOrDefault()
        };
    }

    public async Task<bool> ValidateSubscriptionAsync(string subscriptionId, CancellationToken cancellationToken = default)
    {
        try
        {
            var subscription = await GetSubscriptionAsync(subscriptionId, cancellationToken);
            return subscription.Status is "active" or "trialing";
        }
        catch
        {
            return false;
        }
    }

    public async Task<UpdateSubscriptionResult> UpdateSubscriptionAsync(string subscriptionId, UpdateSubscriptionRequest request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Updating subscription {SubscriptionId}", subscriptionId);

            var service = new SubscriptionService();
            var existingSubscription = await service.GetAsync(subscriptionId, new SubscriptionGetOptions
            {
                Expand = new List<string> { "items" }
            }, cancellationToken: cancellationToken);

            var currentItem = existingSubscription.Items.Data.FirstOrDefault();
            if (currentItem == null)
            {
                throw new PaymentException("Subscription has no items to update", "400", ProviderName);
            }

            var updateOptions = new SubscriptionUpdateOptions
            {
                Items = new List<SubscriptionItemOptions>
                {
                    new()
                    {
                        Id = currentItem.Id,
                        Price = request.NewPriceId,
                        Quantity = request.Quantity ?? currentItem.Quantity
                    }
                },
                Metadata = request.Metadata
            };

            if (request.CancelAtPeriodEnd.HasValue)
                updateOptions.CancelAtPeriodEnd = request.CancelAtPeriodEnd;

            if (request.TrialEnd.HasValue)
            {
                updateOptions.TrialEnd = new AnyOf<DateTime?, SubscriptionTrialEnd>(DateTimeOffset.FromUnixTimeSeconds(request.TrialEnd.Value).DateTime);
            }

            var updated = await service.UpdateAsync(subscriptionId, updateOptions, cancellationToken: cancellationToken);

            return new UpdateSubscriptionResult
            {
                SubscriptionId = updated.Id,
                Status = updated.Status,
            };
        }
        catch (StripeException ex)
        {
            _logger.LogError(ex, "Failed to update subscription {SubscriptionId}", subscriptionId);
            throw new PaymentException("Failed to update subscription", ex.HttpStatusCode.ToString(), ProviderName);
        }
    }

    private void ValidateConfiguration()
    {
        if (string.IsNullOrWhiteSpace(_settings.SecretKey))
            throw new InvalidOperationException("Stripe SecretKey is required");

        if (string.IsNullOrWhiteSpace(_settings.ProviderName))
            throw new InvalidOperationException("ProviderName is required");
    }

    private static void ValidateCreateSubscriptionRequest(CreateSubscriptionRequest request)
    {
        if (request == null)
            throw new ArgumentNullException(nameof(request));

        if (string.IsNullOrWhiteSpace(request.PriceId))
            throw new ArgumentException("PriceId is required", nameof(request));

        if (string.IsNullOrWhiteSpace(request.Email))
            throw new ArgumentException("Email is required", nameof(request));
    }
}