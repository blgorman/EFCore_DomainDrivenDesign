using Ardalis.Specification;
using OrderManagement.Domain.Aggregates;
using OrderManagement.Domain.Enums;

namespace OrderManagement.Infrastructure.Specifications;

public class OrdersByStatusSpecification : Specification<Order>
{
    public OrdersByStatusSpecification(OrderStatus status)
    {
        //TODO: Module 6 Clip 5 — Uncomment the specification predicate below:
        //Query.Where(o => o.Status == status);
    }
}
