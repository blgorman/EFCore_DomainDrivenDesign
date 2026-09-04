# Clip08 - Testing Repositories with Testcontainers
Runs `dotnet test` against `OrderRepositoryTests.cs`, backed by a real SQL Server instance spun up per test run via Testcontainers.

## Demo Class
`DemonstrateRepositoryTesting.ShowTestingApproachAsync()` in `ConsoleAppProject/CodeAndDemonstrations/Module04/`.

Run it from Module 4 → Option 8 — Testing Repositories with Testcontainers. The demo describes the fixture, the two tests, and the command to run. The tests themselves run from `OrderManagement.Tests` with `dotnet test`, not from the console app.

## Tests
- OrderManagement.Tests/Integration/OrderRepositoryTests.cs
- OrderManagement.Tests/Integration/MsSqlFixture.cs

This clip requires having completed Clip 4 (`OrderRepository`'s `Add`, `Remove`, `SaveAsync`, and `GetByIdAsync` must already be implemented) — the tests already exist, and `Add_ThenGetById_ReturnsOrderWithLines` fails until then.

## What To Verify
- `dotnet test --filter Module=4` starts a real SQL Server container via `MsSqlFixture`, runs `Database.MigrateAsync()` against it, and reports all `OrderRepositoryTests` passing.
- `Add_ThenGetById_ReturnsOrderWithLines` confirms the saved order reloads with its line and correct `UnitPrice`.
- `Remove_ThenGetById_ReturnsNull` confirms a removed-and-saved order is gone on reload.
- `MsSqlFixture` is shared across the `"MsSql"` test collection — one container per test run, not one per test.

## Student Changes
None. No code changes for this clip — `OrderRepositoryTests.cs` and `MsSqlFixture.cs` are already written and require no edits. Completing Clip 4 is what turns these tests from failing to passing.
