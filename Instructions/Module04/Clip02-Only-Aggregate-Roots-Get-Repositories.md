# Clip02 - Only Aggregate Roots Get Repositories
Explains why a child entity (`OrderLine`) never gets its own repository, and why the absence of `IOrderLineRepository` from the domain assembly is what keeps callers going through `Order`.

## Demo Class
`DemonstrateOrderRepositoryInterface.WalkInterfaceAsync()` in `ConsoleAppProject/CodeAndDemonstrations/Module04/`.

Run it from Module 4 → Option 2 — Only Aggregate Roots Get Repositories. The demo runs entirely in memory and needs no database.

## Tests
No unit tests are introduced in this clip.

## Student Changes
None. No code changes for this clip.
