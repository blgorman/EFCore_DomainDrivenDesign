namespace OrderManagement.Domain.Events;

public record OrderShippedEvent(int OrderId, DateTime ShippedAt) : IDomainEvent;
