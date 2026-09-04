# Clip03 - Defining the Repository Interface in the Domain Layer
Proves the Dependency Rule at runtime rather than asserting it. Reads `typeof(IOrderRepository).Assembly` to show `IOrderRepository` compiled into `OrderManagement.Domain`, resolves `IOrderRepository` through dependency injection to show the concrete type coming from `OrderManagement.Infrastructure`, and scans every assembly `OrderManagement.Domain` references to confirm none of them is `EntityFrameworkCore`.

## Demo Class
- DemonstrateRepositoryInterfaceInDomain.WalkDomainOwnershipAsync

## Tests
No unit tests are introduced in this clip.

## Files To Edit
- ConsoleAppProject/CodeAndDemonstrations/Module04/DemonstrateRepositoryInterfaceInDomain.cs

## What To Verify
The demo runs five screens once your changes are in. Before your changes it also runs five, and the fifth screen is the "Not Yet Implemented" box.

- Part 3 prints `typeof(IOrderRepository).Assembly.GetName().Name` and the resolved `OrderRepository` instance's assembly name — two different assemblies (`OrderManagement.Domain` vs `OrderManagement.Infrastructure`).
- Part 3b prints "Confirmed: Domain has zero EF Core references." — not the warning branch.
- Part 3b's parameter-type list shows only BCL types, `Order`, and `Ardalis.Specification` types — no `Microsoft.EntityFrameworkCore` types anywhere in the list.

## Student Changes

`IOrderRepository.cs` ships complete. This clip proves the Dependency Rule at runtime instead of writing the interface, so your work is unlocking the reflection that proves it.

### 1. Delete the "Not Yet Implemented" box
- File: ConsoleAppProject/CodeAndDemonstrations/Module04/DemonstrateRepositoryInterfaceInDomain.cs
- Method: WalkDomainOwnershipAsync
- Find TODO marker: `Module 4 Clip 3 — Delete the "Not Yet Implemented" box below.`
- Action: Delete the TODO comment and the `Console.Write(OutputHelpers.BoxedArrayWithTitle("Not Yet Implemented", ...));` call directly below it. Leave the Part 3b section banner line above the TODO in place.

### 2. Uncomment the Part 3b block
- File: same
- Find TODO marker: `Module 4 Clip 3 — Uncomment the Part 3b block below.`
- Action: Delete the TODO comment and remove the `//` prefix from every line in the block below it. The block declares `domainAssembly`, `efCoreReference`, and `methodParamTypes`, then prints them. The `WaitForUserInput` separator already sitting below this block stays where it is.
