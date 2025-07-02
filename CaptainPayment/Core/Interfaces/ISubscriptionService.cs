using CaptainPayment.Core.Models;

namespace CaptainPayment.Core.Interfaces;

public interface ISubscriptionService
{
    /// <summary>
    /// Creates a new subscription for a customer, with optional trial, custom metadata, and attached payment method
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<CreateSubscriptionResult> CreateSubscriptionAsync(CreateSubscriptionRequest request, CancellationToken cancellationToken = default);
    /// <summary>
    /// Retrieves details of a specific subscription by its ID
    /// </summary>
    /// <param name="subscriptionId"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<SubscriptionDetails> GetSubscriptionAsync(string subscriptionId, CancellationToken cancellationToken = default);
    /// <summary>
    /// Updates an existing subscription's details, Modifies pricing/quantity or updates billing options
    /// </summary>
    /// <param name="subscriptionId"></param>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<UpdateSubscriptionResult> UpdateSubscriptionAsync(string subscriptionId, UpdateSubscriptionRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Cancels an existing subscription, optionally at the end of the current period
    /// </summary>
    /// <param name="subscriptionId"></param>
    /// <param name="cancelImmediately"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<CancelSubscriptionResult> CancelSubscriptionAsync(string subscriptionId, bool cancelImmediately = false, CancellationToken cancellationToken = default);
    /// <summary>
    /// Retrieves all subscriptions for a specific customer, including active, canceled, and past subscriptions
    /// </summary>
    /// <param name="customerId"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<IEnumerable<SubscriptionDetails>> GetCustomerSubscriptionsAsync(string customerId, CancellationToken cancellationToken = default);
    /// <summary>
    /// Validates if a subscription is active and not canceled or past due
    /// </summary>
    /// <param name="subscriptionId"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<bool> ValidateSubscriptionAsync(string subscriptionId, CancellationToken cancellationToken = default);
}