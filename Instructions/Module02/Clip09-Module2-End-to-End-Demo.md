# Clip09 - Module2 End to End Demo
End-to-end flow proves domain rules and EF persistence work together.

## Demo Class
- DemonstrateOrderAggregate.RunEndToEndAsync

## What To Verify
- Aggregate creation, mutation, and state transitions all pass.
- Ship then Cancel guard is enforced.
- Reload checks pass for status, lines, and money values.
- Full-solution run also shows Total output/assertions (starter shows TODO placeholders until Clip08 is complete).
- Change-tracker bypass section demonstrates boundary limitations.

## Student Changes
1. Confirm preconditions
   - Action: Verify Clips 2, 3, 4, 5, and 8 are already applied before continuing.

2. Replace the NTI block and uncomment the full demo body
   - File: ConsoleAppProject/CodeAndDemonstrations/Module02/DemonstrateOrderAggregate.cs
   - Method: RunEndToEndAsync
   - Action:
     1. Delete lines 14 through 31 — the entire "Not Yet Implemented" box, from the `SectionBanner("Order Aggregate — End to End")` line down to and including the `));` that closes it.
     2. Delete lines 33 through 36 — the `//TODO: Module 2 Clip 9` comment and the three WARNING lines beneath it.
     3. Remove the `//` prefix from lines 38 through 383, the remaining commented-out demo body.

3. Run the end-to-end demo
   - Action: Select Demo 9 from the Module 2 menu and step through all six parts.

4. Run the full Module 2 test suite
   - Action: Run from the solution root:
     ```powershell
     dotnet test --filter "Module=2&Category=Unit"           # 37 unit tests, no Docker needed
     dotnet test --filter "Module=2&Category=Integration"    # 3 integration tests, requires Docker
     ```
   - Expected: 37 unit tests pass, 0 fail. 3 integration tests pass, 0 fail.
   - Individual clip checks (unit tests):
     ```powershell
     dotnet test --filter "Module=2&Clip=2&Category=Unit"   #  4 tests — read-only collection + Order.Place
     dotnet test --filter "Module=2&Clip=3&Category=Unit"   # 10 tests — private setters + state-machine transitions
     dotnet test --filter "Module=2&Clip=4&Category=Unit"   #  8 tests — Money value object + Order.Total type
     dotnet test --filter "Module=2&Clip=8&Category=Unit"   # 15 tests — guard clauses
     ```
   - Individual clip checks (integration tests, requires Docker):
     ```powershell
     dotnet test --filter "Module=2&Clip=2&Category=Integration"   # 2 tests — read-only round-trip + HasField round-trip
     dotnet test --filter "Module=2&Clip=5&Category=Integration"   # 1 test — shadow FK round-trip
     ```
