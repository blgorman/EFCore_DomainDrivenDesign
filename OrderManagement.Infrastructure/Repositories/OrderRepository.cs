using Ardalis.Specification;
using Ardalis.Specification.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using OrderManagement.Domain.Aggregates;
using OrderManagement.Domain.Repositories;
using OrderManagement.Infrastructure.Data;

namespace OrderManagement.Infrastructure.Repositories;

public class OrderRepository : IOrderRepository
{
    private readonly OrderingContext _ctx;

    public OrderRepository(OrderingContext ctx) => _ctx = ctx;

    //TODO: Module 4 Clip 4 — Implement GetByIdAsync with Include so the full aggregate loads:
    //public async Task<Order?> GetByIdAsync(int id, CancellationToken ct = default)
    //{
    //    return await _ctx.Orders
    //        .Include(o => o.Lines)
    //        .FirstOrDefaultAsync(o => o.Id == id, ct);
    //}
    public Task<Order?> GetByIdAsync(int id, CancellationToken ct = default)
        => Task.FromResult<Order?>(null);

    //TODO: Module 4 Clip 4 — Implement Add and Remove (change-tracking only, no SQL issued yet):
    //public void Add(Order order) => _ctx.Orders.Add(order);
    //public void Remove(Order order) => _ctx.Orders.Remove(order);
    public void Add(Order order) { }
    public void Remove(Order order) { }

    //TODO: Module 4 Clip 4 — Implement the Add overload that sets the shadow CustomerId FK:
    //public void Add(Order order, int customerId)
    //{
    //    _ctx.Orders.Add(order);
    //    _ctx.Entry(order).Property<int>("CustomerId").CurrentValue = customerId;
    //}
    public void Add(Order order, int customerId) { }

    //TODO: Module 4 Clip 4 — Implement SaveAsync to flush all pending ChangeTracker entries:
    //public async Task SaveAsync(CancellationToken ct = default)
    //{
    //    await _ctx.SaveChangesAsync(ct);
    //}
    public Task SaveAsync(CancellationToken ct = default) => Task.CompletedTask;

    //TODO: Module 4 Clip 4 — Implement ListAsync using SpecificationEvaluator:
    //public async Task<IReadOnlyList<Order>> ListAsync(ISpecification<Order> spec, CancellationToken ct = default)
    //{
    //    var query = SpecificationEvaluator.Default.GetQuery(_ctx.Orders.AsQueryable(), spec);
    //    return await query.ToListAsync(ct);
    //}
    public Task<IReadOnlyList<Order>> ListAsync(ISpecification<Order> spec, CancellationToken ct = default)
        => Task.FromResult<IReadOnlyList<Order>>(Array.Empty<Order>());
}
