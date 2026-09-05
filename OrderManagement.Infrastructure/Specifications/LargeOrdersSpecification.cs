using Ardalis.Specification;
using OrderManagement.Domain.Aggregates;

namespace OrderManagement.Infrastructure.Specifications;

public class LargeOrdersSpecification : Specification<Order>
{
    public LargeOrdersSpecification(decimal minimumTotal)
    {
        Query.Where(o => o.Total.Amount >= minimumTotal);
    }
}
