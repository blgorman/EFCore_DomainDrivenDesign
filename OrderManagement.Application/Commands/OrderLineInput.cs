namespace OrderManagement.Application.Commands;

public record OrderLineInput(int ProductId, int Quantity, decimal UnitPrice, string Currency);
