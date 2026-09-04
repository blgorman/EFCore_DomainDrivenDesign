using ConsoleHelpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.DependencyInjection;
using OrderManagement.Infrastructure.Data;

namespace ConsoleAppProject.CodeAndDemonstrations;

public static class DemonstrateFluentConfiguration
{
    // See OrderManagement.Infrastructure/Configurations folder for all the IEntityTypeConfiguration implementations.
    // See OrderManagement.Domain/Aggregates for the aggregate root classes that are being configured.
    // See OrderManagement.Domain/Entities folder for all the entity classes that are being configured.
    public static async Task WalkConfigurationsAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var ctx = scope.ServiceProvider.GetRequiredService<OrderingContext>();

        OutputHelpers.WriteColored(OutputHelpers.SectionBanner("Fluent API Walkthrough"), ConsoleColor.DarkBlue);

        var entityTypes = ctx.Model.GetEntityTypes().ToList();

        foreach (var entityType in entityTypes.Where(e => !e.IsOwned()))
        {
            var shortName = entityType.ClrType?.Name ?? entityType.Name;
            var tableName = entityType.GetTableName() ?? "[owned / no table]";
            var allProps = entityType.GetProperties().ToList();
            var shadowProps = allProps.Where(p => p.IsShadowProperty()).ToList();

            var propertyLines = allProps
                .Select(p =>
                {
                    var tag = p.IsShadowProperty() ? " [shadow]" : "";
                    var colName = p.GetColumnName();
                    return $"  {p.Name} -> {colName}{tag}";
                })
                .ToArray();

            var summaryLines = new List<string>
            {
                $"Entity:        {shortName}",
                $"Table:         {tableName}",
                $"Properties:    {allProps.Count}  (shadow: {shadowProps.Count})",
                "---"
            };
            summaryLines.AddRange(propertyLines);


            OutputHelpers.WriteColored(OutputHelpers.SectionBanner(shortName), ConsoleColor.DarkBlue);
            Console.Write(OutputHelpers.BoxedArrayWithTitle(
                $"Entity: {shortName}",
                summaryLines.ToArray()
            ));

            //--------------------------------------------------------
            InputHelpers.WaitForUserInput(ConsoleColor.DarkYellow);
            //--------------------------------------------------------

        }

        await Task.CompletedTask;
    }

    public static async Task HighlightFluentConfigurationAsync(IServiceProvider serviceProvider)
    {
        await WalkConfigurationsAsync(serviceProvider);

        using var scope = serviceProvider.CreateScope();
        var ctx = scope.ServiceProvider.GetRequiredService<OrderingContext>();

        OutputHelpers.WriteColored(OutputHelpers.SectionBanner("Fluent API — Configuration Walkthrough"), ConsoleColor.DarkBlue);

        var entityTypes = ctx.Model.GetEntityTypes().ToList();
        var ownedTypes = entityTypes.Where(e => e.IsOwned()).ToList();

        // 1. Owned types — OwnsOne maps a value object into the owner's table
        OutputHelpers.WriteColored(OutputHelpers.SectionBanner("OwnsOne maps a value object into the owner's table"), ConsoleColor.DarkBlue);

        var lines = new List<string>
        {
            "Value objects mapped with OwnsOne have no dedicated table.",
            "Their columns are folded into the owner's table (see section 2).",
            "---"
        };
        foreach (var et in ownedTypes)
        {
            var ownerName = et.FindOwnership()?.PrincipalEntityType.ClrType?.Name ?? "?";
            lines.Add($"  {et.ClrType?.Name ?? et.Name}  =>  owned by {ownerName}  [no table]");
        }
 
        Console.Write(OutputHelpers.BoxedListWithTitle(
                $"1) Owned Types  [builder.OwnsOne(...)]"
                , lines));

        //--------------------------------------------------------
        InputHelpers.WaitForUserInput(ConsoleColor.DarkYellow);
        //--------------------------------------------------------

        // 2. Owned type column names — folded into the owner's table with explicit HasColumnName
        OutputHelpers.WriteColored(OutputHelpers.SectionBanner("Owned Type Column Names [HasColumnName]"), ConsoleColor.DarkBlue);

        lines = new List<string>
        {
            "Owned type properties become columns on the owner's table.",
            "The column names below were set explicitly with .HasColumnName(\"...\").",
            "---"
        };
        foreach (var et in ownedTypes)
        {
            var ownership = et.FindOwnership();
            var ownerTable = ownership?.PrincipalEntityType.GetTableName();
            if (ownerTable == null) continue;
            var storeObj = StoreObjectIdentifier.Table(ownerTable, ownership?.PrincipalEntityType.GetSchema());
            foreach (var prop in et.GetProperties())
            {
                var col = prop.GetColumnName(storeObj);
                lines.Add($"  {et.ClrType?.Name ?? et.Name}.{prop.Name}  =>  column \"{col}\"  on [{ownerTable}]");
            }
        }

        Console.Write(OutputHelpers.BoxedListWithTitle(
                        "2) Owned Type Column Mapping  [.HasColumnName(\"...\")]"
                        , lines));
        //--------------------------------------------------------
        InputHelpers.WaitForUserInput(ConsoleColor.DarkYellow);
        //--------------------------------------------------------

        // 3. Backing field navigations — EF bypasses the public property
        OutputHelpers.WriteColored(OutputHelpers.SectionBanner("Backing Field Navigations (bypass the public property)"), ConsoleColor.DarkBlue);

        lines = new List<string>
        {
            "EF Core writes directly to the private backing field, bypassing the public property.",
            "This lets the aggregate encapsulate its collection — callers must use domain methods.",
            "Configured via: builder.Navigation(o => o.Lines).HasField(\"_lines\")",
            "---"
        };
        foreach (var et in entityTypes)
        {
            foreach (var nav in et.GetNavigations())
            {
                var field = nav.GetFieldName();
                if (field != null && field.StartsWith("_"))
                    lines.Add($"  {et.ClrType?.Name ?? et.Name}.{nav.Name}  =>  writes to field: {field}");
            }
        }
        Console.Write(OutputHelpers.BoxedListWithTitle(
                        "3) Backing Field Navigations  [.HasField(\"...\")]"
                        , lines));  

        //--------------------------------------------------------
        InputHelpers.WaitForUserInput(ConsoleColor.DarkYellow);
        //--------------------------------------------------------

        // 4. Explicit vs convention table names
        OutputHelpers.WriteColored(OutputHelpers.SectionBanner("Explicit vs Convention table names"), ConsoleColor.DarkBlue);

        lines = new List<string>
        {
            "By convention, EF uses the DbSet property name when the entity has one, and the class name when it does not.",
            "An explicit .ToTable(\"...\") call overrides this.",
            "---"
        };
        foreach (var et in entityTypes.Where(e => !e.IsOwned()))
        {
            if (et is IConventionEntityType conv)
            {
                var source = conv.GetTableNameConfigurationSource();
                var label = source == ConfigurationSource.Explicit ? "explicit via .ToTable()" : "by convention";
                lines.Add($"  {et.ClrType?.Name ?? et.Name}  =>  \"{et.GetTableName()}\"  ({label})");
            }
        }
        Console.Write(OutputHelpers.BoxedListWithTitle(
                        "4) Table Names: Explicit vs Convention  [.ToTable(\"...\")]"
                        , lines));
        
        await Task.CompletedTask;
    }
}
