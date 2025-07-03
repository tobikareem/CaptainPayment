using CaptainPayment.Core.Config;
using CaptainPayment.Core.Exceptions;
using CaptainPayment.Core.Interfaces;
using CaptainPayment.Core.Models;
using CaptainPayment.Stripe.Extension;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Stripe;

namespace CaptainPayment.Stripe.Services.Products;

public class StripeProductService : IProductService
{
    private readonly ILogger<StripeProductService> _logger;
    private readonly StripeSettings _settings;

    public StripeProductService(IOptions<StripeSettings> options, ILogger<StripeProductService> logger)
    {
        _settings = options.Value ?? throw new ArgumentNullException(nameof(options));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        ValidateConfiguration();
        StripeConfiguration.ApiKey = _settings.SecretKey;
    }

    #region Product Management

    public async Task<ProductResult> CreateProductAsync(CreateProductRequest request, CancellationToken cancellationToken = default)
    {
        ValidateCreateProductRequest(request);

        try
        {
            _logger.LogInformation("Creating product: {ProductName}", request.Name);

            var service = new ProductService();
            var options = new ProductCreateOptions
            {
                Name = request.Name,
                Description = request.Description,
                Active = request.Active ?? true,
                Images = request.Images.Any() ? request.Images : null,
                Metadata = request.Metadata.Any() ? request.Metadata : null, Shippable = request.Shippable,
                StatementDescriptor = !string.IsNullOrWhiteSpace(request.StatementDescriptor) ? request.StatementDescriptor : null,
                TaxCode = !string.IsNullOrWhiteSpace(request.TaxCode) ? request.TaxCode : null,
                UnitLabel = !string.IsNullOrWhiteSpace(request.UnitLabel) ? request.UnitLabel : null,
                Url = !string.IsNullOrWhiteSpace(request.Url) ? request.Url : null,
            };

            if (request.PackageDimensions.Height > 0 ||
                request.PackageDimensions.Length > 0 ||
                request.PackageDimensions.Weight > 0 ||
                request.PackageDimensions.Width > 0)
            {
                options.PackageDimensions = new ProductPackageDimensionsOptions
                {
                    Height = request.PackageDimensions.Height,
                    Length = request.PackageDimensions.Length,
                    Weight = request.PackageDimensions.Weight,
                    Width = request.PackageDimensions.Width
                };
            }

            if (!string.IsNullOrWhiteSpace(request.Currency) && request.AmountInCents > 0)
            {
                options.DefaultPriceData = new ProductDefaultPriceDataOptions
                {
                    Currency = request.Currency.ToLowerInvariant(),
                    UnitAmount = request.AmountInCents
                };
                
                if (request.Interval != StripeInterval.None) 
                {
                    options.DefaultPriceData.Recurring = new ProductDefaultPriceDataRecurringOptions
                    {
                        Interval = request.Interval.GetDisplayName(),
                        IntervalCount = 1
                    };
                }
            }

            var product = await service.CreateAsync(options, cancellationToken: cancellationToken);

            _logger.LogInformation("Product created successfully: {ProductId}", product.Id);
            return MapToProductResult(product);
        }
        catch (StripeException ex)
        {
            _logger.LogError(ex, "Failed to create product");
            throw new PaymentException($"Failed to create product: {ex.Message}",
                ex.HttpStatusCode.ToString(), _settings.ProviderName);
        }
    }

    public async Task<ProductResult> GetProductAsync(string productId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(productId))
            throw new ArgumentException("Product ID cannot be null or empty", nameof(productId));

