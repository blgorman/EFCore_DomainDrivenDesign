using Ardalis.Specification;
using Microsoft.EntityFrameworkCore;
using OrderManagement.Domain.Aggregates;

namespace OrderManagement.Infrastructure.Specifications;

public class OrdersByCustomerSpecification : Specification<Order>
{
    public OrdersByCustomerSpecification(int customerId)
    {
        Query.Where(o => EF.Property<int>(o, "CustomerId") == customerId);

        Query.Include(o => o.Lines).OrderByDescending(o => o.PlacedAt);
    }
}
