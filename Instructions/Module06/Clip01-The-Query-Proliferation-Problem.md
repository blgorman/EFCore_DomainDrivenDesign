# Clip01 - The Query Proliferation Problem
Run the fully working proliferation demo to see how ad-hoc LINQ queries accumulate across service code and why the Specification pattern exists. No code changes in this clip.

## Demo Class
- DemonstrateQueryProliferation.ShowProliferationAsync

## Tests
No tests are introduced in this clip.

## Files To Edit
None. The demo has no `//TODO` gate and runs complete out of the box.

## What To Verify
- The demo seeds eight orders (`SeedDataHelper.SeedAsync`): six `Placed`, one `Shipped`, one `Cancelled`.
- Screen 1 prints live counts for four ad-hoc queries: 6 placed, 6 open, 2 open with more than one line, 1 shipped.
- Screen 2 previews the solution shape: `new OrdersByCustomerSpecification(customerId)` passed to `repository.ListAsync(spec)`.
