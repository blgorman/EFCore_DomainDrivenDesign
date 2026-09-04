using Ardalis.Specification;
using Microsoft.EntityFrameworkCore;
using OrderManagement.Domain.Aggregates;

namespace OrderManagement.Infrastructure.Specifications;

public class OrdersByCustomerSpecification : Specification<Order>
{
    public OrdersByCustomerSpecification(int customerId)
    {
        //TODO: Module 6 Clip 2 — Uncomment the specification predicate below:
        //Query.Where(o => EF.Property<int>(o, "CustomerId") == customerId);

        //TODO: Module 6 Clip 4 — Uncomment Include and OrderBy below:
        //Query.Include(o => o.Lines).OrderByDescending(o => o.PlacedAt);
    }
}
