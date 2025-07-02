using CaptainPayment.Core.Models;

namespace CaptainPayment.Core.Interfaces;

public interface IProductService
{
    // Product Management
    Task<ProductResult> CreateProductAsync(CreateProductRequest request, CancellationToken cancellationToken = default);
    Task<ProductResult> GetProductAsync(string productId, CancellationToken cancellationToken = default);
    Task<ProductResult> UpdateProductAsync(string productId, UpdateProductRequest request, CancellationToken cancellationToken = default);
    Task<bool> DeleteProductAsync(string productId, CancellationToken cancellationToken = default);
    Task<PagedResult<ProductResult>> ListProductsAsync(ProductListRequest request = null, CancellationToken cancellationToken = default);

    // Price Management
    Task<PriceResult> CreatePriceAsync(CreatePriceRequest request, CancellationToken cancellationToken = default);
    Task<PriceResult> GetPriceAsync(string priceId, CancellationToken cancellationToken = default);
    Task<PriceResult> UpdatePriceAsync(string priceId, UpdatePriceRequest request, CancellationToken cancellationToken = default);
    Task<PagedResult<PriceResult>> ListPricesAsync(PriceListRequest request = null, CancellationToken cancellationToken = default);
}