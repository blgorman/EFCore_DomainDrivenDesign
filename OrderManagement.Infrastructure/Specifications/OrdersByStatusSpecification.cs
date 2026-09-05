using Ardalis.Specification;
using OrderManagement.Domain.Aggregates;
using OrderManagement.Domain.Enums;

namespace OrderManagement.Infrastructure.Specifications;

public class OrdersByStatusSpecification : Specification<Order>
{
    public OrdersByStatusSpecification(OrderStatus status)
    {
        Query.Where(o => o.Status == status);
    }
}
