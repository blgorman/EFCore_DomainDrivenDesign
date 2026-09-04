using ConsoleHelpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using OrderManagement.Domain.Aggregates;
using OrderManagement.Domain.ValueObjects;
using OrderManagement.Infrastructure.Data;

namespace ConsoleAppProject.CodeAndDemonstrations;

public static class DemonstrateMultipleContexts
{
    public static async Task RunEndToEndAsync(IServiceProvider serviceProvider)
    {
        // --- Screen 1: DI registration walkthrough ---
        OutputHelpers.WriteColored(OutputHelpers.SectionBanner("Registering Multiple DbContexts in DI"), ConsoleColor.DarkBlue);
        Console.Write(OutputHelpers.BoxedArrayWithTitle(
            "Program.cs — how both contexts are wired up in ConfigureServices",
            new[]
            {
                "var connectionString = config.GetConnectionString(\"ApplicationDbConnection\");",
                "",
                "// Write-side context — carries the domain event interceptor:",
                "services.AddDbContext<OrderingContext>((sp, options) =>",
                "    options.UseSqlServer(connectionString)",
                "           .AddInterceptors(sp.GetRequiredService<DomainEventDispatchInterceptor>()));",
                "",
                "// Read-side context — separate migration history, no interceptor:",
                "services.AddDbContext<ShippingContext>(options =>",
                "    options.UseSqlServer(connectionString,",
                "        o => o.MigrationsHistoryTable(\"__EFMigrationsHistory_Shipping\")));",
                "",
                "Key points:",
                "  Both contexts share the same connection string on purpose.",
                "  They map to the same physical database — different logical models.",
                "  Each is registered as Scoped: new instance per request/scope.",
                "  They resolve independently — DI never confuses one for the other."
            }
        ));

        //----------------------------------------------------------------//
        Console.WriteLine();
        InputHelpers.WaitForUserInput(ConsoleColor.DarkYellow);
        Console.WriteLine();
        //----------------------------------------------------------------//

        // --- Screen 2: What each context "knows" (model configuration) ---
        OutputHelpers.WriteColored(OutputHelpers.SectionBanner("What Each Context Knows — Independent Model Snapshots"), ConsoleColor.DarkBlue);
        Console.Write(OutputHelpers.BoxedArrayWithTitle(
            "OrderingContext.OnModelCreating — the write-side model",
            new[]
            {
                "modelBuilder.ApplyConfiguration(new OrderConfiguration());",
                "  => Maps Order with private _lines backing field, owned Money value object.",
                "  => HasKey(Id), HasMany(Lines).WithOne().HasForeignKey(\"OrderId\")",
                "",
                "modelBuilder.ApplyConfiguration(new OrderLineConfiguration());",
                "  => Maps OrderLine with ProductId, Quantity, UnitPrice (owned Money).",
                "",
                "modelBuilder.ApplyConfiguration(new CustomerConfiguration());",
                "  => Maps Customer with Id, Name, Email.",
                "",
                "Total entity types:  Order, OrderLine, Customer, Money (owned)"
            }
        ));

        //----------------------------------------------------------------//
        Console.WriteLine();
        InputHelpers.WaitForUserInput(ConsoleColor.DarkYellow);
        Console.WriteLine();
        //----------------------------------------------------------------//

        Console.Write(OutputHelpers.BoxedArrayWithTitle(
            "ShippingContext.OnModelCreating — the read-side model",
            new[]
            {
                "modelBuilder.ApplyConfiguration(new ShipmentReadModelConfiguration());",
                "  => builder.ToTable(\"Orders\", t => t.ExcludeFromMigrations());",
                "  => Maps ONLY: Id, Status, PlacedAt.",
                "  => No OrderLines, no Customer, no Money, no backing fields.",
                "",
                "Total entity types:  ShipmentReadModel (lean projection only)",
                "",
                "ExcludeFromMigrations = ShippingContext will NEVER generate a migration",
                "for this table. OrderingContext owns the schema. Shipping just reads it."
            }
        ));

        //----------------------------------------------------------------//
        Console.WriteLine();
        InputHelpers.WaitForUserInput(ConsoleColor.DarkYellow);
        Console.WriteLine();
        //----------------------------------------------------------------//

        // --- Screen 3: Live proof — model isolation via GetEntityTypes() ---
        using var scope = serviceProvider.CreateScope();
        var orderingCtx = scope.ServiceProvider.GetRequiredService<OrderingContext>();
        var shippingCtx = scope.ServiceProvider.GetRequiredService<ShippingContext>();

        await SeedDataHelper.SeedAsync(orderingCtx);

        var orderingTypes = orderingCtx.Model.GetEntityTypes()
            .Select(t => $"  {t.ClrType.Name,-22}  table: {t.GetTableName() ?? "(owned)"}")
            .ToArray();

        var shippingTypes = shippingCtx.Model.GetEntityTypes()
            .Select(t => $"  {t.ClrType.Name,-22}  table: {t.GetTableName() ?? "(owned)"}")
            .ToArray();

        OutputHelpers.WriteColored(OutputHelpers.SectionBanner("Live Proof — Runtime Model Snapshots Are Independent"), ConsoleColor.DarkBlue);
        Console.Write(OutputHelpers.BoxedArrayWithTitle(
            $"orderingCtx.Model.GetEntityTypes()  ({orderingTypes.Length} types)",
            orderingTypes
        ));

        //----------------------------------------------------------------//
        Console.WriteLine();
        InputHelpers.WaitForUserInput(ConsoleColor.DarkYellow);
        Console.WriteLine();
        //----------------------------------------------------------------//

        Console.Write(OutputHelpers.BoxedArrayWithTitle(
            $"shippingCtx.Model.GetEntityTypes()  ({shippingTypes.Length} types)",
            shippingTypes
        ));

        //----------------------------------------------------------------//
        Console.WriteLine();
        InputHelpers.WaitForUserInput(ConsoleColor.DarkYellow);
        Console.WriteLine();
        //----------------------------------------------------------------//

        Console.Write(OutputHelpers.BoxedArrayWithTitle(
            "The two model snapshots are entirely independent",
            new[]
            {
                $"Same instance?          {ReferenceEquals(orderingCtx, shippingCtx)}",
                $"Same ChangeTracker?     {ReferenceEquals(orderingCtx.ChangeTracker, shippingCtx.ChangeTracker)}",
                $"Same Model?             {ReferenceEquals(orderingCtx.Model, shippingCtx.Model)}",
                "",
                "OrderingContext knows nothing about ShipmentReadModel.",
                "ShippingContext knows nothing about Order, OrderLine, or Customer.",
                "EF Core never mixes their identity maps or change trackers."
            }
        ));

        //----------------------------------------------------------------//
        Console.WriteLine();
        InputHelpers.WaitForUserInput(ConsoleColor.DarkYellow);
        Console.WriteLine();
        //----------------------------------------------------------------//

        // --- Screen 4: Write through OrderingContext ---
        var newOrder = Order.Place(SeedDataHelper.CustomerAId, new[]
        {
            (SeedDataHelper.Product1Id, 2, Money.Create(29.99m, "USD")),
            (SeedDataHelper.Product2Id, 1, Money.Create(49.99m, "USD"))
        });

        var trackedBeforeSave = orderingCtx.ChangeTracker.Entries().Count();
        orderingCtx.Orders.Add(newOrder);
        orderingCtx.Entry(newOrder).Property<int>("CustomerId").CurrentValue = SeedDataHelper.CustomerAId;
        var trackedAfterAdd = orderingCtx.ChangeTracker.Entries().Count();
        await orderingCtx.SaveChangesAsync();
        var trackedAfterSave = orderingCtx.ChangeTracker.Entries().Count();

        OutputHelpers.WriteColored(OutputHelpers.SectionBanner("Live Demo — Writing an Order via OrderingContext"), ConsoleColor.DarkBlue);
        Console.Write(OutputHelpers.BoxedArrayWithTitle(
            "Order.Place(...) tracked and saved via orderingCtx.SaveChangesAsync()",
            new[]
            {
                $"Order Id:          {newOrder.Id}",
                $"Customer Id:       {orderingCtx.Entry(newOrder).Property<int>("CustomerId").CurrentValue}",
                $"Status:            {newOrder.Status}",
                $"Lines:             {newOrder.Lines.Count}",
                $"PlacedAt:          {newOrder.PlacedAt:u}",
                "",
                "-- ChangeTracker activity on OrderingContext --",
                $"  Tracked entries before Add:  {trackedBeforeSave}",
                $"  Tracked entries after Add:   {trackedAfterAdd}  (Order + {newOrder.Lines.Count} OrderLine(s))",
                $"  Tracked entries after Save:  {trackedAfterSave}  (Unchanged — clean state post-commit)",
                "",
                "ShippingContext.ChangeTracker was untouched throughout this write.",
                "The order we just saved exists in the database ready to be read."
            }
        ));

        //----------------------------------------------------------------//
        Console.WriteLine();
        InputHelpers.WaitForUserInput(ConsoleColor.DarkYellow);
        Console.WriteLine();
        //----------------------------------------------------------------//

        //TODO: Module 3 Clip 7 — Step 1: Delete this entire "Not Yet Implemented" box
        OutputHelpers.WriteColored(OutputHelpers.SectionBanner("Live Demo — Reading the Same Row via ShippingContext"), ConsoleColor.DarkBlue);
        Console.Write(OutputHelpers.BoxedArrayWithTitle(
            "Not Yet Implemented",
            new[]
            {
                "This screen will work after completing Module 3 Clip 7.",
                "Open DemonstrateMultipleContexts.cs and uncomment the //TODO: Module 3 Clip 7 Screen 5 block.",
                "Prerequisite: ShippingContext must have DbSet<ShipmentReadModel> and ApplyConfiguration set up (added in Clip 4)."
            }
        ));

        //TODO: Module 3 Clip 7 — Step 2: Uncomment this block (requires having previously completed Clip 4).
        //// --- Screen 5: Read back through ShippingContext ---
        //var shippingTrackedBefore = shippingCtx.ChangeTracker.Entries().Count();
        //var shipment = await shippingCtx.Shipments
        //    .FirstOrDefaultAsync(s => s.Id == newOrder.Id);
        //var shippingTrackedAfter = shippingCtx.ChangeTracker.Entries().Count();
        //
        //OutputHelpers.WriteColored(OutputHelpers.SectionBanner("Live Demo — Reading the Same Row via ShippingContext"), ConsoleColor.DarkBlue);
        //
        //if (shipment is not null)
        //{
        //    Console.Write(OutputHelpers.BoxedArrayWithTitle(
        //        "shippingCtx.Shipments.FirstOrDefaultAsync(s => s.Id == newOrder.Id)",
        //        new[]
        //        {
        //            $"Shipment Id:   {shipment.Id}",
        //            $"Status:        {shipment.Status}",
        //            $"PlacedAt:      {shipment.PlacedAt:u}",
        //            "",
        //            "-- Side-by-side comparison --",
        //            $"  newOrder.Id  == shipment.Id  :  {newOrder.Id == shipment.Id}",
        //            $"  newOrder.Status.ToString()   :  {newOrder.Status}",
        //            $"  shipment.Status              :  {shipment.Status}",
        //            "",
        //            "ShipmentReadModel has NO Lines property — the column data for OrderLines",
        //            "exists in the database but ShippingContext never puts it in its model.",
        //            "",
        //            $"  ShippingContext tracked entries before query:  {shippingTrackedBefore}",
        //            $"  ShippingContext tracked entries after query:   {shippingTrackedAfter}",
        //            "",
        //            "OrderingContext still tracks the Order from the write above.",
        //            "ShippingContext tracks only its ShipmentReadModel — completely separate identity maps."
        //        }
        //    ));
        //}
        //
        //----------------------------------------------------------------//
        Console.WriteLine();
        InputHelpers.WaitForUserInput(ConsoleColor.DarkYellow);
        Console.WriteLine();
        //----------------------------------------------------------------//

        //TODO: Module 3 Clip 7 — Step 3: Delete this entire "Not Yet Implemented" box
        OutputHelpers.WriteColored(OutputHelpers.SectionBanner("Status Change Propagates Between Contexts via the Database"), ConsoleColor.DarkBlue);
        Console.Write(OutputHelpers.BoxedArrayWithTitle(
            "Not Yet Implemented",
            new[]
            {
                "This screen will work after completing Module 3 Clip 7.",
                "Open DemonstrateMultipleContexts.cs and uncomment the //TODO: Module 3 Clip 7 Screen 6 block.",
                "Prerequisite: ShippingContext must have DbSet<ShipmentReadModel> and ApplyConfiguration set up (added in Clip 4)."
            }
        ));

        //TODO: Module 3 Clip 7 — Step 4: Uncomment this block (requires having previously completed Clip 4).
        //// --- Screen 6: Ship the order via OrderingContext, verify Shipping sees new status ---
        //var reloadedOrder = await orderingCtx.Orders
        //    .Include(o => o.Lines)
        //    .FirstAsync(o => o.Id == newOrder.Id);
        //
        //reloadedOrder.Process();
        //reloadedOrder.Confirm();
        //reloadedOrder.Ship();
        //await orderingCtx.SaveChangesAsync();
        //
        //// ShippingContext must re-query — its identity map has the old stale entry
        //shippingCtx.ChangeTracker.Clear();
        //var shippedShipment = await shippingCtx.Shipments
        //    .AsNoTracking()
        //    .FirstOrDefaultAsync(s => s.Id == newOrder.Id);
        //
        //OutputHelpers.WriteColored(OutputHelpers.SectionBanner("Status Change Propagates Between Contexts via the Database"), ConsoleColor.DarkBlue);
        //Console.Write(OutputHelpers.BoxedArrayWithTitle(
        //    "OrderingContext ships the order; ShippingContext re-queries to see the new status",
        //    new[]
        //    {
        //        "-- OrderingContext --",
        //        $"  Reloaded order and called order.Ship()",
        //        $"  Status after Ship():   {reloadedOrder.Status}",
        //        $"  Saved via orderingCtx.SaveChangesAsync()",
        //        "",
        //        "-- ShippingContext re-query (AsNoTracking, ChangeTracker.Clear) --",
        //        $"  {(shippedShipment is null ? "Not found" : $"Shipment Status: {shippedShipment.Status}")}",
        //        "",
        //        "There is no in-process notification from one context to the other.",
        //        "ShippingContext sees the change because both read from the same database.",
        //        "The domain boundary is enforced in code, not in the storage layer.",
        //        "",
        //        "This is intentional: contexts communicate through the database,",
        //        "not through shared in-memory references."
        //    }
        //));
        //
        
        //----------------------------------------------------------------//
        Console.WriteLine();
        InputHelpers.WaitForUserInput(ConsoleColor.DarkYellow);
        Console.WriteLine();
        //----------------------------------------------------------------//

        // --- Screen 7: Migration isolation ---
        OutputHelpers.WriteColored(OutputHelpers.SectionBanner("Migration Isolation — Each Context Has Its Own History"), ConsoleColor.DarkBlue);
        Console.Write(OutputHelpers.BoxedArrayWithTitle(
            "Running migrations for each context independently",
            new[]
            {
                "// Add a migration for OrderingContext (owns the schema):",
                "dotnet ef migrations add AddOrderIndex \\",
                "    --project OrderManagement.Infrastructure \\",
                "    --startup-project ConsoleAppProject \\",
                "    --context OrderingContext \\",
                "    --output-dir Migrations/Ordering",
                "",
                "// Add a migration for ShippingContext (only ever adds its own tables):",
                "dotnet ef migrations add InitShipping \\",
                "    --project OrderManagement.Infrastructure \\",
                "    --startup-project ConsoleAppProject \\",
                "    --context ShippingContext \\",
                "    --output-dir Migrations/Shipping",
                "",
                "// Apply independently:",
                "dotnet ef database update --context OrderingContext",
                "dotnet ef database update --context ShippingContext"
            }
        ));

        //----------------------------------------------------------------//
        Console.WriteLine();
        InputHelpers.WaitForUserInput(ConsoleColor.DarkYellow);
        Console.WriteLine();
        //----------------------------------------------------------------//

        Console.Write(OutputHelpers.BoxedArrayWithTitle(
            "Migration history tables in the database",
            new[]
            {
                "OrderingContext  =>  __EFMigrationsHistory            (default table)",
                "ShippingContext  =>  __EFMigrationsHistory_Shipping   (configured via MigrationsHistoryTable)",
                "",
                "Why separate history tables?",
                "  EF Core uses the history table to decide which migrations have run.",
                "  If both contexts share a table, EF cannot distinguish whose migration is whose.",
                "  Each context needs its own history so they can evolve independently.",
                "",
                "ShipmentReadModelConfiguration uses ExcludeFromMigrations() on Orders.",
                "This means ShippingContext never generates a migration for the Orders table.",
                "That table belongs to OrderingContext — only OrderingContext can change its schema."
            }
        ));

        //----------------------------------------------------------------//
        Console.WriteLine();
        InputHelpers.WaitForUserInput(ConsoleColor.DarkYellow);
        Console.WriteLine();
        //----------------------------------------------------------------//

        // --- Screen 8: Summary ---
        OutputHelpers.WriteColored(OutputHelpers.SectionBanner("Summary — Multiple DbContexts in Practice"), ConsoleColor.DarkBlue);
        Console.Write(OutputHelpers.BoxedArrayWithTitle(
            "What dual-context DI registration gives you",
            new[]
            {
                "ISOLATION",
                "  Each context has a separate model, ChangeTracker, and identity map.",
                "  Entities from one context cannot accidentally be attached to the other.",
                "  ReferenceEquals(orderingCtx, shippingCtx)  =>  false — always.",
                "",
                "INDEPENDENT SCHEMA OWNERSHIP",
                "  OrderingContext owns and migrates the Orders table.",
                "  ShippingContext opts out of migrations for that table (ExcludeFromMigrations).",
                "  Each context can add its own tables without touching the other's history.",
                "",
                "SHARED PHYSICAL DATABASE — BY DESIGN",
                "  Both contexts read from the same database file.",
                "  Status changes written by OrderingContext are visible to ShippingContext",
                "  on the next query — no event bus required inside a single process.",
                "",
                "REGISTRATION PATTERN",
                "  AddDbContext<OrderingContext>(...) — include domain event interceptor.",
                "  AddDbContext<ShippingContext>(...) — configure separate history table.",
                "  Inject by concrete type; never share a DbContext instance across contexts.",
                "",
                "BEST PRACTICE",
                "  Keep one context per bounded context.",
                "  Never make one context aware of another context's types.",
                "  Let the domain shape the model — not the other way around."
            }
        ));
    }
}
