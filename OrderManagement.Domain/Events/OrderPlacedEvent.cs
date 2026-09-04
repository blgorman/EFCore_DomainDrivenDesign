namespace OrderManagement.Domain.Events;

public record OrderPlacedEvent(int OrderId, DateTime PlacedAt) : IDomainEvent;
