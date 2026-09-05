using AnemicOrder = AppModels.Order;
using ConsoleHelpers;
using OrderManagement.Domain.Aggregates;
using OrderManagement.Domain.ValueObjects;

namespace ConsoleAppProject.CodeAndDemonstrations;

public static class DemonstrateAnemicVsDomainModel
{
    public static async Task ContrastModelsAsync()
    {
        var anemicOrder = new AnemicOrder
        {
            Id = 42,
            Status = "Banana",
            Total = -99.99m,
            OrderDate = DateTime.UtcNow,
            CustomerName = "Test Customer"
        };

        OutputHelpers.WriteColored(OutputHelpers.SectionBanner("ANEMIC — any state accepted"), ConsoleColor.DarkBlue);
        Console.Write(OutputHelpers.BoxedArrayWithTitle(
            "Anemic Order — No Invariants (any state accepted)",
            new[]
            {
                $"Id:           {anemicOrder.Id}",
                $"Status:       {anemicOrder.Status}   <-- completely invalid, no guard",
                $"Total:        {anemicOrder.Total:C}  <-- negative amount, no exception",
                $"CustomerName: {anemicOrder.CustomerName}",
                $"OrderDate:    {anemicOrder.OrderDate:u}"
            }
        ));

        //----------------------------------------------------------------//
        Console.WriteLine();
        InputHelpers.WaitForUserInput(ConsoleColor.DarkYellow);
        Console.WriteLine();
        //----------------------------------------------------------------//

        var validOrder = Order.Place(
            Random.Shared.Next(1, int.MaxValue),
            new[] { (Random.Shared.Next(1, int.MaxValue), 1, Money.Create(10m, "USD")) }
        );

        OutputHelpers.WriteColored(OutputHelpers.SectionBanner("DOMAIN — valid path through factory"), ConsoleColor.DarkBlue);
        Console.Write(OutputHelpers.BoxedArrayWithTitle(
            "Domain Order — Valid Construction Path (full guards in Clip 08)",
            new[]
            {
                $"Id:         {validOrder.Id}",
                $"Status:     {validOrder.Status}",
                $"PlacedAt:   {validOrder.PlacedAt:u}",
                $"Lines:      {validOrder.Lines.Count}",
                $"Total:      {validOrder.Total.Amount:C}  <-- always 0 until Clip 8 adds the calculation"
            }
        ));

        await Task.CompletedTask;
    }
}
