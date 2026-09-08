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

    public async Task<Order?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        return await _ctx.Orders
            .Include(o => o.Lines)
            .FirstOrDefaultAsync(o => o.Id == id, ct);
    }

    public void Add(Order order) => _ctx.Orders.Add(order);
    public void Remove(Order order) => _ctx.Orders.Remove(order);

    public void Add(Order order, int customerId)
    {
        _ctx.Orders.Add(order);
        _ctx.Entry(order).Property<int>("CustomerId").CurrentValue = customerId;
    }

    public async Task SaveAsync(CancellationToken ct = default)
    {
        await _ctx.SaveChangesAsync(ct);
    }

    public async Task<IReadOnlyList<Order>> ListAsync(ISpecification<Order> spec, CancellationToken ct = default)
    {
        var query = SpecificationEvaluator.Default.GetQuery(_ctx.Orders.AsQueryable(), spec);
        return await query.ToListAsync(ct);
    }
}
