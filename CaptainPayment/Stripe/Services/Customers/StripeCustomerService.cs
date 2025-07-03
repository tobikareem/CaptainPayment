using CaptainPayment.Core.Config;
using CaptainPayment.Core.Exceptions;
using CaptainPayment.Core.Interfaces;
using CaptainPayment.Core.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Stripe;

namespace CaptainPayment.Stripe.Services.Customers;

public class StripeCustomerService : ICustomerService
{
    private readonly ILogger<StripeCustomerService> _logger;
    private readonly StripeSettings _settings;

    public StripeCustomerService(IOptions<StripeSettings> options, ILogger<StripeCustomerService> logger)
    {
        _settings = options.Value ?? throw new ArgumentNullException(nameof(options));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        ValidateConfiguration();
        StripeConfiguration.ApiKey = _settings.SecretKey;
    }

    public async Task<CustomerResult> CreateCustomerAsync(CreateCustomerRequest request, CancellationToken cancellationToken = default)
    {
        ValidateCreateCustomerRequest(request);

        try
        {
            _logger.LogInformation("Creating Stripe customer for email: {Email}", request.Email);

            var options = BuildCustomerCreateOptions(request);
            var service = new CustomerService();
            var customer = await service.CreateAsync(options, cancellationToken: cancellationToken);

            _logger.LogInformation("Customer created successfully: {CustomerId}", customer.Id);
            return MapToCustomerResult(customer);
        }
        catch (StripeException ex)
        {
            _logger.LogError(ex, "Stripe error while creating customer for {Email}", request.Email);
            throw new PaymentException($"Stripe customer creation failed: {ex.Message}", ex.HttpStatusCode.ToString(), _settings.ProviderName);
        }
    }

    public async Task<CustomerResult> GetCustomerAsync(string customerId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(customerId))
            throw new ArgumentException("Customer ID cannot be null or empty", nameof(customerId));

