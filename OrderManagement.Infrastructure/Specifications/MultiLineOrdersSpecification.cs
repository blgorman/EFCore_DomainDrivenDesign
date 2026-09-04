using Ardalis.Specification;
using OrderManagement.Domain.Aggregates;

namespace OrderManagement.Infrastructure.Specifications;

public class MultiLineOrdersSpecification : Specification<Order>
{
    public MultiLineOrdersSpecification()
    {
        Query.Where(o => o.Lines.Count > 1);
    }
}
