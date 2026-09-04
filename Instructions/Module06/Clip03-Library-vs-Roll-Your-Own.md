# Clip03 - Library vs. Roll-Your-Own: Ardalis.Specification vs. Custom Base Class

Compare installing `Ardalis.Specification` against writing the `Specification<T>` base class and the evaluator by hand, and review the trade-off this solution accepts by letting `IOrderRepository` accept `ISpecification<Order>`. Full slide clip with no IDE demo. No code changes in this clip.

## Demo Class

None. There is no menu option for this clip.

## Tests

No tests are introduced in this clip.

## Files To Edit

None. There are no `//TODO: Module 6 Clip 3` markers anywhere in the solution.

## What To Verify

- `OrderManagement.Domain/OrderManagement.Domain.csproj` references `Ardalis.Specification`, and `OrderManagement.Infrastructure/OrderManagement.Infrastructure.csproj` references `Ardalis.Specification.EntityFrameworkCore`. Both packages are already installed, so there is nothing to add.
- `OrderManagement.Domain/Repositories/IOrderRepository.cs` declares `ListAsync(ISpecification<Order> spec, CancellationToken ct = default)`, which is why the Domain project references `Ardalis.Specification`. The comment above the interface records that trade-off.
- `OrderManagement.Infrastructure/Specifications/OrdersByCustomerSpecification.cs` extends `Specification<Order>` and builds its query through the `Query` builder.
- The solution builds with zero compiler errors.
