# Clip08 - Migration Strategy for Multiple Contexts
Give ShippingContext its own migration history table so it stops sharing OrderingContext's default one.

## Demo Class
- DemonstrateMigrationStrategy.ShowMigrationCommandsAsync

## Tests
No unit tests are introduced in this clip.

## Files To Edit
- ConsoleAppProject/Program.cs

## What To Verify
- ShippingContext's registration passes `MigrationsHistoryTable("__EFMigrationsHistory_Shipping")`.
- OrderingContext still uses the default `__EFMigrationsHistory` table (no change needed there).

## Student Changes

### 1. Give ShippingContext its own migration history table
- File: ConsoleAppProject/Program.cs
- Location: `ConfigureServices`, the `AddDbContext<ShippingContext>` registration
- Find TODO marker: `Module 3 Clip 8 — Add MigrationsHistoryTable so ShippingContext tracks its migrations independently from OrderingContext in a separate __EFMigrationsHistory_Shipping table.`
- Action: Replace
  ```cs
  options.UseSqlServer(connectionString));
  ```
  With:
  ```cs
  options.UseSqlServer(connectionString,
      o => o.MigrationsHistoryTable("__EFMigrationsHistory_Shipping")));
  ```
  Delete the TODO comment above it.
