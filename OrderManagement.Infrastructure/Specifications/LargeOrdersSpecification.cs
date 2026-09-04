using Ardalis.Specification;
using OrderManagement.Domain.Aggregates;

namespace OrderManagement.Infrastructure.Specifications;

public class LargeOrdersSpecification : Specification<Order>
{
    public LargeOrdersSpecification(decimal minimumTotal)
    {
        //TODO: Module 6 Clip 5 — Uncomment the total filter below (requires having completed Module 2 Clip 4, which changes Total to Money):
        //Query.Where(o => o.Total.Amount >= minimumTotal);
    }
}
