using CaptainPayment.Core.Models;

namespace CaptainPayment.Core.Interfaces;

public interface ICustomerService
{
    Task<CustomerResult> CreateCustomerAsync(CreateCustomerRequest request, CancellationToken cancellationToken = default);
    Task<CustomerResult> GetCustomerAsync(string customerId, CancellationToken cancellationToken = default);
    Task<CustomerResult> UpdateCustomerAsync(string customerId, UpdateCustomerRequest request, CancellationToken cancellationToken = default);
    Task<bool> DeleteCustomerAsync(string customerId, CancellationToken cancellationToken = default);
    Task<PagedResult<CustomerResult>> ListCustomersAsync(CustomerListRequest request, CancellationToken cancellationToken = default);
    Task<CustomerResult> SearchCustomersAsync(string query, CancellationToken cancellationToken = default);
}
