# Captain Payment

A comprehensive .NET payment processing library that provides seamless integration with Stripe for subscription management, customer handling, and payment processing.

[![NuGet Version](https://img.shields.io/nuget/v/CaptainPayment.svg)](https://www.nuget.org/packages/CaptainPayment/)
[![License](https://img.shields.io/badge/license-MIT-blue.svg)](LICENSE)
[![Build Status](https://img.shields.io/badge/build-passing-brightgreen.svg)](https://github.com/yourusername/captain-payment)

## 🚀 Features

- **Subscription Management**: Create, update, cancel, and retrieve subscription information
- **Customer Management**: Full CRUD operations for customer data
- **Payment Processing**: Handle payment intents and transactions
- **Error Handling**: Comprehensive exception handling with custom payment exceptions
- **Async Support**: Full async/await support for all operations
- **Logging Integration**: Built-in logging support using Microsoft.Extensions.Logging
- **Configuration**: Easy setup using .NET dependency injection and configuration patterns

## 📦 Installation

Install the package via NuGet Package Manager:

```bash
dotnet add package CaptainPayment
```

Or via Package Manager Console:

```powershell
Install-Package CaptainPayment
```

## ⚙️ Configuration

### 1. Configure Stripe Settings

Add your Stripe configuration to `appsettings.json`:

```json
{
  "StripeSettings": {
    "SecretKey": "sk_test_your_stripe_secret_key",
    "PublishableKey": "pk_test_your_stripe_publishable_key",
    "ProviderName": "Stripe",
    "WebhookSecret": "whsec_your_webhook_secret"
  }
}
```

### 2. Register Services

In your `Program.cs` or `Startup.cs`:

```csharp
using CaptainPayment.Core.Config;
using CaptainPayment.Stripe.Services.Subscriptions;
using CaptainPayment.Stripe.Services.Customers;
using CaptainPayment.Stripe.Services.Payments;

var builder = WebApplication.CreateBuilder(args);

// Configure Stripe settings
builder.Services.Configure<StripeSettings>(
    builder.Configuration.GetSection("StripeSettings"));

// Register Captain Payment services
builder.Services.AddScoped<ISubscriptionService, StripeSubscriptionService>();
builder.Services.AddScoped<ICustomerService, StripeCustomerService>();
builder.Services.AddScoped<IPaymentService, StripePaymentService>();
builder.Services.AddScoped<IPaymentProvider, StripeSubscriptionService>();

// Example Usage:
services.AddStripePayments(options =>
        {
            options.SecretKey = configuration.Payment.Stripe.SandboxClientSecret; // This is usually the secret key for test mode
            options.PublishableKey = configuration.Payment.Stripe.SandboxClientId; // This is usually the publishable key
            options.WebhookSecret = configuration.Payment.Stripe.WebhookSecret;
            options.ProviderName = configuration.Payment.Stripe.ProviderName;
            options.PaymentOptions = configuration.Payment.Stripe.PaymentOptions;
            options.SubscriptionDefaults = configuration.Payment.Stripe.SubscriptionDefaults;
        });

var app = builder.Build();
```

## 🔧 Usage Examples

### Subscription Management

#### Creating a Subscription

```csharp
public class SubscriptionController : ControllerBase
{
    private readonly ISubscriptionService _subscriptionService;

    public SubscriptionController(ISubscriptionService subscriptionService)
    {
        _subscriptionService = subscriptionService;
    }

    [HttpPost("create")]
    public async Task<IActionResult> CreateSubscription([FromBody] CreateSubscriptionRequest request)
    {
        try
        {
            var result = await _subscriptionService.CreateSubscriptionAsync(request);
            return Ok(result);
        }
        catch (PaymentException ex)
        {
            return BadRequest(new { Error = ex.Message, Code = ex.ErrorCode });
        }
    }
}
```

#### Retrieving a Subscription

```csharp
[HttpGet("{subscriptionId}")]
public async Task<IActionResult> GetSubscription(string subscriptionId)
{
    try
    {
        var subscription = await _subscriptionService.GetSubscriptionAsync(subscriptionId);
        return Ok(subscription);
    }
    catch (PaymentException ex)
    {
        return NotFound(new { Error = ex.Message });
    }
}
```

#### Canceling a Subscription

```csharp
[HttpDelete("{subscriptionId}")]
public async Task<IActionResult> CancelSubscription(string subscriptionId)
{
    try
    {
        var result = await _subscriptionService.CancelSubscriptionAsync(subscriptionId);
        return Ok(result);
    }
    catch (PaymentException ex)
    {
        return BadRequest(new { Error = ex.Message });
    }
}
```

### Customer Management

#### Creating a Customer

```csharp
public class CustomerController : ControllerBase
{
    private readonly ICustomerService _customerService;

    public CustomerController(ICustomerService customerService)
    {
        _customerService = customerService;
    }

    [HttpPost]
    public async Task<IActionResult> CreateCustomer([FromBody] CreateCustomerRequest request)
    {
        try
        {
            var customer = await _customerService.CreateCustomerAsync(request);
            return Ok(customer);
        }
        catch (PaymentException ex)
        {
            return BadRequest(new { Error = ex.Message });
        }
    }
}
```

#### Searching Customers

```csharp
[HttpGet("search")]
public async Task<IActionResult> SearchCustomers(string query)
{
    try
    {
        var customer = await _customerService.SearchCustomersAsync(query);
        return Ok(customer);
    }
    catch (PaymentException ex)
    {
        return BadRequest(new { Error = ex.Message });
    }
}
```

### Payment Processing

#### Creating a Payment Intent

```csharp
public class PaymentController : ControllerBase
{
    private readonly IPaymentService _paymentService;

    public PaymentController(IPaymentService paymentService)
    {
        _paymentService = paymentService;
    }

    [HttpPost("intent")]
    public async Task<IActionResult> CreatePaymentIntent([FromBody] PaymentIntentRequest request)
    {
        try
        {
            var intent = await _paymentService.CreatePaymentIntentAsync(
                request.Amount, 
                request.Currency, 
                request.CustomerId);
            
            return Ok(new { ClientSecret = intent.ClientSecret });
        }
        catch (Exception ex)
        {
            return BadRequest(new { Error = ex.Message });
        }
    }
}
```

## 📋 API Reference

### ISubscriptionService

| Method | Description | Parameters | Returns |
|--------|-------------|------------|---------|
| `CreateSubscriptionAsync` | Creates a new subscription | `CreateSubscriptionRequest` | `CreateSubscriptionResult` |
| `GetSubscriptionAsync` | Retrieves subscription details | `string subscriptionId` | `SubscriptionDetails` |
| `CancelSubscriptionAsync` | Cancels a subscription | `string subscriptionId` | `CancelSubscriptionResult` |
| `UpdateSubscriptionAsync` | Updates subscription details | `string subscriptionId, UpdateSubscriptionRequest` | `UpdateSubscriptionResult` |
| `ValidateSubscriptionAsync` | Validates subscription status | `string subscriptionId` | `bool` |

### ICustomerService

| Method | Description | Parameters | Returns |
|--------|-------------|------------|---------|
| `CreateCustomerAsync` | Creates a new customer | `CreateCustomerRequest` | `CustomerResult` |
| `GetCustomerAsync` | Retrieves customer details | `string customerId` | `CustomerResult` |
| `UpdateCustomerAsync` | Updates customer information | `string customerId, UpdateCustomerRequest` | `CustomerResult` |
| `DeleteCustomerAsync` | Deletes a customer | `string customerId` | `bool` |
| `ListCustomersAsync` | Lists customers with pagination | `CustomerListRequest` | `PagedResult<CustomerResult>` |
| `SearchCustomersAsync` | Searches customers by query | `string query` | `CustomerResult` |

### IPaymentService

| Method | Description | Parameters | Returns |
|--------|-------------|------------|---------|
| `CreatePaymentIntentAsync` | Creates a payment intent | `decimal amount, string currency, string customerId` | `PaymentIntent` |

## 🚨 Error Handling

Captain Payment uses custom exception handling with the `PaymentException` class:

```csharp
try
{
    var result = await _subscriptionService.CreateSubscriptionAsync(request);
    // Handle success
}
catch (PaymentException ex)
{
    // Handle payment-specific errors
    Console.WriteLine($"Payment Error: {ex.Message}");
    Console.WriteLine($"Error Code: {ex.ErrorCode}");
    Console.WriteLine($"Provider: {ex.Provider}");
}
catch (Exception ex)
{
    // Handle general errors
    Console.WriteLine($"General Error: {ex.Message}");
}
```

## 🔗 Data Models

### CreateSubscriptionRequest

```csharp
public class CreateSubscriptionRequest
{
    public string Email { get; set; }
    public string FullName { get; set; }
    public string CustomerId { get; set; }
    public string PaymentMethodId { get; set; }
    public string PriceId { get; set; }
    public int? TrialPeriodDays { get; set; }
    public Dictionary<string, string> Metadata { get; set; }
}
```

### CreateCustomerRequest

```csharp
public class CreateCustomerRequest
{
    public string Email { get; set; }
    public string Name { get; set; }
    public string Phone { get; set; }
    public string Description { get; set; }
    public CustomerAddress Address { get; set; }
    public Dictionary<string, string> Metadata { get; set; }
}
```

## 🔐 Security Considerations

- **API Keys**: Store Stripe API keys securely using environment variables or Azure Key Vault
- **Webhooks**: Validate webhook signatures to ensure requests are from Stripe
- **PCI Compliance**: Never store sensitive card data in your application
- **Logging**: Avoid logging sensitive payment information


## 📈 Monitoring and Logging

Captain Payment integrates with Microsoft.Extensions.Logging. Configure logging in your application:

```csharp
builder.Services.AddLogging(config =>
{
    config.AddConsole();
    config.AddDebug();
    config.SetMinimumLevel(LogLevel.Information);
});
```

## 🤝 Contributing

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/amazing-feature`)
3. Commit your changes (`git commit -m 'Add some amazing feature'`)
4. Push to the branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request

## 📝 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## 📞 Support

- **Documentation**: [GitHub Wiki](https://github.com/yourusername/captain-payment/wiki)
- **Issues**: [GitHub Issues](https://github.com/yourusername/captain-payment/issues)
- **Email**: general@tobikareem.com

## 🔄 Changelog

### v1.0.0
- Initial release
- Stripe integration for subscriptions, customers, and payments
- Comprehensive error handling
- Full async support

---