        try
        {
            var service = new CustomerService();
            var customer = await service.GetAsync(customerId, new CustomerGetOptions
            {
                Expand = new List<string> { "default_source", "subscriptions", "tax_ids" }
            }, cancellationToken: cancellationToken);

            return MapToCustomerResult(customer);
        }
        catch (StripeException ex)
        {
            _logger.LogError(ex, "Failed to retrieve customer {CustomerId}", customerId);
            throw new PaymentException($"Failed to retrieve customer: {ex.Message}",
                ex.HttpStatusCode.ToString(), _settings.ProviderName);
        }
    }

    public async Task<CustomerResult> UpdateCustomerAsync(string customerId, UpdateCustomerRequest request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(customerId))
            throw new ArgumentException("Customer ID cannot be null or empty", nameof(customerId));

        try
        {
            var service = new CustomerService();
            var options = new CustomerUpdateOptions
            {
                Email = request.Email,
                Name = request.Name,
                Phone = request.Phone,
                Description = request.Description,
                Address = MapToStripeAddress(request.Address),
                Shipping = MapToStripeShipping(request.ShippingAddress, request.Name),
                Metadata = request.Metadata
            };
            
            if (!string.IsNullOrWhiteSpace(request.DefaultPaymentMethodId))
            {
                options.InvoiceSettings = new CustomerInvoiceSettingsOptions
                {
                    DefaultPaymentMethod = request.DefaultPaymentMethodId
                };
            }

            var customer = await service.UpdateAsync(customerId, options, cancellationToken: cancellationToken);

            _logger.LogInformation("Customer updated successfully: {CustomerId}", customerId);
            return MapToCustomerResult(customer);
        }
        catch (StripeException ex)
        {
            _logger.LogError(ex, "Failed to update customer {CustomerId}", customerId);
            throw new PaymentException($"Failed to update customer: {ex.Message}",
                ex.HttpStatusCode.ToString(), _settings.ProviderName);
        }
    }

    public async Task<bool> DeleteCustomerAsync(string customerId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(customerId))
            throw new ArgumentException("Customer ID cannot be null or empty", nameof(customerId));

        try
        {
            var service = new CustomerService();
            var deletedCustomer = await service.DeleteAsync(customerId, cancellationToken: cancellationToken);

            _logger.LogInformation("Customer deleted successfully: {CustomerId}", customerId);
            return deletedCustomer.Deleted ?? false;
        }
        catch (StripeException ex)
        {
            _logger.LogError(ex, "Failed to delete customer {CustomerId}", customerId);
            throw new PaymentException($"Failed to delete customer: {ex.Message}",
                ex.HttpStatusCode.ToString(), _settings.ProviderName);
        }
    }

    public async Task<PagedResult<CustomerResult>> ListCustomersAsync(CustomerListRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var service = new CustomerService();

            var options = new CustomerListOptions
            {
                Limit = request.Limit ?? 10,
                StartingAfter = request.StartingAfter,
                EndingBefore = request.EndingBefore,
                Email = request.Email,
            };

            if (request.Created != null)
            {
                options.Created = new DateRangeOptions
                {
                    GreaterThan = request.Created.GreaterThan,
                    LessThan = request.Created.LessThan
                };
            }

            var stripeCustomers = await service.ListAsync(options, cancellationToken: cancellationToken);

            return new PagedResult<CustomerResult>
            {
                Data = stripeCustomers.Data.Select(MapToCustomerResult).ToList(),
                HasMore = stripeCustomers.HasMore,
                TotalCount = stripeCustomers.Data.Count
            };
        }
        catch (StripeException ex)
        {
            _logger.LogError(ex, "Error while listing Stripe customers");
            throw new PaymentException($"Failed to list customers: {ex.Message}", ex.HttpStatusCode.ToString(), _settings.ProviderName);
        }
    }


    public async Task<CustomerResult> SearchCustomersAsync(string query, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(query))
            throw new ArgumentException("Search query cannot be null or empty", nameof(query));

        try
        {
            var service = new CustomerService();
            
            var customers = await service.ListAsync(new CustomerListOptions
            {
                Email = query,
                Limit = 1
            }, cancellationToken: cancellationToken);

            var customer = customers.Data.FirstOrDefault();
            if (customer == null)
            {
                return new CustomerResult();
            }

            _logger.LogInformation("Customer found by email: {Email}", query);
            return MapToCustomerResult(customer);

        }
        catch (StripeException ex)
        {
            _logger.LogError(ex, "Failed to search customers with query: {Query}", query);
            throw new PaymentException($"Failed to search customers: {ex.Message}",
                ex.HttpStatusCode.ToString(), _settings.ProviderName);
        }
    }


    private static CustomerCreateOptions BuildCustomerCreateOptions(CreateCustomerRequest request)
    {
        var options = new CustomerCreateOptions
        {
            Email = request.Email,
            Name = request.Name,
            Phone = request.Phone,
            Description = request.Description,
            TaxExempt = request.TaxExempt,
            Metadata = request.Metadata
        };
        
        if (request.Address != null && !string.IsNullOrWhiteSpace(request.Address.Line1))
        {
            options.Address = MapToStripeAddress(request.Address);
        }
        
        if (request.ShippingAddress != null && !string.IsNullOrWhiteSpace(request.ShippingAddress.Line1))
        {
            options.Shipping = MapToStripeShipping(request.ShippingAddress, request.Name);
        }
        
        if (!string.IsNullOrWhiteSpace(request.DefaultPaymentMethodId))
        {
            options.PaymentMethod = request.DefaultPaymentMethodId;
            options.InvoiceSettings = new CustomerInvoiceSettingsOptions
            {
                DefaultPaymentMethod = request.DefaultPaymentMethodId
            };
        }

        return options;
    }

    private static void ValidateCreateCustomerRequest(CreateCustomerRequest request)
    {
        if (request == null)
            throw new ArgumentNullException(nameof(request));

        if (string.IsNullOrWhiteSpace(request.Email))
            throw new ArgumentException("Email is required", nameof(request));
    }

    private static AddressOptions MapToStripeAddress(CustomerAddress address)
    {
        return new AddressOptions
        {
            Line1 = address.Line1,
            Line2 = address.Line2,
            City = address.City,
            State = address.State,
            PostalCode = address.PostalCode,
            Country = address.Country
        };
    }

    private static ShippingOptions MapToStripeShipping(CustomerAddress address, string name)
    {

        return new ShippingOptions
        {
            Name = name,
            Address = MapToStripeAddress(address)
        };
    }

    private static CustomerResult MapToCustomerResult(Customer customer)
    {
        return new CustomerResult
        {
            Id = customer.Id,
            Email = customer.Email,
            Name = customer.Name,
            Phone = customer.Phone,
            Description = customer.Description,
            Created = customer.Created,
            Currency = customer.Currency,
            Balance = customer.Balance,
            Delinquent = customer.Delinquent.GetValueOrDefault(),
            DefaultSource = customer.DefaultSourceId,
            InvoicePrefix = customer.InvoicePrefix,
            Address = (customer.Address != null ? MapFromStripeAddress(customer.Address) : null) ?? new CustomerAddress(),
            ShippingAddress = (customer.Shipping?.Address != null ? MapFromStripeAddress(customer.Shipping.Address) : null) ?? new CustomerAddress(),
            TaxExempt = customer.TaxExempt,
            Metadata = customer.Metadata ?? new Dictionary<string, string>(),
            LiveMode = customer.Livemode
        };
    }

    private static CustomerAddress MapFromStripeAddress(Address address)
    {
        return new CustomerAddress
        {
            Line1 = address.Line1,
            Line2 = address.Line2,
            City = address.City,
            State = address.State,
            PostalCode = address.PostalCode,
            Country = address.Country
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