        try
        {
            var service = new ProductService();
            var product = await service.GetAsync(productId, cancellationToken: cancellationToken);
            return MapToProductResult(product);
        }
        catch (StripeException ex)
        {
            _logger.LogError(ex, "Failed to retrieve product {ProductId}", productId);
            throw new PaymentException($"Failed to retrieve product: {ex.Message}",
                ex.HttpStatusCode.ToString(), _settings.ProviderName);
        }
    }

    public async Task<ProductResult> UpdateProductAsync(string productId, UpdateProductRequest request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(productId))
            throw new ArgumentException("Product ID cannot be null or empty", nameof(productId));

        try
        {
            var service = new ProductService();
            var options = new ProductUpdateOptions
            {
                Name = request.Name,
                Description = request.Description,
                Active = request.Active,
                Images = request.Images,
                Metadata = request.Metadata,
                StatementDescriptor = request.StatementDescriptor,
                TaxCode = request.TaxCode,
                UnitLabel = request.UnitLabel,
                Url = request.Url
            };

            var product = await service.UpdateAsync(productId, options, cancellationToken: cancellationToken);

            _logger.LogInformation("Product updated successfully: {ProductId}", productId);
            return MapToProductResult(product);
        }
        catch (StripeException ex)
        {
            _logger.LogError(ex, "Failed to update product {ProductId}", productId);
            throw new PaymentException($"Failed to update product: {ex.Message}",
                ex.HttpStatusCode.ToString(), _settings.ProviderName);
        }
    }

    public async Task<bool> DeleteProductAsync(string productId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(productId))
            throw new ArgumentException("Product ID cannot be null or empty", nameof(productId));

        try
        {
            var service = new ProductService();
            var deletedProduct = await service.DeleteAsync(productId, cancellationToken: cancellationToken);

            _logger.LogInformation("Product deleted successfully: {ProductId}", productId);
            return deletedProduct.Deleted ?? false;
        }
        catch (StripeException ex)
        {
            _logger.LogError(ex, "Failed to delete product {ProductId}", productId);
            throw new PaymentException($"Failed to delete product: {ex.Message}",
                ex.HttpStatusCode.ToString(), _settings.ProviderName);
        }
    }

    public async Task<PagedResult<ProductResult>> ListProductsAsync(ProductListRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var service = new ProductService();
            var options = new ProductListOptions
            {
                Limit = request.Limit ?? 10,
                StartingAfter = request.StartingAfter,
                EndingBefore = request.EndingBefore,
                Active = request.Active,
                Created = request.Created != null ? new DateRangeOptions
                {
                    GreaterThan = request.Created.GreaterThan,
                    LessThan = request.Created.LessThan
                } : null,
                Ids = request.Ids,
                Shippable = request.Shippable,
                Url = request.Url
            };

            var products = await service.ListAsync(options, cancellationToken: cancellationToken);

            return new PagedResult<ProductResult>
            {
                Data = products.Data.Select(MapToProductResult).ToList(),
                HasMore = products.HasMore,
                TotalCount = products.Data.Count
            };
        }
        catch (StripeException ex)
        {
            _logger.LogError(ex, "Failed to list products");
            throw new PaymentException($"Failed to list products: {ex.Message}",
                ex.HttpStatusCode.ToString(), _settings.ProviderName);
        }
    }

    #endregion

    #region Price Management

    public async Task<PriceResult> CreatePriceAsync(CreatePriceRequest request, CancellationToken cancellationToken = default)
    {
        ValidateCreatePriceRequest(request);

        try
        {
            _logger.LogInformation("Creating price for product: {ProductId}", request.ProductId);

            var service = new PriceService();
            var options = new PriceCreateOptions
            {
                Product = request.ProductId,
                Currency = request.Currency.ToLowerInvariant(),
                UnitAmount = (long)request.UnitAmount,
                Active = request.Active ?? true,
                Metadata = request.Metadata.Any() ? request.Metadata : null,
                Nickname = !string.IsNullOrWhiteSpace(request.Nickname) ? request.Nickname : null,
                TaxBehavior = !string.IsNullOrWhiteSpace(request.TaxBehavior) ? request.TaxBehavior : null,
            };

            if (request.Recurring.Interval != StripeInterval.None)
            {
                options.Recurring = new PriceRecurringOptions
                {
                    Interval = request.Recurring.Interval.GetDisplayName(),
                    IntervalCount = request.Recurring.IntervalCount ?? 1,
                };
                
                if (request.Recurring.UsageType != StripeUsageType.None)
                {
                    options.Recurring.UsageType = request.Recurring.UsageType.GetDisplayName();
                }
            }

            // Handle tiered pricing
            if (request.Tiers.Any())
            {
                options.BillingScheme = "tiered";
                options.TiersMode = request.TiersMode;
                options.Tiers = request.Tiers.Select(t => new PriceTierOptions
                {
                    UpTo = t.UpTo,
                    UnitAmount = t.UnitAmount.HasValue ? (long)t.UnitAmount.Value : null,
                    FlatAmount = t.FlatAmount.HasValue ? (long)t.FlatAmount.Value : null
                }).ToList();
            }

            var price = await service.CreateAsync(options, cancellationToken: cancellationToken);

            _logger.LogInformation("Price created successfully: {PriceId}", price.Id);
            return MapToPriceResult(price);
        }
        catch (StripeException ex)
        {
            _logger.LogError(ex, "Failed to create price");
            throw new PaymentException($"Failed to create price: {ex.Message}",
                ex.HttpStatusCode.ToString(), _settings.ProviderName);
        }
    }

    public async Task<PriceResult> GetPriceAsync(string priceId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(priceId))
            throw new ArgumentException("Price ID cannot be null or empty", nameof(priceId));

        try
        {
            var service = new PriceService();
            var price = await service.GetAsync(priceId, cancellationToken: cancellationToken);
            return MapToPriceResult(price);
        }
        catch (StripeException ex)
        {
            _logger.LogError(ex, "Failed to retrieve price {PriceId}", priceId);
            throw new PaymentException($"Failed to retrieve price: {ex.Message}",
                ex.HttpStatusCode.ToString(), _settings.ProviderName);
        }
    }

    public async Task<PriceResult> UpdatePriceAsync(string priceId, UpdatePriceRequest request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(priceId))
            throw new ArgumentException("Price ID cannot be null or empty", nameof(priceId));

        try
        {
            var service = new PriceService();
            var options = new PriceUpdateOptions
            {
                Active = request.Active,
                Metadata = request.Metadata,
                Nickname = request.Nickname,
                TaxBehavior = request.TaxBehavior
            };

            var price = await service.UpdateAsync(priceId, options, cancellationToken: cancellationToken);

            _logger.LogInformation("Price updated successfully: {PriceId}", priceId);
            return MapToPriceResult(price);
        }
        catch (StripeException ex)
        {
            _logger.LogError(ex, "Failed to update price {PriceId}", priceId);
            throw new PaymentException($"Failed to update price: {ex.Message}",
                ex.HttpStatusCode.ToString(), _settings.ProviderName);
        }
    }

    public async Task<PagedResult<PriceResult>> ListPricesAsync(PriceListRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var service = new PriceService();
            var options = new PriceListOptions
            {
                Limit = request.Limit ?? 10,
                StartingAfter = request.StartingAfter,
                EndingBefore = request.EndingBefore,
                Active = request.Active,
                Currency = request.Currency,
                Product = request.ProductId,
                Type = request.Type,
                Created = request.Created != null ? new DateRangeOptions
                {
                    GreaterThan = request.Created.GreaterThan,
                    LessThan = request.Created.LessThan
                } : null
            };

            var prices = await service.ListAsync(options, cancellationToken: cancellationToken);

            return new PagedResult<PriceResult>
            {
                Data = prices.Data.Select(MapToPriceResult).ToList(),
                HasMore = prices.HasMore,
                TotalCount = prices.Data.Count
            };
        }
        catch (StripeException ex)
        {
            _logger.LogError(ex, "Failed to list prices");
            throw new PaymentException($"Failed to list prices: {ex.Message}",
                ex.HttpStatusCode.ToString(), _settings.ProviderName);
        }
    }

    #endregion

    private void ValidateConfiguration()
    {
        if (string.IsNullOrWhiteSpace(_settings.SecretKey))
            throw new ArgumentException("Stripe Secret Key is not configured", nameof(_settings.SecretKey));
        if (string.IsNullOrWhiteSpace(_settings.PublishableKey))
            throw new ArgumentException("Stripe Publishable Key is not configured", nameof(_settings.PublishableKey));
    }

    private static void ValidateCreateProductRequest(CreateProductRequest request)
    {
        if (request == null)
            throw new ArgumentNullException(nameof(request));

        if (string.IsNullOrWhiteSpace(request.Name))
            throw new ArgumentException("Product name is required", nameof(request.Name));

        if (request.Name.Length > 250)
            throw new ArgumentException("Product name cannot exceed 250 characters", nameof(request.Name));

        if (!string.IsNullOrWhiteSpace(request.Currency))
        {
            if (request.Currency.Length != 3)
                throw new ArgumentException("Currency must be a 3-letter ISO code", nameof(request.Currency));

            if (request.AmountInCents <= 0)
                throw new ArgumentException("Amount must be greater than 0 when currency is provided",
                    nameof(request.AmountInCents));
        }

        if (!string.IsNullOrWhiteSpace(request.StatementDescriptor) &&
            request.StatementDescriptor.Length > 22)
            throw new ArgumentException("Statement descriptor cannot exceed 22 characters",
                nameof(request.StatementDescriptor));

        if (!string.IsNullOrWhiteSpace(request.Url) &&
            !Uri.TryCreate(request.Url, UriKind.Absolute, out _))
            throw new ArgumentException("Invalid URL format", nameof(request.Url));

        if (request.Images.Count > 8)
            throw new ArgumentException("Cannot have more than 8 images", nameof(request.Images));

        if (request.Metadata.Count > 50)
            throw new ArgumentException("Cannot have more than 50 metadata keys", nameof(request.Metadata));

        if (request.Metadata.Any(kvp =>
                kvp.Key.Length > 40 || kvp.Value.Length > 500))
            throw new ArgumentException("Metadata keys cannot exceed 40 characters and values cannot exceed 500 characters");
    }

    private static void ValidateCreatePriceRequest(CreatePriceRequest request)
    {
        if (request == null)
            throw new ArgumentNullException(nameof(request));

        if (string.IsNullOrWhiteSpace(request.ProductId))
            throw new ArgumentException("Product ID is required", nameof(request));

        if (string.IsNullOrWhiteSpace(request.Currency))
            throw new ArgumentException("Currency is required", nameof(request));

        if (request.UnitAmount <= 0)
            throw new ArgumentException("Unit amount must be greater than zero", nameof(request));
    }

    private static ProductResult MapToProductResult(Product product)
    {
        return new ProductResult
        {
            Id = product.Id,
            Name = product.Name,
            Description = product.Description,
            Active = product.Active,
            Created = product.Created,
            Updated = product.Updated,
            Images = product.Images?.ToList() ?? new List<string>(),
            Metadata = product.Metadata,
            Shippable = product.Shippable,
            StatementDescriptor = product.StatementDescriptor,
            TaxCode = product.TaxCodeId,
            UnitLabel = product.UnitLabel,
            Url = product.Url,
            LiveMode = product.Livemode
        };
    }

    private static PriceResult MapToPriceResult(Price price)
    {
        return new PriceResult
        {
            Id = price.Id,
            ProductId = price.ProductId,
            Active = price.Active,
            BillingScheme = price.BillingScheme,
            Created = price.Created,
            Currency = price.Currency,
            Nickname = price.Nickname,
            UnitAmount = (decimal)(price.UnitAmount ?? 0) / 100,
            UnitAmountDecimal = price.UnitAmountDecimal,
            Type = price.Type,
            TaxBehavior = price.TaxBehavior,
            Metadata = price.Metadata,
            LiveMode = price.Livemode
        };
    }
}