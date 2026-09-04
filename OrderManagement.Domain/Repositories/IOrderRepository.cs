using Ardalis.Specification;
using OrderManagement.Domain.Aggregates;

namespace OrderManagement.Domain.Repositories;

// Design trade-off: ISpecification<T> is from Ardalis.Specification, which means the domain
// project takes a dependency on a third-party library. This is a deliberate pragmatic choice:
// Ardalis.Specification is a thin, stable abstraction that adds no infrastructure concerns to
// the domain. The alternative (defining our own ISpecification<T> marker here and bridging it
// in Infrastructure) adds indirection without meaningful gain for this project's scope.
// This trade-off is covered explicitly in the Module 6 Specification Pattern demos.
public interface IOrderRepository
{
    Task<Order?> GetByIdAsync(int id, CancellationToken ct = default);
    void Add(Order order);
    void Add(Order order, int customerId);
    void Remove(Order order);
    Task SaveAsync(CancellationToken ct = default);
    Task<IReadOnlyList<Order>> ListAsync(ISpecification<Order> spec, CancellationToken ct = default);
}
