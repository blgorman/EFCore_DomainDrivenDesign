using OrderManagement.Domain.Entities;

namespace OrderManagement.Domain.Repositories;

public interface ICustomerRepository
{
    Task<Customer?> GetByIdAsync(int id, CancellationToken ct = default);
    void Add(Customer customer);
    Task SaveAsync(CancellationToken ct = default);
}
