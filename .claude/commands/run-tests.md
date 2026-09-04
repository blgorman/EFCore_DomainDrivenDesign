---
description: Run this repository's tests filtered to one module and clip — builds the xUnit trait filter from the numbers given, runs it, and reports the real pass and fail counts without fixing anything.
argument-hint: [Module NN] [Clip NN] [unit|integration]
---

# Run Tests

You are running this repository's tests on behalf of a student working through the course, scoped to the module and clip they name.

Your job is to **build the right filter, run it, and report what actually happened.** A failing test is information the student needs, not a problem for you to solve. You do not edit test code, you do not edit product code, and you do not make a red test go green.

---

## Invocation

The student names a module, optionally a clip, and optionally a category, in any of these shapes:

- "/run-tests Module 2 Clip 4"
- "/run-tests 2 4"
- "/run-tests 6" — whole module
- "/run-tests" — everything
- "/run-tests 2 4 unit" — that clip's unit tests only
- "/run-tests 5 integration"

Numbers are read in order: the first is the module, the second is the clip. The words `unit` and `integration` are case-insensitive and may appear anywhere in the arguments.

**A clip number without a module number is not a valid request.** `Clip` is not unique on its own — Clip 4 exists in Module 2 and in Module 4, and they are unrelated tests. Ask which module.

---

## Building the filter

The test project carries three xUnit traits. `Category` and `Module` are declared on the test class; `Clip` is declared on each individual test method.

```csharp
[Trait("Category", "Unit")]   // or "Integration"
[Trait("Module", "2")]
public class OrderTests
{
    [Fact]
    [Trait("Clip", "3")]
    public void Order_Status_Has_Private_Setter() { ... }
}
```

Compose the `--filter` expression from exactly what the student gave you:

| Arguments | Filter |
|---|---|
| module + clip | `Module=2&Clip=4` |
| module only | `Module=2` |
| nothing | no `--filter` argument at all |
| any of the above + `unit` | append `&Category=Unit` |
| any of the above + `integration` | append `&Category=Integration` |

**A clip number means that clip and no other.** `/run-tests 2 4` runs the Module 2 Clip 4 tests only — it does not include Clips 2 and 3. If the student wants an earlier clip too, they will ask for it.

Then run:

```bash
dotnet test OrderManagement.Tests/OrderManagement.Tests.csproj --filter "Module=2&Clip=4"
```

Always name the test project explicitly. Always quote the filter expression — `&` is a shell operator in every shell this repository is used from.

---

## What tests exist

Run the filter the student asked for even when this table says it will match nothing — the table is for explaining a zero-match result, not for talking the student out of a request.

| Module | Unit test clips | Integration test clips |
|---|---|---|
| 2 | 2, 3, 4, 8 | 2, 5 |
| 3 | — | 5 |
| 4 | — | 2, 4 |
| 5 | — | 6 |
| 6 | 5, 6 | — |

Modules 1 and 7 have no tests at all.

Two Module 6 Clip 7 tests ship commented out in `OrderManagement.Tests/Application/SpecificationTests.cs`, under `//TODO: Module 6 Clip 7` markers. They are student exercises. `/run-tests 6 7` matches zero tests until the student uncomments them, and that is the correct starter behavior — **never uncomment them to produce a result.**

---

## Zero matches is a result, not an error

`dotnet test` reports success when a filter matches nothing. Read the actual test counts in the output, and if zero tests ran, say so in plain words:

> The filter `Module=3&Clip=2` matched no tests. Module 3's only tests are tagged Clip 5.

Never let a zero-match run be reported as tests passing.

---

## Environment conditions that are not test failures

- **Integration tests require Docker.** `MsSqlFixture` starts a SQL Server container through Testcontainers. If Docker is not running, the fixture throws before any assertion executes. Report that as an environment condition and name it — the student needs to start Docker, not change their code.
- **On ARM64, the fixture already handles the image.** `MsSqlFixture.BuildContainer` substitutes `mcr.microsoft.com/azure-sql-edge` on ARM hardware because the official SQL Server image has no ARM build. No student action is needed, and this is not something to change.
- **`MSB3027` / `MSB3021` file-lock errors are not test failures.** They mean a console instance is still running and holding an output file. Close it and run again. Only an actual `error CS####` is a compile failure, and only a reported failed test count is a test failure.

---

## On a failing test, report — do not fix

A red test is the answer to the student's question. Leave it red.

Do not edit the test, do not edit the code under test, do not adjust an assertion, and do not re-run with a narrower filter to get a clean result. If a test fails, report:

1. **The failing test's full name**, and the full repository-relative path with the line number the failure points to, for example `OrderManagement.Tests/Domain/OrderTests.cs:89`.
2. **The assertion output, quoted** from the runner rather than paraphrased.
3. **Which clip's work the test covers**, read from its `Clip` trait, so the student knows where to look.

If several tests fail for one cause, say that once and name the cause, rather than listing each failure separately.

Never describe a test as passing or failing without having run it. If you did not run something, say "unverified."

---

## Reporting

State the filter you ran, the command verbatim, and the counts you actually observed — passed, failed, skipped, and total. Quote the runner's summary line.

Refer to clips as "Module _N_ Clip _M_", never a bare "Clip 4" and never a bare step or clip number on its own, because a bare number is ambiguous across modules.

---

## Scope

Answer questions about what the run showed. Change nothing. **Never commit and never push** — this skill only reads and runs.
