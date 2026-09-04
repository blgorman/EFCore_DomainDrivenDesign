using Microsoft.EntityFrameworkCore;
using OrderManagement.Application.Contracts;
using OrderManagement.Infrastructure.Data;

namespace OrderManagement.Infrastructure.ReadModels;

public class OrderReadRepository : IOrderReadRepository
{
    private readonly OrderingContext _ctx;

    public OrderReadRepository(OrderingContext ctx) => _ctx = ctx;

    public async Task<IReadOnlyList<OrderSummaryDto>> ListSummariesAsync(CancellationToken ct = default)
    {
        // Status maps to its column through a string conversion; converting it client-side
        // keeps the server projection translatable.
        var rows = await _ctx.Orders
            .AsNoTracking()
            .Select(o => new
            {
                o.Id,
                CustomerId = EF.Property<int>(o, "CustomerId"),
                o.Status,
                o.PlacedAt,
                LineCount = o.Lines.Count
            })
            .OrderByDescending(o => o.PlacedAt)
            .ToListAsync(ct);

        return rows
            .Select(r => new OrderSummaryDto(r.Id, r.CustomerId, r.Status.ToString(), r.PlacedAt, r.LineCount))
            .ToList();
    }
}
