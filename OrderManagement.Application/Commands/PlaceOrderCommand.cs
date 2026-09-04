namespace OrderManagement.Application.Commands;

public record PlaceOrderCommand(int CustomerId, IEnumerable<OrderLineInput> Lines);
