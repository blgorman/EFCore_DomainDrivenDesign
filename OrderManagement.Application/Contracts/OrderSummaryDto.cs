namespace OrderManagement.Application.Contracts;

public record OrderSummaryDto(
    int OrderId,
    int CustomerId,
    string Status,
    DateTime PlacedAt,
    int LineCount);
