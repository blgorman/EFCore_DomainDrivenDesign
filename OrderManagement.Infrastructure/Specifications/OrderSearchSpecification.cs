using Ardalis.Specification;
using OrderManagement.Domain.Aggregates;
using OrderManagement.Domain.Enums;

namespace OrderManagement.Infrastructure.Specifications;

public class OrderSearchSpecification : Specification<Order>
{
    public OrderSearchSpecification(OrderStatus? status, int? minLines)
    {
        if (status.HasValue)
            Query.Where(o => o.Status == status.Value);

        if (minLines.HasValue)
            Query.Where(o => o.Lines.Count >= minLines.Value);

        Query.Include(o => o.Lines).OrderByDescending(o => o.PlacedAt);
    }
}
