using AppModels;
using ConsoleHelpers;

namespace ConsoleAppProject.CodeAndDemonstrations;

public class DemonstrateAnemicModel
{
    public static async Task ShowAnemicOrderAsync()
    {
        var order = new Order
        {
            Id = 1,
            Status = "Banana",
            Total = -99.99m,
            OrderDate = DateTime.Now,
            CustomerName = "Test Customer"
        };

        OutputHelpers.WriteColored(OutputHelpers.SectionBanner("ANEMIC MODEL DEMO"), ConsoleColor.DarkBlue);
        Console.Write(OutputHelpers.BoxedArrayWithTitle(
            "Anemic Order — No Invariants",
            new[]
            {
                $"Id:           {order.Id}",
                $"Customer:     {order.CustomerName}",
                $"Status:       {order.Status}   <-- not a real status, no guard",
                $"Total:        {order.Total:C}  <-- negative price, no exception",
                $"Date:         {order.OrderDate:d}",
                "",
                "No guard. No validation. Any caller can put this in an invalid state.",
                "In Module 2 we replace this with a domain model that prevents this."
            }
        ));

        await Task.CompletedTask;
    }
}
